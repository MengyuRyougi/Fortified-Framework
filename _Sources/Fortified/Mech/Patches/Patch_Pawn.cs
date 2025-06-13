using HarmonyLib;
using RimWorld;
using Verse;

namespace Fortified
{
    //用來防止敵對機兵掉落自帶武器
    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.DropAndForbidEverything))]
    internal static class Patch_Pawn
    {
        [HarmonyPrefix]
        static bool PreFix(Pawn __instance)
        {
            if (__instance is IWeaponUsable && __instance.Faction != Faction.OfPlayer)
            {
                __instance.equipment?.DestroyAllEquipment();
                __instance.apparel?.DestroyAll();
            }
            return true;
        }
    }
}
