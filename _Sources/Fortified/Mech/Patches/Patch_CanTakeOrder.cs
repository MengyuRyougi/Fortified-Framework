using Verse;
using RimWorld;

using HarmonyLib;

namespace Fortified
{

    [HarmonyPatch(typeof(FloatMenuMakerMap),"CanTakeOrder")]
    public static class Patch_CanTakeOrder
    {
        [HarmonyPostfix]
        public static void AllowTakeOrder(Pawn pawn, ref bool __result)
        {
            if(__result) return;
            if (pawn is IWeaponUsable)
            {
                __result = true;
            }
        }
    }
}