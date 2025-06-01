using HarmonyLib;
using RimWorld;

namespace Fortified
{
    [HarmonyPatch(typeof(Building_Turret))]
    internal static class Patch_ThreatDisabled
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Building_Turret), "ThreatDisabled")]
        public static void PostBuilding_Turret_ThreatDisabled(ref bool __result, Building_Turret __instance)
        {
            if ((__instance as Building_TurretCapacity)?.CanEnter ?? false)
            {
                __result = true;
            }
        }
    }
}