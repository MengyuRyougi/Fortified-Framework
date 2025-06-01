using Verse;
using RimWorld;
using HarmonyLib;
using System;

namespace Fortified
{
    [StaticConstructorOnStartup]
    [HarmonyPatch(new Type[] { typeof(PawnGenerationRequest) })]
    [HarmonyPatch(typeof(PawnGenerator))]
    [HarmonyPatch(nameof(PawnGenerator.GeneratePawn))]

    internal static class Patch_GeneratePawnTitle //確保生成的NPC具有正確的官銜陣營
    {
        public static void Postfix(ref Pawn __result)
        {
            if (!ModsConfig.RoyaltyActive) return;
            if (__result.ageTracker.AgeBiologicalYears < 1) return;
            if (!__result.kindDef.HasModExtension<DefaultTilteFactionExtension>()) return;

            Faction faction = Find.FactionManager?.FirstFactionOfDef(__result.kindDef.GetModExtension<DefaultTilteFactionExtension>().faction);
            if (faction != null)
            {
                foreach (RoyalTitle item in __result.royalty.AllTitlesForReading)
                {
                    item.faction = faction;
                }
            }
        }
    }
}