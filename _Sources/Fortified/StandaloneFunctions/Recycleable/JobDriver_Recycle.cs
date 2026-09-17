using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Fortified;

/// <summary>
/// TargetA = 倒地的 pawn 或屍體。走過去、原地作業一段時間、然後交給 CompRecycleable.Recycle 收尾。
/// </summary>
public class JobDriver_Recycle : JobDriver
{
    private CompRecycleable Comp => CompRecycleable.Get(TargetThingA);

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(TargetA, job, 1, -1, null, errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedOrNull(TargetIndex.A);
        this.FailOnBurningImmobile(TargetIndex.A);
        // 目標站起來、被搬走、或標記被取消時中止。
        this.FailOn(() =>
        {
            CompRecycleable c = Comp;
            return c == null || !c.CanRecycle().Accepted || !c.Designated;
        });

        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

        CompRecycleable comp = Comp;
        int ticks = comp?.WorkTicksFor(pawn) ?? 600;
        Toil work = Toils_General.Wait(ticks, TargetIndex.A);
        work.WithProgressBarToilDelay(TargetIndex.A);
        work.WithEffect(() => comp?.Props.workEffecter ?? EffecterDefOf.ConstructMetal, TargetIndex.A);
        work.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
        work.activeSkill = () => SkillDefOf.Construction;
        yield return work;

        yield return Toils_General.Do(() => Comp?.Recycle(pawn));
    }
}
