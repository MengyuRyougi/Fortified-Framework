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

        private CompRefuelable compFuel;

        private bool HasFuel => compFuel == null || compFuel.HasFuel;
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            compFuel = parent.TryGetComp<CompRefuelable>();
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn myPawn)
        {
            if (compFuel != null)
            {
                if (!HasFuel)
                {
                    yield return new FloatMenuOption(FloatMenuOptionLabel(myPawn) + "FFF.Reason.NotFueled".Translate(), null);
                    yield break;
                }
                if (compFuel.Fuel < (float)Props.fuelCostsPerUse)
                {
                    yield return new FloatMenuOption(FloatMenuOptionLabel(myPawn) + "FFF.Reason.NoEnoughFuel".Translate(), null);
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
            compFuel.ConsumeFuel(Props.fuelCostsPerUse);
        }
    }
}