using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Fortified
{
    public class Bill_Production_Environmental : Bill_Production
    {
        public Building_WorkTable WorkBench => billStack.billGiver as Building_WorkTable;

        public ModExt_EnvironmentalBill Extension => recipe.GetModExtension<ModExt_EnvironmentalBill>();

        public Bill_Production_Environmental()
        {
        }
        public Bill_Production_Environmental(RecipeDef recipe, Precept_ThingStyle precept = null) : base(recipe, precept)
        {
        }
        public override void ExposeData()
        {
            base.ExposeData();
        }

        public override bool ShouldDoNow()
        {
            if (suspended || !base.ShouldDoNow())
            {
                return false;
            }
            if (!CheckEnvironment())
            {
                if (billStack.billGiver is Building_WorkTableAutonomous at && at.IsWorking())
                {
                    at.Cancel();
                }
                return false;
            }
            return true;
        }
        protected bool CheckEnvironment()
        {
            if (Extension == null) return true;
            if (Extension.OnlyInVacuum)
            {
                if (!this.WorkBench.Map.Biome.inVacuum || !OrbitUtility.InVacuum(this.WorkBench))
                {
                    suspended = true;
                    Messages.Message("FFF.Message.BillSuspendedInNonVacuum".Translate(Label, WorkBench.Label), MessageTypeDefOf.CautionInput);
                    return false;
                }
            }
            if (Extension.OnlyInMicroGravity)
            {
                if (!OrbitUtility.InMicroGravity(this.WorkBench))
                {
                    suspended = true;
                    Messages.Message("FFF.Message.BillSuspendedInNonMicroGravity".Translate(Label, WorkBench.Label), MessageTypeDefOf.CautionInput);
                    return false;
                }
            }
            if (Extension.OnlyInDarkness)
            { 
                //WIP
            }
            return true;
        }

        public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
        {
            if (repeatMode == BillRepeatModeDefOf.RepeatCount)
            {
                if (repeatCount > 0)
                {
                    repeatCount--;
                }
                if (repeatCount == 0)
                {
                    Messages.Message("MessageBillComplete".Translate(LabelCap), (Thing)billStack.billGiver, MessageTypeDefOf.TaskCompletion);
                }
            }
            recipe.Worker.Notify_IterationCompleted(billDoer, ingredients);
        }
    }
}