using Verse;
using RimWorld;
using HarmonyLib;

namespace Fortified
{
    [HarmonyPatch(typeof(Building_TurretGun), "CanSetForcedTarget", (MethodType)1)]
    internal static class Patch_CanSetForcedTarget
    {
        public static void Postfix(ref bool __result, Building_TurretGun __instance)
        {
            if (__result) return;
            if (__instance.Faction != null && !__instance.Faction.IsPlayer) return;

            if (__instance.def.HasModExtension<ForceTargetableExtension>())
            {
                __result = true;
            }
        }
    }
}