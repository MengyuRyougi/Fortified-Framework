using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Fortified
{
    /// <summary>
    /// Drone 是一次性使用的遙控機器人。
    /// </summary>
    public class CompDrone : ThingComp
    {
        private Pawn pawn => parent as Pawn;
        protected Thing parentPlatform = null;
        public Thing Platform => parentPlatform;//召喚的Thing。
        private CompMechPowerCell powerCell;
        public Thing PlatformOwner
        {
            get
            {
                if (parentPlatform == null) return null;
                if (isApparelPlatform) return Apparel.Wearer;
                return parentPlatform;
            }
        }

        private bool isApparelPlatform = false;
        public bool IsApparelPlatform => isApparelPlatform;
        public Apparel Apparel => Platform as Apparel;

        public CompProperties_Drone Props => (CompProperties_Drone)this.props;

        public bool CanDraft
        {
            get
            {
                if (parentPlatform == null || !parentPlatform.Spawned) return false;
                if (CanDraftAsApparelPlatform()) return true;
                if (parentPlatform.Faction != Faction.OfPlayer) return false;

                if (parentPlatform is Building building)
                {
                    if (!parentPlatform.TryGetComp<CompPowerTrader>().PowerOn) return false;
                    if (building.TryGetComp<CompBreakdownable>().BrokenDown) return false;
                    if (building.TryGetComp<CompFlickable>().SwitchIsOn == false) return false;
                }
                return true;
            }
        }
        public override void PostPostMake()
        {
            //玩家召喚的並不會有自帶裝備。
            if (this.parent.Faction == Faction.OfPlayer)
            {
                (this.parent as Pawn).equipment?.DestroyAllEquipment(DestroyMode.Vanish);
            }
        }
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            powerCell = parent.TryGetComp<CompMechPowerCell>();

        }
        public override void Notify_Killed(Map prevMap, DamageInfo? dinfo = null)
        {
            if (this.parent.Faction == Faction.OfPlayer)
            {
                (this.parent as Pawn).equipment?.DropAllEquipment(parent.Position);
            }
            base.Notify_Killed(prevMap, dinfo);
        }
        private bool CanDraftAsApparelPlatform()
        {
            if (!isApparelPlatform) return false;

            var wearer = Apparel.Wearer;
            if (wearer == null) return false;
            if (!wearer.Spawned) return false;
            if (wearer.Map != parent.Map) return false;
            if (!wearer.IsPlayerControlled) return false;
            return true;
        }
        public bool HasPlatform
        {
            get
            {
                if (parentPlatform == null) return false;
                if (isApparelPlatform)
                {
                    return Apparel.Wearer != null && Apparel.Wearer.Spawned && Apparel.Wearer.Map == parent.Map;
                }
                return parentPlatform.Spawned && parentPlatform.Map == parent.Map;
            }
        }
        public void SetPlatform(Thing thing)
        {
            parentPlatform = thing;
            if (thing.def.IsApparel) isApparelPlatform = true;
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }
            if (parent is Pawn p && p.Faction == Faction.OfPlayer && Props.returnToDraftPlatformJob != null && HasPlatform)
            {
                var draftGizmo = new Command_Action
                {
                    defaultLabel = "FFF.Drone.Return".Translate(),
                    defaultDesc = "FFF.Drone.ReturnDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get(Props.returnGizmoPath),
                    action = () =>
                    {
                        ReturnToPlatform();
                    }
                };
                yield return draftGizmo;
            }
        }
        public override void CompTick()
        {
            if (!parent.Spawned) return;

            if (!parent.IsHashIntervalTick(500)) return;
            if (powerCell.PowerTicksLeft < 5000) Log.Message(powerCell.PowerTicksLeft);

            if (pawn.CurJobDef != Props.returnToDraftPlatformJob && powerCell != null && powerCell.PowerTicksLeft < 5000)
            {
                ReturnToPlatform();
            }
        }
        bool noPlatformWarning = false;
        public void ReturnToPlatform()
        {
            if (!HasPlatform && !noPlatformWarning)
            {
                //如果沒有平台則警告
                Messages.Message("FFF.Drone.NoPlatform".Translate(), MessageTypeDefOf.RejectInput, false);
                noPlatformWarning = true;
                return;
            }

            if (isApparelPlatform)
            {
                //pawn.jobs.StopAll();
                pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(Props.returnToDraftPlatformJob, PlatformOwner, Apparel),JobTag.DraftedOrder);
            }
            else
            {
                //pawn.jobs.StopAll();
                pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(Props.returnToDraftPlatformJob, PlatformOwner), JobTag.DraftedOrder);
            }
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref parentPlatform, "parentPlatform");
            Scribe_Values.Look(ref isApparelPlatform, "isApparelPlatform", false);
        }
    }
    public class CompProperties_Drone : CompProperties
    {
        [NoTranslate]
        public string returnGizmoPath = "UI/Drone_Retract";

        public JobDef returnToDraftPlatformJob = null;
        public CompProperties_Drone()
        {
            this.compClass = typeof(CompDrone);
        }
    }

    [HarmonyPatch(typeof(PawnComponentsUtility), nameof(PawnComponentsUtility.AddAndRemoveDynamicComponents))]
    internal static class Patch_AddAndRemoveDynamicComponents
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, bool actAsIfSpawned)
        {
            if (ModsConfig.BiotechActive && pawn.kindDef != null && pawn.TryGetComp<CompDrone>() != null && pawn.workSettings == null)
            {

                pawn.workSettings = new Pawn_WorkSettings(pawn);
                pawn.workSettings.EnableAndInitializeIfNotAlreadyInitialized();
            }
        }
    }

    //[HarmonyPatch(typeof(ThinkNode_ConditionalWorkMode), "Satisfied")]
    //internal static class Patch_Satisfied
    //{
    //    [HarmonyPostfix]
    //    public static void Postfix(Pawn pawn, ref bool __result)
    //    {
    //        if (__result) return;
    //        if (pawn.Faction == Faction.OfPlayer && pawn.TryGetComp<CompDrone>() != null)
    //        {
    //            __result = true;
    //        }
    //    }
    //}

    [HarmonyPatch(typeof(Pawn_DraftController), nameof(Pawn_DraftController.ShowDraftGizmo), MethodType.Getter)]
    internal static class Patch_ShowDraftGizmo
    {
        internal static void Postfix(Pawn_DraftController __instance, ref bool __result)
        {
            if (__result) return;
            if (__instance.pawn.Faction == Faction.OfPlayer && __instance.pawn.TryGetComp<CompDrone>() != null) __result = true;
        }
    }
    [HarmonyPatch(typeof(MechanitorUtility), nameof(MechanitorUtility.CanDraftMech))]
    internal static class Patch_CanDraftMech
    {
        internal static void Postfix(Pawn mech, ref AcceptanceReport __result)
        {
            if (__result) return;
            if (mech.TryGetComp<CompDrone>(out var d) && d.CanDraft) __result = true;
        }
    }
}