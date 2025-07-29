using HarmonyLib;
using RimWorld;
using Verse;

namespace Fortified
{
    [HarmonyPatch(typeof(CompOverseerSubject), "State",MethodType.Getter)]
    public static class Patch_CompOverseerSubject_State
    {
        [HarmonyPostfix]
        public static void Postfix(ref OverseerSubjectState __result, CompOverseerSubject __instance)
        {
            //如果是OverseerSubjectState.Overseen的話就不需要再檢查了。
            if (__result == OverseerSubjectState.Overseen) return;

            if (__instance.parent.TryGetComp<CompDeadManSwitch>() is CompDeadManSwitch comp && comp.woken)
            {
                __result = OverseerSubjectState.Overseen;
            }
        }
    }
}
