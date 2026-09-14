using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace Fortified
{
    public static class ModificationUtility
    {
        public static bool SupportedByRace(Pawn pawn, CompProperties_AddHediffOnTarget comp)
        {
            return pawn != null && comp != null && (comp.supportRaceDefs.NullOrEmpty() || comp.supportRaceDefs.Contains(pawn.def));
        }

        public static BodyPartRecord GetBodyPartRecord(Pawn pawn, CompProperties_AddHediffOnTarget props)
        {
            return HasSpaceToAttach(pawn, props, out BodyPartRecord part) ? part : null;
        }

        public static bool HasSpaceToAttach(Pawn pawn, CompProperties_AddHediffOnTarget comp, out BodyPartRecord bodyPart)
        {
            bodyPart = null;
            if (pawn?.RaceProps?.body == null || comp == null) return false;

            if (comp.targetBodyPartDefs.NullOrEmpty())
            {
                bodyPart = pawn.RaceProps.body.corePart;
                return bodyPart != null;
            }

            List<BodyPartRecord> parts = pawn.RaceProps.body.AllParts;
            for (int i = 0; i < parts.Count; i++)
            {
                if (!CanAttachToPart(pawn, comp, parts[i])) continue;
                bodyPart = parts[i];
                return true;
            }
            return false;
        }

        public static bool CanAttachToPart(Pawn pawn, CompProperties_AddHediffOnTarget comp, BodyPartRecord part)
        {
            if (!IsValidTargetPart(pawn, comp, part)) return false;
            if (comp.targetBodyPartDefs.NullOrEmpty()) return true;

            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            for (int i = 0; i < hediffs.Count; i++)
            {
                Hediff hediff = hediffs[i];
                if (hediff?.Part == part && hediff.TryGetComp<HediffComp_Modification>() != null) return false;
            }
            return true;
        }

        public static bool IsValidTargetPart(Pawn pawn, CompProperties_AddHediffOnTarget comp, BodyPartRecord part, bool allowEquivalentPart = false)
        {
            if (pawn?.health?.hediffSet == null || pawn.RaceProps?.body == null || comp == null || part == null) return false;
            if (!SupportedByRace(pawn, comp) || pawn.health.hediffSet.PartIsMissing(part)) return false;
            if (comp.targetBodyPartDefs.NullOrEmpty()) return part == pawn.RaceProps.body.corePart;
            return allowEquivalentPart || comp.targetBodyPartDefs.Contains(part.def);
        }

        /// <summary>
        /// Spawned stacks of <paramref name="def"/> on <paramref name="map"/> that <paramref name="actor"/> may use,
        /// nearest to <paramref name="origin"/> first. Backed by the vanilla ListerThings; no extra indexing needed.
        /// </summary>
        public static List<Thing> GetCandidates(Map map, ThingDef def, Pawn actor, IntVec3 origin, bool requireReachableAndReservable)
        {
            List<Thing> result = new List<Thing>();
            List<Thing> things = def == null ? null : map?.listerThings?.ThingsOfDef(def);
            if (things == null) return result;
            for (int i = 0; i < things.Count; i++)
            {
                Thing thing = things[i];
                if (thing == null || thing.Destroyed || !thing.Spawned || thing.stackCount <= 0) continue;
                if (actor != null)
                {
                    if (thing.IsForbidden(actor)) continue;
                    if (requireReachableAndReservable && (AvailableCount(thing, actor) <= 0 || !actor.CanReach(thing, PathEndMode.Touch, Danger.Deadly) || !actor.CanReserve(thing))) continue;
                }
                result.Add(thing);
            }
            if (origin.IsValid && result.Count > 1)
            {
                result.Sort((left, right) => (left.Position - origin).LengthHorizontalSquared.CompareTo((right.Position - origin).LengthHorizontalSquared));
            }
            return result;
        }

        public static int CountAvailable(Map map, ThingDef def, Pawn actor)
        {
            List<Thing> candidates = GetCandidates(map, def, actor, actor?.Position ?? IntVec3.Invalid, true);
            int count = 0;
            for (int i = 0; i < candidates.Count; i++) count += AvailableCount(candidates[i], actor);
            return count;
        }

        /// <summary>
        /// Pieces of <paramref name="thing"/> not already claimed by <paramref name="actor"/>'s own jobs.
        /// CanReserve always succeeds for the claimant itself, so without this a stack the mech has
        /// already reserved for a queued modification job would be counted as free again.
        /// </summary>
        public static int AvailableCount(Thing thing, Pawn actor)
        {
            if (thing == null) return 0;
            int count = thing.stackCount;
            List<ReservationManager.Reservation> reservations = actor?.Map?.reservationManager?.ReservationsReadOnly;
            if (reservations == null) return count;
            for (int i = 0; i < reservations.Count && count > 0; i++)
            {
                ReservationManager.Reservation reservation = reservations[i];
                if (reservation.Claimant != actor || reservation.Target.Thing != thing) continue;
                count -= reservation.StackCount < 0 ? thing.stackCount : reservation.StackCount;
            }
            return count < 0 ? 0 : count;
        }

        public static bool HasAnyBodyPartOf(Pawn pawn, List<BodyPartDef> partDefs)
        {
            return pawn?.RaceProps?.body != null && !pawn.RaceProps.body.AllParts.Where(p => partDefs.Contains(p.def)).EnumerableNullOrEmpty();
        }

        public static bool HasBodyPartOf(Pawn pawn, BodyPartDef partDef)
        {
            return pawn?.RaceProps?.body != null && !pawn.RaceProps.body.AllParts.Where(p => p.def == partDef).EnumerableNullOrEmpty();
        }
    }
}
