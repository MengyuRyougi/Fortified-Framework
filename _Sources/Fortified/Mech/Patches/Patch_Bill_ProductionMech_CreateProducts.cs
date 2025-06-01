using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Fortified
{
    //確保由玩家所培育的(可裝備武器)機兵不會出廠自帶武器。

    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(Bill_ProductionMech), nameof(Bill_ProductionMech.CreateProducts))]
    static class Patch_Bill_ProductionMech_CreateProducts
    {
        static void Postfix(ref Thing __result)
        {
            if (__result is IWeaponUsable && __result is Pawn pawn)
            {
                pawn.inventory.DestroyAll();
                pawn.equipment.DestroyAllEquipment();
            }
        }
    }
}
