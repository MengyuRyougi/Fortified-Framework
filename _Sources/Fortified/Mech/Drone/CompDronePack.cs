using RimWorld;
using System.Linq;
using Verse;

namespace Fortified
{
    public class CompDronePack : CompAIUsablePack
    {
        protected override float ChanceToUse(Pawn wearer)
        {
            if (wearer.IsPrisoner) return 0f;
            if (Rand.Chance(0.05f))
            {
                if (wearer.Map.mapPawns.FactionsOnMap().Any(d => d.HostileTo(wearer.Faction)))
                {
                    return 0.25f;
                }
            }
            return 0f;
        }

        protected override void UsePack(Pawn wearer)
        {
            var A = wearer.apparel.AllApparelVerbs.Where(a => a.VerbOwner_ChargedCompSource.parent == this.parent).First();
            if (A != null)
            {
                A.TryStartCastOn(new LocalTargetInfo(wearer.Position));
            }
            else
            {
                Log.Error($"Failed to find verb for {this.parent.Label} on {wearer.Name}.");
            }
        }
    }
}