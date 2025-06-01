using HarmonyLib;
using RimWorld;
using System;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Fortified
{

    [HarmonyPatch(typeof(PawnComponentsUtility), "AddComponentsForSpawn")]
    public static class Patch_MechInteracte
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn pawn)
        {
            if (pawn.TryGetComp<CompDeadManSwitch>() is CompDeadManSwitch comp && comp.woken)
            {
                if (pawn.interactions == null)
                {
                    pawn.interactions = new Pawn_InteractionsTracker(pawn);
                }
            }
        }
    }
}
