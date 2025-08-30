using HarmonyLib;
using RimWorld;
using Verse;

namespace Fortified
{
    public class Harmony_BillUtility
    {
        [HarmonyPatch(typeof(BillUtility), "MakeNewBill")]
        static class MakeNewBill_PostFix
        {
            [HarmonyPostfix]
            static void PostFix(RecipeDef recipe, ref Bill __result)
            {
                if (recipe.HasModExtension<ModExt_EnvironmentalBill>())
                {
                    __result = new Bill_Production_Environmental(recipe, null);
                }
            }
        }
    }
}