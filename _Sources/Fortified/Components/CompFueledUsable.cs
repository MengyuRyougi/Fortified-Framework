using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Fortified
{
    public class CompProperties_FueledUsable : CompProperties_Usable
    {
        public int fuelCostsPerUse = 1;

        public CompProperties_FueledUsable()
        {
            compClass = typeof(CompFueledUsable);
        }
    }

    public class CompFueledUsable : CompUsable
    {
        public new CompProperties_FueledUsable Props => (CompProperties_FueledUsable)props;

        private CompRefuelable CompFuel => parent.TryGetComp<CompRefuelable>();

        private bool HasFuel
        {
            get
            {
                if (CompFuel != null)
                {
                    return CompFuel.HasFuel;
                }
                return true;
            }
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn myPawn)
        {
            if (CompFuel != null)
            {
                if (!HasFuel)
                {
                    yield return new FloatMenuOption(FloatMenuOptionLabel(myPawn) + "FT_NotFueled".Translate(), null);
                    yield break;
                }
                if (CompFuel.Fuel < (float)Props.fuelCostsPerUse)
                {
                    yield return new FloatMenuOption(FloatMenuOptionLabel(myPawn) + "FT_NoEnoughFuel".Translate(), null);
                    yield break;
                }
            }
            foreach (FloatMenuOption item in base.CompFloatMenuOptions(myPawn))
            {
                yield return item;
            }
        }

        public override void UsedBy(Pawn p)
        {
            base.UsedBy(p);
            CompFuel.ConsumeFuel(Props.fuelCostsPerUse);
        }
    }
}