using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Fortified
{
    public class FloatMenuOptionProvider_RepairSelf : FloatMenuOptionProvider_AwakenMech
    {
        protected override bool Drafted => true;
        protected override bool Undrafted => true;
        protected override bool Multiselect => false;
        protected override bool MechanoidCanDo => true;
        protected override bool CanSelfTarget => true;

        protected override bool AppliesInt(FloatMenuContext context)
        {
            return MechRepairUtility.CanRepair(context.FirstSelectedPawn);
        }

        //提供的選項。
        protected override FloatMenuOption GetSingleOptionFor(Pawn clickedPawn, FloatMenuContext context)
        {
            if (clickedPawn != context.FirstSelectedPawn) return null;

            return new FloatMenuOption("RepairMech".Translate(clickedPawn.LabelShortCap), () =>
            {
                Job job = JobMaker.MakeJob(FFF_DefOf.FFF_RepairSelf, clickedPawn);
                clickedPawn.jobs.StartJob(job);
            });
        }
    }
}