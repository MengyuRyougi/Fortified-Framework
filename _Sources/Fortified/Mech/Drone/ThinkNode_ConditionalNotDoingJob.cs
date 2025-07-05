using Verse;
using Verse.AI;

namespace Fortified
{
    public class ThinkNode_ConditionalNotDoingJob : ThinkNode_Conditional
    {
        public JobDef jobDef;
        public override ThinkNode DeepCopy(bool resolve = true)
        {
            ThinkNode_ConditionalNotDoingJob copy = (ThinkNode_ConditionalNotDoingJob)base.DeepCopy(resolve);
            return copy;
        }
        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn.jobs == null || pawn.jobs.curJob == null) return true;
            if (pawn.jobs.curJob.def == jobDef) return false;
            return true;
        }
    }
}