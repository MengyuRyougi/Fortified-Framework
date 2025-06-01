using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Fortified
{
    //Mostly intended to make fly overhead bullets can hit thing
    public class Bullet_ForAirSupport : Bullet
    {
        protected new bool CanHit(Thing thing)
        {
            if (!thing.Spawned)
            {
                return false;
            }
            if (thing == launcher)
            {
                return false;
            }
            if (thing.Map != base.Map)
            {
                return false;
            }
            if (CoverUtility.ThingCovered(thing, base.Map))
            {
                return false;
            }
            return true;
        }

        protected override void ImpactSomething()
        {
            if (def.projectile.flyOverhead)
            {
                RoofDef roofDef = Map.roofGrid.RoofAt(Position);
                if (roofDef != null)
                {
                    if (roofDef.isThickRoof)
                    {
                        if (!def.projectile.soundHitThickRoof.NullOrUndefined())
                        {
                            def.projectile.soundHitThickRoof.PlayOneShot(new TargetInfo(Position, Map));
                        }
                        Destroy();
                        return;
                    }
                    if (Position.GetEdifice(Map) == null || Position.GetEdifice(Map).def.Fillage != FillCategory.Full)
                    {
                        RoofCollapserImmediate.DropRoofInCells(Position, Map);
                    }
                }
            }

            if (usedTarget.HasThing && CanHit(usedTarget.Thing))
            {
                Impact(usedTarget.Thing);
                return;
            }

            List<Thing> list = VerbUtility.ThingsToHit(Position, Map, CanHit);
            list.Shuffle();
            for (int i = 0; i < list.Count; i++)
            {
                Thing thing = list[i];
                float num;
                if (thing is Pawn pawn)
                {
                    num = 0.5f * Mathf.Clamp(pawn.BodySize, 0.1f, 2f);
                    if (pawn.GetPosture() != 0 && (origin - destination).MagnitudeHorizontalSquared() >= 20.25f)
                    {
                        num *= 0.5f;
                    }
                    if (launcher != null && pawn.Faction != null && launcher.Faction != null && !pawn.Faction.HostileTo(launcher.Faction))
                    {
                        num *= VerbUtility.InterceptChanceFactorFromDistance(origin, base.Position);
                    }
                }
                else
                {
                    num = 1.5f * thing.def.fillPercent;
                }
                if (Rand.Chance(num))
                {
                    Impact(list.RandomElement());
                    return;
                }
            }
            Impact(null);
        }

    }
}
