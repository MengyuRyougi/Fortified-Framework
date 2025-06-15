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
        public override Job JobOnThing(Pawn pawn, Thing thing, bool forced = false)
        {
            Job result = base.JobOnThing(pawn, thing, forced);
            if (result != null && result.def == JobDefOf.DoBill)
            {
                Job job = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("FFF_DoAutonomousBill"), thing);
                job.targetQueueB = result.targetQueueB;
                job.countQueue = result.countQueue;
                job.haulMode = HaulMode.ToCellNonStorage;
                job.bill = result.bill;
                return job;
            }
            return result;
        }
    }
}