using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Fortified;
public class CompProperties_ShieldingDevice : CompProperties
{
    public List<IncidentDef> incidentsWhitelist;
    public int coolDownTicks = 60000;
    public int coolDownPerRaidPoint = 0;
    public int powerCostPerColonist = 500;

    public EffecterDef activeEffecter;
    public EffecterDef cooldownEffecter;
    public EffecterDef triggerEffecter;

    public List<PlanetLayerDef> planetLayerWhitelist;

    public CompProperties_ShieldingDevice()
    {
        compClass = typeof(CompShieldingDevice);
    }
}
public class CompShieldingDevice : ThingComp
{
    private CompPowerTrader compPowerInt;
    public static List<CompShieldingDevice> cache = new List<CompShieldingDevice>();
    public CompProperties_ShieldingDevice Props => (CompProperties_ShieldingDevice)props;
    private Effecter effecter;
    public CompPowerTrader CompPower
    {
        get
        {
            compPowerInt ??= parent.TryGetComp<CompPowerTrader>();
            return compPowerInt;
        }
    }
    public bool HasPower => CompPower == null || CompPower.PowerOn;

    private int remainingTicks = 0;
    public bool CoolingDown => remainingTicks > 0;
    public bool LayerAllowed => Props.planetLayerWhitelist.NullOrEmpty() || Props.planetLayerWhitelist.Contains(parent.Map.Tile.LayerDef);
    public bool Active => HasPower && !CoolingDown && LayerAllowed;

    private MapComponent_Population popCache;
    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        if (!cache.Contains(this)) cache.Add(this);
        if (parent.Map != null)
        {
            popCache = parent.Map.GetComponent<MapComponent_Population>();
            if (popCache == null)
            {
                popCache = new MapComponent_Population(parent.Map);
                parent.Map.components.Add(popCache);
                popCache.FinalizeInit();
            }
            popCache.OnColonistCountChanged += HandleColonistCountChanged;

            // 初始套用一次
            HandleColonistCountChanged(popCache.GetColonistCount());
        }
    }

    public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
    {
        base.PostDeSpawn(map, mode);
        if (popCache != null)
        {
            popCache.OnColonistCountChanged -= HandleColonistCountChanged;
            popCache = null;
        }
        cache.Remove(this);
    }

    public override void PostDestroy(DestroyMode mode, Map previousMap)
    {
        base.PostDestroy(mode, previousMap);
        if (popCache != null)
        {
            popCache.OnColonistCountChanged -= HandleColonistCountChanged;
            popCache = null;
        }
        cache.Remove(this);
    }
    public override void CompTickInterval(int delta)
    {
    }
    public override void CompTick()
    {
        if (!HasPower) return;
        // 只處理冷卻；耗電調整不在 Tick 中做（由事件觸發）
        if (CoolingDown)
        {
            remainingTicks--;
            // 只在冷卻歸零的那一刻提示一次；不能用 !Active 判斷，位於白名單外星層時 Active 恆為 false，會每 tick 洗訊息
            if (remainingTicks == 0)
            {
                Messages.Message("FFF.MessageDeviceReady".Translate(parent.LabelCap), parent, MessageTypeDefOf.PositiveEvent);
            }
        }

        //冷卻中／可用狀態的顯示；不可運作的星層上不顯示
        EffecterDef effecterDef = CoolingDown ? Props.cooldownEffecter : Active ? Props.activeEffecter : null;
        if (effecterDef == null)
        {
            effecter?.Cleanup();
            effecter = null;
        }
        else if (effecter == null || effecter.def != effecterDef)
        {
            effecter?.Cleanup();
            effecter = effecterDef.SpawnAttached(parent, parent.Map);
        }
        else effecter.EffectTick(parent, TargetInfo.Invalid);
    }
    public override string CompInspectStringExtra()
    {
        string text = "FFF.ColonistCount".Translate(colonistCountCache);
        if (CoolingDown) text += "\n" + "FFF.MessageCooldownRemaining".Translate(remainingTicks.ToStringTicksToPeriod());
        else if (parent.Spawned && !LayerAllowed) text += "\n" + "CannotFunctionOnLayer".Translate(parent.Map.Tile.LayerDef.label).CapitalizeFirst();
        return base.CompInspectStringExtra() + text;
    }
    public void TriggerCooldown(int raidPoint)
    {
        Props.triggerEffecter.Spawn().Trigger(parent, parent);
        remainingTicks = raidPoint * Props.coolDownPerRaidPoint;
    }
    public void TriggerCooldown()
    {
        Props.triggerEffecter.Spawn().Trigger(parent, parent);
        remainingTicks = Props.coolDownTicks;
    }
    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref remainingTicks, "remainingCoolDownTicks", 0);

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            if (!cache.Contains(this)) cache.Add(this);
        }
    }
    private int colonistCountCache = 0;
    private void HandleColonistCountChanged(int colonistCount)
    {
        colonistCountCache = colonistCount;

        if (CompPower == null) return;
        CompPower.PowerOutput = 0 - (Props.powerCostPerColonist * colonistCount + CompPower.Props.PowerConsumption);//人數*500+基礎耗電
    }

    public static IEnumerable<Thing> GetTowerByIncident(IncidentDef def)
    {
        if (cache.NullOrEmpty())
        {
            yield break;
        }
        IEnumerable<CompShieldingDevice> enumerable = cache.Where((CompShieldingDevice x) => x.Props.incidentsWhitelist.Contains(def) && x.Active);
        if (enumerable.EnumerableNullOrEmpty())
        {
            yield break;
        }
        foreach (CompShieldingDevice item in enumerable)
        {
            yield return item.parent;
        }
    }
}
