using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Fortified
{
    //Self Repair Effect
    public class HediffComp_MechHeal : HediffComp
    {
        public int ticksSinceHeal;

        public HediffCompProperties_MechHeal Props => (HediffCompProperties_MechHeal)props;

        public override void CompExposeData()
        {
            Scribe_Values.Look(ref ticksSinceHeal, "ticksSinceHeal", 0);
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (!parent.pawn.Spawned) return;

            ticksSinceHeal++;
            if (ticksSinceHeal > Props.healIntervalTicksStanding)
            {
                Pawn pawn = parent.pawn;
                if (pawn.health != null)
                {
                    var a = GetHediffs;
                    if (!a.NullOrEmpty())
                    {
                        a.RandomElement().Severity -= Props.healAmount;
                    }
                }
                ticksSinceHeal = 0;
            }
        }
        private List<Hediff> GetHediffs => (from Hediff item in parent.pawn.health.hediffSet.hediffs.Where(p => p is Hediff_Injury) select item).ToList();
    }
    public class HediffCompProperties_MechHeal : HediffCompProperties
    {
        public int healIntervalTicksStanding = 50;
        public float healAmount = 1f;

        public HediffCompProperties_MechHeal()
        {
            compClass = typeof(HediffComp_MechHeal);
        }
    }
}
