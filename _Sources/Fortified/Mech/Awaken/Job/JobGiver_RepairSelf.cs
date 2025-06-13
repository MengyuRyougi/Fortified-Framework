using Fortified;
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
    public class JobGiver_RepairSelf : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.TryGetComp<CompDeadManSwitch>() is CompDeadManSwitch comp && comp.woken && MechRepairUtility.CanRepair(pawn))
            {
                return JobMaker.MakeJob(FFF_DefOf.FFF_RepairSelf,pawn);
            }
            return null;
        }
    }
}
