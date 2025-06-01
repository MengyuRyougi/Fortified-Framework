using Verse;
using HarmonyLib;
using RimWorld;

namespace Fortified
{
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.IsColonistPlayerControlled), MethodType.Getter)]
    public static class Patch_Pawn_IsColonistPlayerControlled
    {
        public static void Postfix(ref bool __result, Pawn __instance)
        {
            if (!__result)
            {
                if (__instance.IsColonyMechPlayerControlled)
                {
                    __result = true;
                }
            }
        }
    }
    [HarmonyPatch(typeof(Pawn_StyleTracker), nameof(Pawn_StyleTracker.CanDesireLookChange), MethodType.Getter)]
    public static class Patch_Pawn_StyleTracker_CanDesireLookChange
    {
        public static bool Prefix(Pawn_StyleTracker __instance, ref bool __result)
        {
            if (__instance.pawn is HumanlikeMech)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
}
