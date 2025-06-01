using HarmonyLib;
using RimWorld;
using Verse;

namespace Fortified
{
    [HarmonyPatch(typeof(MechRepairUtility), "IsMissingWeapon")]
    internal class Patch_IsMissingWeapon
    {
        public static void Postfix(ref bool __result, Pawn mech)
        {
            if (mech is IWeaponUsable)
            {
                __result = false;
            }
        }
    }
}

