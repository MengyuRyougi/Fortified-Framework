using Fortified;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Fortified
{
    public class IngestionOutcomeDoer_GiveThoughtWhenNoBionic : IngestionOutcomeDoer
    {
        public HediffDef BionicHediff;

        public ThoughtDef thoughtDef;

        public float severity = -1f;

        public ChemicalDef toleranceChemical;

        private bool divideByBodySize;

        public bool multiplyByGeneToleranceFactors;

        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount)
        {
            //沒有特定Hediff才會產生副作用
            if (!pawn.health.hediffSet.HasHediff(BionicHediff))
            {
                pawn.needs.mood.thoughts.memories.TryGainMemory(DMS_DefOf.DMS_OverEat, null, null);
            }
        }
        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(ThingDef parentDef)
        {
            if (!parentDef.IsDrug || !(chance >= 1f))
            {
                yield break;
            }
            foreach (StatDrawEntry item in thoughtDef.SpecialDisplayStats(StatRequest.ForEmpty()))
            {
                yield return item;
            }
        }
    }
}
