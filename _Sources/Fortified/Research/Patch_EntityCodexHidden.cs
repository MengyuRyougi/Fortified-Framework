using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Fortified
{
    /// <summary>
    /// 解除巨石未激活時對自訂知識分類研究的隱藏。
    ///
    /// 原生 <see cref="EntityCodex.Hidden(ResearchProjectDef)"/> 在 Anomaly 啟用時，只要專案帶有
    /// <c>knowledgeCategory</c>、巨石尚未激活（HighestLevelReached &lt; 1）且本局會生成巨石，就一律回傳隱藏。
    /// 這條規則是為 Anomaly 自己的 Basic/Advanced 研究樹設計的，但它並不區分分類來源，
    /// 因此掛在 <see cref="ModExtension_UniqueResearchTab"/> 分頁上的自訂分類（例如 Occultech）
    /// 在玩家激活巨石之前也會整頁顯示為（未知研究）且無法研究。
    ///
    /// 這裡只在「原生判定為隱藏、且該分類屬於 FFF 自訂分頁」時介入，改為只套用實體圖鑑（EntityCodexEntryDef）
    /// 的探明規則；Anomaly 自身的分類與 debug_UnhideAllResearch 行為完全不受影響。
    /// FFF 自己的探明機制走 <see cref="Patch_ResearchProjectDef_IsHidden"/>，在本 postfix 之後另行疊加。
    /// </summary>
    [HarmonyPatch(typeof(EntityCodex), nameof(EntityCodex.Hidden), typeof(ResearchProjectDef))]
    public static class Patch_EntityCodex_Hidden
    {
        // 專案 -> 宣告「研究此專案時探明」的圖鑑條目；null 代表沒有任何條目綁定。
        // Def 在 DefDatabase 生命週期內固定，可安全快取，避免每 frame × 每專案掃整份 DefDatabase。
        private static Dictionary<ResearchProjectDef, EntityCodexEntryDef> codexEntryCache;

        [HarmonyPostfix]
        public static void Postfix(EntityCodex __instance, ResearchProjectDef def, ref bool __result)
        {
            // 原生已判定為可見（含 debug_UnhideAllResearch）就不需要再算。
            if (!__result || def == null || def.knowledgeCategory == null)
            {
                return;
            }
            // 只處理 FFF 自訂分頁擁有的分類；Anomaly 的 Basic/Advanced 維持原生巨石門檻。
            if (ResearchTabUtility.FindTabForCategory(def.knowledgeCategory) == null)
            {
                return;
            }

            // 跳過巨石門檻，僅保留原生的實體圖鑑探明規則。
            EntityCodexEntryDef entry = GetCodexEntry(def);
            __result = entry != null && !__instance.Discovered(entry);
        }

        private static EntityCodexEntryDef GetCodexEntry(ResearchProjectDef def)
        {
            if (codexEntryCache == null)
            {
                codexEntryCache = new Dictionary<ResearchProjectDef, EntityCodexEntryDef>();
            }
            if (codexEntryCache.TryGetValue(def, out EntityCodexEntryDef cached))
            {
                return cached;
            }

            EntityCodexEntryDef found = null;
            foreach (EntityCodexEntryDef entryDef in DefDatabase<EntityCodexEntryDef>.AllDefsListForReading)
            {
                if (entryDef.discoveredResearchProjects.NotNullAndContains(def))
                {
                    found = entryDef;
                    break;
                }
            }
            codexEntryCache[def] = found;
            return found;
        }
    }
}
