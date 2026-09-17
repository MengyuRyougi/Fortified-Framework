using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Fortified
{
    // Def 基底：ThingDef / BuildableDef / TerrainDef / GeneDef / AbilityDef 都會呼叫 base，一個 Postfix 即涵蓋。
    // 不要再 patch 這些子類，否則 base 鏈會重複輸出。
    [HarmonyPatch(typeof(Def), nameof(Def.SpecialDisplayStats))]
    internal static class Patch_Def_SpecialDisplayStats_InfoDisplay
    {
        public static IEnumerable<StatDrawEntry> Postfix(
            IEnumerable<StatDrawEntry> values,
            Def __instance,
            StatRequest req)
        {
            foreach (StatDrawEntry entry in values)
                yield return entry;

            foreach (StatDrawEntry entry in InfoDisplayUtility.BuildEntries(__instance, req))
                yield return entry;
        }
    }

    // HediffDef 不呼叫 base，需獨立 Postfix
    [HarmonyPatch(typeof(HediffDef), nameof(HediffDef.SpecialDisplayStats))]
    internal static class Patch_HediffDef_SpecialDisplayStats_InfoDisplay
    {
        public static IEnumerable<StatDrawEntry> Postfix(
            IEnumerable<StatDrawEntry> values,
            HediffDef __instance,
            StatRequest req)
        {
            foreach (StatDrawEntry entry in values)
                yield return entry;

            foreach (StatDrawEntry entry in InfoDisplayUtility.BuildEntries(__instance, req))
                yield return entry;
        }
    }

    // Hediff 實例：Hediff 本身與 HediffComp 實例可實作 IInfoProvider 讀執行期狀態。
    // Def 層級的條目已由 HediffDef Postfix 輸出，這裡先標記為已見避免重複。
    [HarmonyPatch(typeof(Hediff), nameof(Hediff.SpecialDisplayStats))]
    internal static class Patch_Hediff_SpecialDisplayStats_InfoDisplay
    {
        public static IEnumerable<StatDrawEntry> Postfix(
            IEnumerable<StatDrawEntry> values,
            Hediff __instance,
            StatRequest req)
        {
            foreach (StatDrawEntry entry in values)
                yield return entry;

            HashSet<FFF_InfoDef> seen = new HashSet<FFF_InfoDef>();
            InfoDisplayUtility.MarkSeen(__instance.def, req, seen);

            foreach (StatDrawEntry entry in InfoDisplayUtility.BuildEntriesFromSources(
                         __instance.def, req, InfoDisplayUtility.HediffInstanceSources(__instance), seen))
                yield return entry;
        }
    }

    // Pawn 卡彙總：身上的 Hediff / 服裝 / 武器 / 基因所帶的機制直接顯示在 Pawn 訊息卡，標題加來源後綴。
    // race ThingDef 自身的條目已由 Def Postfix 輸出，先標記為已見；同一 InfoDef 只顯示一次。
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.SpecialDisplayStats))]
    internal static class Patch_Pawn_SpecialDisplayStats_InfoDisplay
    {
        public static IEnumerable<StatDrawEntry> Postfix(
            IEnumerable<StatDrawEntry> values,
            Pawn __instance)
        {
            foreach (StatDrawEntry entry in values)
                yield return entry;

            foreach (StatDrawEntry entry in InfoDisplayUtility.BuildPawnAggregateEntries(__instance))
                yield return entry;
        }
    }
}
