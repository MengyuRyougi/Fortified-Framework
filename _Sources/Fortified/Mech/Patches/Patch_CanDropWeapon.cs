using Verse;
using RimWorld;
using HarmonyLib;

namespace Fortified
{
    [HarmonyPatch(typeof(ITab_Pawn_Gear), "CanControlColonist", MethodType.Getter)]
    public static class Patch_CanDropWeapon
    {
        [HarmonyPostfix]
        public static void CanControl(ref bool __result)
        {
            if (__result) return;
            if (Find.Selector.SingleSelectedThing is Pawn pawn)
            {
                if (pawn is IWeaponUsable && pawn.Faction != null && pawn.Faction.IsPlayer)
                {
                    __result = true;
                }
            }
        }
    }
}