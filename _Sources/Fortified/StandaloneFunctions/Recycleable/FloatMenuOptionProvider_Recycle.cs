using RimWorld;
using Verse;
using Verse.AI;

namespace Fortified;

/// <summary>
/// 右鍵倒地的可拆解 pawn 或其屍體時提供「拆解」選項：先打上標記，再直接派工。
/// WorkGiverDef 設了 directOrderable=false，所以不會和原版的「優先…」選項重複。
/// </summary>
public class FloatMenuOptionProvider_Recycle : FloatMenuOptionProvider
{
    protected override bool Drafted => true;

    protected override bool Undrafted => true;

    protected override bool Multiselect => false;

    protected override bool RequiresManipulation => true;

    protected override FloatMenuOption GetSingleOptionFor(Thing clickedThing, FloatMenuContext context)
    {
        CompRecycleable comp = CompRecycleable.Get(clickedThing);
        if (comp == null) return null;

        Pawn worker = context.FirstSelectedPawn;
        string label = "FFF.Recycle.Order".Translate(clickedThing.LabelShort);

        AcceptanceReport report = comp.CanRecycle();
        if (!report.Accepted)
        {
            if (report.Reason.NullOrEmpty()) return null;
            return new FloatMenuOption(label + ": " + report.Reason.CapitalizeFirst(), null);
        }

        WorkTypeDef workType = FFF_DefOf.FFF_RecycleWorkGiver.workType;
        if (worker.WorkTypeIsDisabled(workType))
        {
            return new FloatMenuOption(label + ": " + "CannotPrioritizeWorkTypeDisabled".Translate(workType.gerundLabel), null);
        }
        if (clickedThing.IsBurning())
        {
            return new FloatMenuOption(label + ": " + "BurningLower".Translate(), null);
        }
        if (!worker.CanReach(clickedThing, PathEndMode.Touch, Danger.Deadly))
        {
            return new FloatMenuOption(label + ": " + "NoPath".Translate().CapitalizeFirst(), null);
        }
        if (!worker.CanReserve(clickedThing, 1, -1, null, ignoreOtherReservations: true))
        {
            Pawn other = worker.Map.reservationManager.FirstRespectedReserver(clickedThing, worker);
            string reason = other != null ? "ReservedBy".Translate(other.LabelShort, other) : "Reserved".Translate();
            return new FloatMenuOption(label + ": " + reason, null);
        }

        string full = "FFF.Recycle.OrderWithProducts".Translate(clickedThing.LabelShort, comp.ProductsSummary(worker));
        return RimWorld.FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(full, delegate
        {
            CompRecycleable.SetDesignated(clickedThing, true);
            Job job = JobMaker.MakeJob(FFF_DefOf.FFF_Recycle, clickedThing);
            job.workGiverDef = FFF_DefOf.FFF_RecycleWorkGiver;
            worker.jobs.TryTakeOrderedJob(job, JobTag.Misc);
        }), worker, clickedThing);
    }
}
