using Fortified;
using RimWorld;
using Verse;

namespace Fortified
{
    public class Gene_Wetware : Gene
    {
        private bool check;
        public override void Tick()
        {
            base.Tick();
            if (pawn.Spawned && !check)
            {
                if (pawn?.ageTracker?.AgeBiologicalYears < 1)
                {
                    Thing t = ThingMaker.MakeThing(RimWorld.ThingDefOf.ComponentSpacer);
                    if (DefDatabase<ThingDef>.GetNamedSilentFail("Neurocomputer") != null)
                    {
                        t = ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamedSilentFail("Neurocomputer"));
                    }
                    GenPlace.TryPlaceThing(t, pawn.Position, pawn.Map, ThingPlaceMode.Near);
                    pawn.DeSpawn();
                    pawn.Destroy(DestroyMode.KillFinalize);
                }
                check = true;
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref check, "Gene_Wetware_Check");
        }
    }
}