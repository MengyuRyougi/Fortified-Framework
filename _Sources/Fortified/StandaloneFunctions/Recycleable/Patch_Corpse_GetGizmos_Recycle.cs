using System.Collections.Generic;
using HarmonyLib;
using Verse;

namespace Fortified;

/// <summary>
/// 屍體本身沒有 CompRecycleable（comp 掛在裡面的 pawn 上），這裡把拆解 gizmo 補到屍體的 gizmo 清單裡。
/// </summary>
[HarmonyPatch(typeof(Corpse), nameof(Corpse.GetGizmos))]
internal static class Patch_Corpse_GetGizmos_Recycle
{
    [HarmonyPostfix]
    private static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Corpse __instance)
    {
        foreach (Gizmo g in __result)
        {
            yield return g;
        }
        CompRecycleable comp = CompRecycleable.Get(__instance);
        if (comp == null) yield break;
        foreach (Gizmo g in comp.RecycleGizmos())
        {
            yield return g;
        }
    }
}
