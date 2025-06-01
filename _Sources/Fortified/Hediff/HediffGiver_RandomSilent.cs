using Verse;

namespace Fortified
{
    public class HediffGiver_RandomSilent : HediffGiver
    {
        public float mtbDays;

        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            float num = mtbDays;
            float num2 = ChanceFactor(pawn);
            if (num2 != 0f && Rand.MTBEventOccurs(num / num2, 60000f, 60f) && TryApply(pawn))
            {
                //SendLetter(pawn, cause);
            }
        }
    }
}
