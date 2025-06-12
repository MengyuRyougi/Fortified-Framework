using Verse;
using HarmonyLib;

namespace Fortified
{
    //[HarmonyPatch(typeof(Pawn), nameof(Pawn.IsColonyMechPlayerControlled), MethodType.Getter)]
    //internal static class Patch_IsColonyMechPlayerControlled
    //{
    //    internal static void Postfix(Pawn __instance, ref bool __result)
    //    {
    //        if (__result) return;
    //        if (__instance is IWeaponUsable && __instance.Spawned && __instance.IsColonyMech && !__instance.DeadOrDowned) __result = true;
    //    }
    //}
}