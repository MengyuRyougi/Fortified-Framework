using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace Fortified
{
    public class JobDriver_RemoveModification : JobDriver
    {
        private const int DurationTicks = 600;

        private Pawn TargetPawn => (Pawn)job.GetTarget(TargetIndex.A).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(TargetPawn, job, 1, -1, null, errorOnFailed);
        }
        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.A);
            Toil toil = Toils_General.WaitWith(TargetIndex.A, DurationTicks, true, true);
            toil.FailOnDespawnedOrNull(TargetIndex.A);
            toil.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            toil.WithEffect(EffecterDefOf.MechRepairing, TargetIndex.A);
            toil.handlingFacing = true;
            yield return toil;
            yield return Toils_General.Do(RemoveModification);
        }

        private void RemoveModification()
        {
            var mod = TargetPawn.health.hediffSet.GetHediffComps<HediffComp_Modification>().Where(m => m.isApplyTarget).First();
            if (mod != null) return;

            Messages.Message("FFF.Message.Modification.Removed".Translate(TargetPawn), TargetPawn, MessageTypeDefOf.PositiveEvent);
            TargetPawn.health.RemoveHediff(mod.parent);
            Thing thing = ThingMaker.MakeThing(mod.parent.def.spawnThingOnRemoved);
            thing.stackCount = 1;
            GenPlace.TryPlaceThing(thing, thing.Position, thing.MapHeld, ThingPlaceMode.Near);
        }
    }
}