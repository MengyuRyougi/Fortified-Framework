using Multiplayer.API;
using RimWorld;
using Verse;
using Verse.AI;

namespace Fortified
{
    /// <summary>
    /// Entry points the modification window uses to hand orders to a mech. Every parameter is
    /// plain data so the calls can be synchronised in multiplayer; the Job objects are built
    /// on each client from that data.
    /// </summary>
    public static class MechModificationOrders
    {
        [SyncMethod]
        public static void OrderInstall(Pawn mech, ThingDef itemDef, Thing item, int partIndex, bool allowEquivalentPart, bool queue)
        {
            if (mech?.jobs == null || itemDef == null || FFF_DefOf.FFF_Modification == null) return;
            BodyPartRecord part = ResolvePart(mech, partIndex);
            if (item == null || item.Destroyed || !item.Spawned || item.def != itemDef)
            {
                item = ModificationUtility.GetCandidates(mech.Map, itemDef, mech, mech.Position, true).FirstOrFallback();
            }
            Job_Modification job = ModificationJobUtility.MakeApplyJob(FFF_DefOf.FFF_Modification, mech, item, part);
            job.itemDefName = itemDef.defName;
            job.allowEquivalentPart = allowEquivalentPart;
            mech.jobs.TryTakeOrderedJob(job, JobTag.MiscWork, queue);
        }

        [SyncMethod]
        public static void OrderUninstall(Pawn mech, HediffDef hediffDef, int partIndex, bool queue)
        {
            if (mech?.jobs == null || hediffDef == null) return;
            JobDef removeDef = hediffDef.CompProps<HediffCompProperties_Modification>()?.applyJob ?? FFF_DefOf.FFF_ModificationRemove;
            if (removeDef == null) return;
            Job_Modification job = ModificationJobUtility.MakeRemoveJob(removeDef, mech, hediffDef, ResolvePart(mech, partIndex));
            mech.jobs.TryTakeOrderedJob(job, JobTag.MiscWork, queue);
        }

        private static BodyPartRecord ResolvePart(Pawn mech, int partIndex)
        {
            var parts = mech.RaceProps?.body?.AllParts;
            return parts != null && partIndex >= 0 && partIndex < parts.Count ? parts[partIndex] : null;
        }
    }
}
