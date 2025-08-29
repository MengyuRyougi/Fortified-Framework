using Verse;
using RimWorld;

using HarmonyLib;

namespace Fortified
{
    //[HarmonyPatch(typeof(Pawn), nameof(Pawn.CanTakeOrder), MethodType.Getter)]
    //public static class Patch_Pawn_CanTakeOrder
    //{
    //    [HarmonyPostfix]
    //    public static void CanTakeOrder(Pawn __instance, ref bool __result)
    //    {
    //        if (__result) return;
    //        if (__instance is IWeaponUsable && __instance.IsColonyMechPlayerControlled)
    //        {
    //            __result = true;
    //        }
    //    }
    //}
}