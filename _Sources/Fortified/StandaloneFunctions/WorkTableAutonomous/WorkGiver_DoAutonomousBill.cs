using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace Fortified
{
    public class WorkGiver_DoAutonomousBill : WorkGiver_DoBill
    {
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return base.HasJobOnThing(pawn, t, forced) && pawn.CanReserveAndReach(t, PathEndMode.InteractionCell, Danger.Deadly) && t is Building_WorkTableAutonomous;
        }

        public override Job JobOnThing(Pawn pawn, Thing thing, bool forced = false)
        {
            if (thing is Building_WorkTableAutonomous building)
            {
                if (building.activeBill == null)
                {
                    Job job = base.JobOnThing(pawn, thing, forced);
                    if (job != null)
                    {
                        Job job2 = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("FFF_DoAutonomousBill"), thing);
                        job2.targetQueueA = job.targetQueueA;
                        job2.targetQueueB = job.targetQueueB;
                        job2.countQueue = job.countQueue;
                        job2.haulMode = HaulMode.ToCellNonStorage;
                        job2.bill = job.bill;
                        return job2;
                    }
                }
                else if (!building.prepared && building.activeBill.recipe.PawnSatisfiesSkillRequirements(pawn))
                {
                    return JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("FFF_FinishAutonomousBill"), thing);
                }
            }
            return null;
        }
    }
}