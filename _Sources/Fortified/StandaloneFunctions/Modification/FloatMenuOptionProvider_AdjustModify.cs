using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using static System.Net.Mime.MediaTypeNames;
using Verse.AI;

namespace Fortified
{
    public class FloatMenuOptionProvider_AdjustModify : FloatMenuOptionProvider
    {
        protected override bool Drafted => false;
        protected override bool Undrafted => true;
        protected override bool Multiselect => false;
        protected override bool MechanoidCanDo => true;

        public override IEnumerable<FloatMenuOption> GetOptionsFor(Pawn clickedPawn, FloatMenuContext context)
        {
            if (clickedPawn.health == null) yield break;

            var comps = clickedPawn.health.hediffSet.GetHediffComps<HediffComp_Modification>();
            if (comps.EnumerableNullOrEmpty()) yield break;
            foreach (HediffComp_Modification item in comps.Cast<HediffComp_Modification>())
            {
                yield return RimWorld.FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(item.Props.applyJob.LabelCap, delegate
                {
                    item.isApplyTarget = true;
                    context.FirstSelectedPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(item.Props.applyJob, clickedPawn), JobTag.Misc);
                    FleckMaker.Static(clickedPawn.DrawPos, clickedPawn.MapHeld, FleckDefOf.FeedbackEquip);
                }, MenuOptionPriority.High), context.FirstSelectedPawn, clickedPawn);
            }
        }
    }
}