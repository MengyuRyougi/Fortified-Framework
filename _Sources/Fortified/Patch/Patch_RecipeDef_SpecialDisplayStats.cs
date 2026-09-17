using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Fortified;

[HarmonyPatch(typeof(RecipeDef), nameof(RecipeDef.SpecialDisplayStats))]
internal static class Patch_RecipeDef_SpecialDisplayStats
{
    public static IEnumerable<StatDrawEntry> Postfix(
        IEnumerable<StatDrawEntry> values,
        RecipeDef __instance,
        StatRequest req)
    {
        foreach (StatDrawEntry statDrawEntry in values)
            yield return statDrawEntry;
        IEnumerable<StatDrawEntry> stats = __instance
            .GetModExtension<ModExt_EnvironmentalBill>()?
            .SpecialDisplayStats();
        if (stats != null)
        {
            foreach (StatDrawEntry statDrawEntry in stats)
                yield return statDrawEntry;
        }
        // RecipeDef 不呼叫 base.SpecialDisplayStats，特殊機制條目在此追加
        foreach (StatDrawEntry statDrawEntry in InfoDisplayUtility.BuildEntries(__instance, req))
            yield return statDrawEntry;
    }
}