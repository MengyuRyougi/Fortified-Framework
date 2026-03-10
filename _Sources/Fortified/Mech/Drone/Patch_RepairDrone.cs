using HarmonyLib;
using RimWorld;
using Verse;

namespace Fortified
{
    /// <summary>
    /// Allows pawns with repair capability to repair mechs with CompDrone component.
    /// Patches the vanilla repair job validation to allow CompDrone-enabled pawns to be repaired.
    /// </summary>
    [HarmonyPatch(typeof(MechRepairUtility), nameof(MechRepairUtility.CanRepair))]
    internal static class Patch_RepairDrone_CanRepair
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn mech, ref bool __result)
        {
            if (__result) return;
            
            // Allow repair if this is a CompDrone pawn
            if (mech.TryGetComp<CompDrone>(out _))
            {
                __result = true;
            }
        }
    }
}
