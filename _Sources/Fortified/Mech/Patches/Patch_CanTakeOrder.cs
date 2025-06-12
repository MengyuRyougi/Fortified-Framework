using Verse;
using RimWorld;

using HarmonyLib;

namespace Fortified
{

    //疑似是1.6不再需要了。
    //[HarmonyPatch(typeof(FloatMenuMakerMap),"CanTakeOrder")]
    //public static class Patch_CanTakeOrder
    //{
    //    [HarmonyPostfix]
    //    public static void AllowTakeOrder(Pawn pawn, ref bool __result)
    //    {
    //        if(__result) return;
    //        if (pawn is IWeaponUsable)
    //        {
    //            __result = true;
    //        }
    //    }
    //}
}