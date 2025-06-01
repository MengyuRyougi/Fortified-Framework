using Verse;
using RimWorld;
using System.Reflection;
using HarmonyLib;

namespace Fortified
{
    //這個是給防化服的，效果是瘟疫事件時不將穿戴者視為感染目標。

    [HarmonyPatch(typeof(MetalhorrorUtility), nameof(MetalhorrorUtility.CanBeInfected))]
    internal static class Patch_CanBeInfected
    {
        public static void Postfix(ref bool __result, Pawn pawn)
        {
            if (__result)
            {
                if (pawn.apparel.AnyApparel)
                {
                    foreach (ThingWithComps apparel in pawn.apparel.WornApparel)
                    {
                        if (apparel.def.GetModExtension<BiochemicalProtectionExtension>() != null) __result = false;
                    }
                }
            }
        }
    }
}
