using System.Linq;
using Verse;

namespace Fortified
{
    public class CompDronePack : CompAIUsablePack
    {
        protected override float ChanceToUse(Pawn wearer)
        {
            return 0.01f;
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