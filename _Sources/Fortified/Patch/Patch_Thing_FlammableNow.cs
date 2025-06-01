using Verse;
using HarmonyLib;
using System;

namespace Fortified
{
    //機體不可燃。

    [HarmonyPatch(typeof(Thing), "get_FlammableNow")]
    public static class Patch_Thing_FlammableNow
    {
        [HarmonyPostfix]
        static void Postfix(ref bool __result, Thing __instance)
        {
            if (__result && __instance is IWeaponUsable)
            {
                __result = false;
            }
        }
    }
}
