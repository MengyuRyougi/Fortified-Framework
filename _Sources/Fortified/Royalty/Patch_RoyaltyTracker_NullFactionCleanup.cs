using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Fortified
{
    // 陣營被移除（temporary faction 隨任務清理、或存檔缺少對應 mod）後，
    // 原版 FactionManager.Remove 只會把 pawn.Faction 設為 null，
    // 不會清掉 Pawn_RoyaltyTracker 裡的 titles / factionPermits。
    // 這些項目在下次讀檔時 faction 引用會解析成 null，接著：
    //   - ExposeData 的 RemoveAll → HasPermit(def, null) 每次讀檔都刷
    //     "Cannot get current title for null faction"
    //   - AssignHeirIfNone(def, null) → heirs.ContainsKey(null) 直接拋 ArgumentNullException
    // 這裡在 PostLoadInit 進入原版邏輯前，把 faction 已經是 null 的項目剔除。
    // 只動明確壞掉的資料，不會影響正常的官銜或許可證。
    [HarmonyPatch(typeof(Pawn_RoyaltyTracker), nameof(Pawn_RoyaltyTracker.ExposeData))]
    internal static class Patch_RoyaltyTracker_NullFactionCleanup
    {
        private static readonly AccessTools.FieldRef<Pawn_RoyaltyTracker, List<RoyalTitle>> titlesRef =
            AccessTools.FieldRefAccess<Pawn_RoyaltyTracker, List<RoyalTitle>>("titles");

        private static readonly AccessTools.FieldRef<Pawn_RoyaltyTracker, List<FactionPermit>> permitsRef =
            AccessTools.FieldRefAccess<Pawn_RoyaltyTracker, List<FactionPermit>>("factionPermits");

        [HarmonyPrefix]
        static void Prefix(Pawn_RoyaltyTracker __instance, Pawn ___pawn)
        {
            // 跨引用已在 ResolvingCrossRefs 階段解析完畢，PostLoadInit 時 null 即代表陣營不存在。
            if (Scribe.mode != LoadSaveMode.PostLoadInit) return;

            int removedTitles = titlesRef(__instance)?.RemoveAll(t => t != null && t.faction == null) ?? 0;
            int removedPermits = permitsRef(__instance)?.RemoveAll(p => p != null && p.Faction == null) ?? 0;

            if (removedTitles > 0 || removedPermits > 0)
            {
                Log.Warning($"[FFF] Removed {removedTitles} title(s) and {removedPermits} permit(s) with a missing faction from {___pawn?.LabelShort ?? "unknown pawn"}.");
            }
        }
    }
}
