using Verse;

namespace Fortified
{
    public class ModExtension_QualityChance : DefModExtension
    {
        public float qualityChance = 0.25f; // Default chance of getting a quality item
        public float GetQualityChance()
        {
            return qualityChance;
        }
    }
}