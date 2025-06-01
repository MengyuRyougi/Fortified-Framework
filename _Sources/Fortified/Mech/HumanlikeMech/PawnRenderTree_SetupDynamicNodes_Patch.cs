using Verse;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using System.Linq;
using System;

namespace Fortified
{
    [HarmonyPatch(typeof(PawnRenderTree), "SetupDynamicNodes")]
    public class PawnRenderTree_SetupDynamicNodes_Patch
    {
        private static void Prefix(PawnRenderTree __instance, Pawn ___pawn)
        {
            if (___pawn is HumanlikeMech)
            {
                Traverse.Create((object)__instance).Method("SetupApparelNodes", Array.Empty<object>()).GetValue();
                //__instance.SetupApparelNodes(); 嗚，我是笨蛋，學不會用UnsafeAccesser
            }
        }
    }
    [HarmonyPatch(typeof(PawnRenderTree), "ShouldAddNodeToTree")]
    static class PawnRenderTree_ShouldAddNodeToTree_Patch
    {
        private static void Postfix(ref bool __result, PawnRenderNodeProperties props, Pawn ___pawn)
        {
            if (__result == true) return;
            if (___pawn is HumanlikeMech && (props.workerClass== typeof(PawnRenderNodeWorker_Apparel_Body) || props.workerClass ==  typeof(PawnRenderNodeWorker_Apparel_Head)))
            {
                __result = true;
                return;
            }
        }
    }
    
}
