using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Fortified
{
    public class WorkGiver_HaulResourcesToCarrier_Building : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Undefined);

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return pawn.Map.listerThings.GetAllThings(t => t.Faction == Faction.OfPlayer && t.TryGetComp<CompMechPlatform>() != null);
        }
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            CompMechPlatform comp = null;
            if (t is Pawn p)
            {
                if (!p.IsColonyMech || !p.Spawned || p.Downed)
                {
                    return false;
                }
                comp = p.GetComp<CompMechPlatform>();
            }
            if (t is Building b)
            {
                if (!b.Spawned || b.Faction != Faction.OfPlayer)
                {
                    return false;
                }
                comp = b.GetComp<CompMechPlatform>();
            }

            if (comp == null)
            {
                return false;
            }
            int amountToAutofill = comp.AmountToAutofill;
            if (amountToAutofill <= 0)
            {
                return false;
            }
            if (!pawn.CanReserve(t, 1, -1, null, forced))
            {
                return false;
            }
            return !HaulAIUtility.FindFixedIngredientCount(pawn, comp.Props.fixedIngredient, amountToAutofill).NullOrEmpty();
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            CompMechPlatform compMechPlatform = t.TryGetComp<CompMechPlatform>();
            if (compMechPlatform == null)
            {
                return null;
            }
            int amountToAutofill = compMechPlatform.AmountToAutofill;
            if (amountToAutofill <= 0)
            {
                return null;
            }
            List<Thing> list = HaulAIUtility.FindFixedIngredientCount(pawn, compMechPlatform.Props.fixedIngredient, amountToAutofill);
            if (!list.NullOrEmpty())
            {
                Job job = HaulAIUtility.HaulToContainerJob(pawn, list[0], t);
                job.count = Mathf.Min(job.count, amountToAutofill);
                job.targetQueueB = (from i in list.Skip(1)
                                    select new LocalTargetInfo(i)).ToList();
                return job;
            }
            return null;
        }
    }
}