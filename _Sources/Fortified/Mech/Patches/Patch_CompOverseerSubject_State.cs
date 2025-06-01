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
            if (__instance.parent.TryGetComp<CompDeadManSwitch>() is CompDeadManSwitch comp && comp.woken)
            {
                __result = OverseerSubjectState.Overseen;
            }

            //甚至不確定有沒有效。
            else if (__instance.parent is Pawn p && p.HostFaction == Faction.OfPlayer)
            {
                __result = OverseerSubjectState.Overseen;
            }
        }
    }
}
