using Verse;
using RimWorld;
using UnityEngine;
using HarmonyLib;
using RimWorld.QuestGen;

namespace Fortified
{
    [DefOf]
    internal static class QuestDefOf
    {
        public static FactionDef DMS_Army = null;
        public static QuestScriptDef DMS_PromotionCeremony = null;
    }
    [HarmonyPatch(typeof(RoyalTitleUtility), nameof(RoyalTitleUtility.GenerateBestowingCeremonyQuest), MethodType.Normal)]
    internal static class Patch_GenerateBestowingCeremonyQuest //確保生成的NPC具有正確的官銜陣營
    {
        public static bool Prefix(Pawn pawn, Faction faction)
        {
            if (faction.def == QuestDefOf.DMS_Army)
            {
                Slate slate = new Slate();
                slate.Set("titleHolder", pawn);
                slate.Set("bestowingFaction", faction);
                if (QuestDefOf.DMS_PromotionCeremony.CanRun(slate))
                {
                    Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(QuestDefOf.DMS_PromotionCeremony, slate);
                    if (quest.root.sendAvailableLetter)
                    {
                        QuestUtility.SendLetterQuestAvailable(quest);
                    }
                }
                return false;
            }
            else return true;
        }
    }
}