using System.Collections.Generic;
using System.Text;
using Multiplayer.API;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Fortified;

/// <summary>
/// 掛在 Pawn（主要是 CompDrone 無人機）上，讓倒地或死亡的個體可以被殖民者原地拆解成資源。
/// 標記方式走 DesignationDef（FFF_Recycle），所以取消標記可以直接用原版的「取消」指定器；
/// 屍體是另一個 Thing，Comp 仍然掛在裡面的 Pawn 上，透過 <see cref="CompRecycleable.Get"/> 取得。
/// </summary>
public class CompProperties_Recycleable : CompProperties
{
    /// <summary>拆解產物。留空時改用 race 本身的 butcherProducts。</summary>
    public List<ThingDefCountClass> products;

    /// <summary>基礎產出倍率。</summary>
    public float yieldFactor = 1f;

    /// <summary>已死亡（屍體）狀態下額外乘上的倍率。</summary>
    public float deadYieldFactor = 0.5f;

    /// <summary>倒地時是否依剩餘生命值比例縮放產出。</summary>
    public bool scaleByHealth = false;

    /// <summary>產出再乘上工作者的這個 stat（例如 ButcheryMechanoidEfficiency）。null = 不乘。</summary>
    public StatDef efficiencyStat;

    /// <summary>基礎工作量（tick），會除以工作者的 workSpeedStat。</summary>
    public int workTicks = 600;

    /// <summary>工作速度 stat。null = 固定 workTicks。</summary>
    public StatDef workSpeedStat;

    public bool allowDowned = true;

    public bool allowDead = true;

    /// <summary>拆解前先把裝備／衣物／物品欄丟到地上，而不是隨機體一起消失。</summary>
    public bool dropEquipment = true;

    /// <summary>
    /// 只允許拆解玩家陣營的個體。
    /// false 時：屍體不限陣營；倒地個體限玩家、無陣營或敵對陣營（不能拆友軍的）。
    /// </summary>
    public bool playerFactionOnly = false;

    /// <summary>工作中的 effecter。null = 原版 ConstructMetal。</summary>
    public EffecterDef workEffecter;

    public SoundDef finishSound;

    [NoTranslate]
    public string gizmoIconPath = "UI/Designators/Deconstruct";

    public CompProperties_Recycleable()
    {
        compClass = typeof(CompRecycleable);
    }

    public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
    {
        foreach (string e in base.ConfigErrors(parentDef))
        {
            yield return e;
        }
        if (parentDef.thingClass != null && !typeof(Pawn).IsAssignableFrom(parentDef.thingClass))
        {
            yield return $"{parentDef.defName}: CompProperties_Recycleable only works on Pawn.";
        }
        if (!allowDowned && !allowDead)
        {
            yield return $"{parentDef.defName}: CompProperties_Recycleable has both allowDowned and allowDead disabled.";
        }
        if (workTicks <= 0)
        {
            yield return $"{parentDef.defName}: CompProperties_Recycleable workTicks must be > 0.";
        }
    }
}

public class CompRecycleable : ThingComp
{
    // 換圖／死亡過程中暫存的標記：DeSpawn 時原版不會清 designation，
    // 這裡先收起來，等重新生成或屍體落地後再補回去。
    private bool pendingDesignation;

    private Texture2D cachedIcon;

    public CompProperties_Recycleable Props => (CompProperties_Recycleable)props;

    public Pawn Pawn => parent as Pawn;

    /// <summary>目前代表這個個體的地圖物件：活著是 pawn 本身，死後是屍體（可能為 null）。</summary>
    public Thing TargetThing
    {
        get
        {
            Pawn p = Pawn;
            if (p == null) return null;
            return p.Dead ? p.Corpse : p;
        }
    }

    private Texture2D Icon => cachedIcon ??= ContentFinder<Texture2D>.Get(Props.gizmoIconPath, reportFailure: false) ?? BaseContent.BadTex;

    /// <summary>不論點到的是 pawn 還是屍體都能拿到 comp。</summary>
    public static CompRecycleable Get(Thing t)
    {
        if (t is Corpse c)
        {
            return c.InnerPawn?.TryGetComp<CompRecycleable>();
        }
        return t?.TryGetComp<CompRecycleable>();
    }

    public bool Designated
    {
        get
        {
            Thing t = TargetThing;
            if (t == null || !t.Spawned) return false;
            return t.Map.designationManager.DesignationOn(t, FFF_DefOf.FFF_RecycleDesignation) != null;
        }
    }

    /// <summary>
    /// 目前狀態是否允許拆解（不含可達性／預約）。
    /// 站著的活體回傳無理由的 false，右鍵選單就不會多出一條沒用的灰字。
    /// </summary>
    public AcceptanceReport CanRecycle()
    {
        Pawn p = Pawn;
        Thing t = TargetThing;
        if (p == null || t == null || !t.Spawned) return false;

        if (p.Dead)
        {
            if (!Props.allowDead) return false;
            if (Props.playerFactionOnly && p.Faction != Faction.OfPlayer) return "FFF.Recycle.NotOurs".Translate();
        }
        else
        {
            if (!Props.allowDowned || !p.Downed) return false;
            if (Props.playerFactionOnly && p.Faction != Faction.OfPlayer) return "FFF.Recycle.NotOurs".Translate();
            // 倒地的友軍單位不給拆，避免順手把盟友的東西拆了。
            if (p.Faction != null && p.Faction != Faction.OfPlayer && !p.Faction.HostileTo(Faction.OfPlayer))
            {
                return "FFF.Recycle.NotOurs".Translate();
            }
        }
        return true;
    }

    #region 標記

    [SyncMethod]
    public static void SetDesignated(Thing target, bool value)
    {
        if (target == null || !target.Spawned) return;
        DesignationManager dm = target.Map.designationManager;
        Designation existing = dm.DesignationOn(target, FFF_DefOf.FFF_RecycleDesignation);
        if (value && existing == null)
        {
            dm.AddDesignation(new Designation(target, FFF_DefOf.FFF_RecycleDesignation));
        }
        else if (!value && existing != null)
        {
            dm.RemoveDesignation(existing);
        }
    }

    public IEnumerable<Gizmo> RecycleGizmos()
    {
        if (!CanRecycle().Accepted) yield break;
        Thing t = TargetThing;
        yield return new Command_Toggle
        {
            defaultLabel = "FFF.Recycle.Gizmo".Translate(),
            defaultDesc = "FFF.Recycle.GizmoDesc".Translate(ProductsSummary(null)),
            icon = Icon,
            isActive = () => Designated,
            toggleAction = () => SetDesignated(t, !Designated)
        };
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        foreach (Gizmo g in base.CompGetGizmosExtra())
        {
            yield return g;
        }
        // 死後由 Patch_Corpse_GetGizmos_Recycle 從屍體那邊接手。
        if (Pawn != null && !Pawn.Dead)
        {
            foreach (Gizmo g in RecycleGizmos())
            {
                yield return g;
            }
        }
    }

    public override string CompInspectStringExtra()
    {
        if (Pawn != null && !Pawn.Dead && Designated)
        {
            return "FFF.Recycle.Marked".Translate();
        }
        return null;
    }

    public override void PostDeSpawn(Map map, DestroyMode mode)
    {
        base.PostDeSpawn(map, mode);
        if (map == null) return;
        Designation d = map.designationManager.DesignationOn(parent, FFF_DefOf.FFF_RecycleDesignation);
        if (d != null)
        {
            pendingDesignation = true;
            map.designationManager.RemoveDesignation(d);
        }
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        if (respawningAfterLoad || !pendingDesignation) return;
        pendingDesignation = false;
        if (CanRecycle().Accepted)
        {
            SetDesignated(parent, true);
        }
    }

    public override void Notify_Killed(Map prevMap, DamageInfo? dinfo = null)
    {
        base.Notify_Killed(prevMap, dinfo);
        if (!pendingDesignation) return;
        pendingDesignation = false;
        // Kill 流程裡 pawn 先 DeSpawn、屍體落地、然後才 Destroy 並通知到這裡，
        // 所以此時 Corpse 已經在地圖上，可以直接把標記接過去。
        Corpse corpse = Pawn?.Corpse;
        if (corpse != null && corpse.Spawned && Props.allowDead)
        {
            SetDesignated(corpse, true);
        }
    }

    #endregion

    #region 產出

    public List<ThingDefCountClass> BaseProducts => !Props.products.NullOrEmpty() ? Props.products : parent.def.butcherProducts;

    public float YieldFactor(Pawn worker)
    {
        float f = Props.yieldFactor;
        Pawn p = Pawn;
        if (p != null)
        {
            if (p.Dead)
            {
                f *= Props.deadYieldFactor;
            }
            else if (Props.scaleByHealth)
            {
                f *= Mathf.Clamp01(p.health.summaryHealth.SummaryHealthPercent);
            }
        }
        if (worker != null && Props.efficiencyStat != null)
        {
            f *= worker.GetStatValue(Props.efficiencyStat);
        }
        return Mathf.Max(0f, f);
    }

    public int WorkTicksFor(Pawn worker)
    {
        float ticks = Props.workTicks;
        if (worker != null && Props.workSpeedStat != null)
        {
            ticks /= Mathf.Max(0.1f, worker.GetStatValue(Props.workSpeedStat));
        }
        return Mathf.Max(1, Mathf.RoundToInt(ticks));
    }

    /// <summary>給 UI 用的預估產出（四捨五入，不含隨機）。</summary>
    public string ProductsSummary(Pawn worker)
    {
        List<ThingDefCountClass> products = BaseProducts;
        if (products.NullOrEmpty()) return "FFF.Recycle.NoProducts".Translate();
        float factor = YieldFactor(worker);
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < products.Count; i++)
        {
            ThingDefCountClass c = products[i];
            if (c?.thingDef == null) continue;
            int count = Mathf.RoundToInt(c.count * factor);
            if (count <= 0) continue;
            if (sb.Length > 0) sb.Append(", ");
            sb.Append(c.thingDef.label).Append(" x").Append(count);
        }
        return sb.Length > 0 ? sb.ToString() : "FFF.Recycle.NoProducts".Translate();
    }

    /// <summary>執行拆解：丟裝備 → 生成產物 → 移除本體。由 JobDriver_Recycle 在工作完成時呼叫。</summary>
    public void Recycle(Pawn worker)
    {
        Pawn p = Pawn;
        Thing t = TargetThing;
        if (p == null || t == null || !t.Spawned) return;

        Map map = t.Map;
        IntVec3 pos = t.Position;
        float factor = YieldFactor(worker);

        if (Props.dropEquipment)
        {
            p.equipment?.DropAllEquipment(pos, forbid: false);
            p.apparel?.DropAll(pos, forbid: false);
            p.inventory?.DropAllNearPawn(pos);
        }

        List<ThingDefCountClass> products = BaseProducts;
        if (products != null)
        {
            for (int i = 0; i < products.Count; i++)
            {
                ThingDefCountClass c = products[i];
                if (c?.thingDef == null) continue;
                if (c.IsChanceBased && !Rand.Chance(c.DropChance)) continue;
                int count = GenMath.RoundRandom(c.count * factor);
                while (count > 0)
                {
                    ThingDef stuff = c.stuff ?? (c.thingDef.MadeFromStuff ? GenStuff.DefaultStuffFor(c.thingDef) : null);
                    Thing thing = ThingMaker.MakeThing(c.thingDef, stuff);
                    int n = Mathf.Min(count, Mathf.Max(1, thing.def.stackLimit));
                    thing.stackCount = n;
                    count -= n;
                    if (!GenPlace.TryPlaceThing(thing, pos, map, ThingPlaceMode.Near) && !thing.Destroyed)
                    {
                        thing.Destroy();
                    }
                }
            }
        }

        Props.finishSound?.PlayOneShot(new TargetInfo(pos, map));

        // 移除本體。倒地個體比照 Retract 的做法直接 Vanish，不走 Kill：
        // 不會觸發死亡通知、DeathActionWorker（例如死亡爆炸）與陣營關係處理。
        if (t is Corpse corpse)
        {
            if (!corpse.Destroyed) corpse.Destroy();
        }
        else
        {
            p.DeSpawnOrDeselect();
            if (!p.Destroyed) p.Destroy();
        }
    }

    #endregion

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref pendingDesignation, "pendingRecycleDesignation", false);
    }
}
