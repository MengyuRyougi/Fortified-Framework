using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Fortified
{
    public static class BuildingTurretCapacityUtil
    {
        public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn, Thing me, ThingOwner innerContainer)
        {
            if (innerContainer.Count == 0)
            {
                if (!myPawn.CanReach(me, PathEndMode.InteractionCell, Danger.Deadly, false, false, TraverseMode.ByPawn))
                {
                    FloatMenuOption floatMenuOption3 = new FloatMenuOption("CannotUseNoPath".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
                    yield return floatMenuOption3;
                }
                else
                {
                    JobDef jobDef = FFF_DefOf.FFF_EnterBunkerFacility;
                    string label = "FT_BunkerFacility_EnterText".Translate();
                    void action()
                    {
                        Job job = JobMaker.MakeJob(jobDef, me);
                        myPawn.jobs.TryTakeOrderedJob(job, new JobTag?(JobTag.Misc), false);
                    }
                    yield return RimWorld.FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, action, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0), myPawn, me, "ReservedBy");
                }
            }
            yield break;
        }
    }
}
