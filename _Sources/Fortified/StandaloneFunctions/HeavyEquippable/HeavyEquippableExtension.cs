using RimWorld;
using Verse;

namespace Fortified
{
    public class HeavyEquippableExtension : DefModExtension
    {
        public HeavyEquippableDef EquippableDef;

        public bool CanEquippedBy(Pawn pawn)//無論是其他種族還是機兵都吃這個判斷，但能夠從Tag層面上就能使用該武器的機兵不受限制
        {
            if (EquippableDef == null)
            {
                Log.Warning("HeavyEquippableExtension doesn't have a HeavyEquippableDef sets!");
                return true;
            }
            if (EquippableDef.EquippableByRace.Contains(pawn.def)) return true;
            if (pawn.BodySize >= EquippableDef.EquippableBaseBodySize && EquippableDef.EquippableBaseBodySize != -1) return true;
            if (CheckUtility.HasAnyApparelOf(pawn, EquippableDef.EquippableWithApparel)) return true;
            if (CheckUtility.HasAnyHediffOf(pawn, EquippableDef.EquippableWithHediff)) return true;
            return false;
        }
        public bool CanEquippedBy(ThingDef pawnRaceDef)//無論是其他種族還是機兵都吃這個判斷，但能夠從Tag層面上就能使用該武器的機兵不受限制
        {
            if (EquippableDef == null)
            {
                Log.Warning("HeavyEquippableExtension doesn't have a HeavyEquippableDef sets!");
                return true;
            }
            if (EquippableDef.EquippableByRace.Contains(pawnRaceDef)) return true;
            if (pawnRaceDef.race.baseBodySize >= EquippableDef.EquippableBaseBodySize && EquippableDef.EquippableBaseBodySize != -1) return true;
            return false;
        }
    }
}
