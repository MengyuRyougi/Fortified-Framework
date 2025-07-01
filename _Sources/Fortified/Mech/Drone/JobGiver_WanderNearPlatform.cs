using Verse;
using Verse.AI;

namespace Fortified
{
    public class JobGiver_WanderNearPlatform : JobGiver_Wander
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.jobs.curJob == null) return base.TryGiveJob(pawn);
            return null;
        }
        public JobGiver_WanderNearPlatform()
        {
            wanderRadius = 7f;
            locomotionUrgency = LocomotionUrgency.Walk;
            ticksBetweenWandersRange = new IntRange(120, 240);
        }

        protected override IntVec3 GetWanderRoot(Pawn pawn)
        {
            if (pawn.TryGetComp<CompDrone>(out var d) && d.HasPlatform)
            {
                if (d.IsApparelPlatform) return d.Apparel.Wearer.Position;
                return d.Platform.Position;
            }
            return pawn.Position;
        }
    }
}