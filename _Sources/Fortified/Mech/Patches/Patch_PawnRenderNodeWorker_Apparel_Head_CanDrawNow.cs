using Verse;
using HarmonyLib;

namespace Fortified
{
    [HarmonyPatch(typeof(PawnRenderNodeWorker_Apparel_Head), nameof(PawnRenderNodeWorker_Apparel_Head.CanDrawNow))]
    public static class Patch_PawnRenderNodeWorker_Apparel_Head_CanDrawNow
    {
        public static bool Prefix(PawnDrawParms parms, ref bool __result)
        {
            return !(parms.pawn is HumanlikeMech) || !(__result = true);
        }
    }
}
