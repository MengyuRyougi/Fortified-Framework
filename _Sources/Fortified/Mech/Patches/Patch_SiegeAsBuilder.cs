using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI.Group;
using Verse.AI;
using UnityEngine;

namespace Fortified
{
    [HarmonyPatch(typeof(LordToil_Siege), "SetAsBuilder")]
    internal class Patch_SiegeAsBuilder
    {
        public static bool Prefix(LordToil_Siege __instance, Pawn p)
        {

            if (p.def.race.IsMechanoid)
            {
                __instance.SetAsDefender(p);
                return false;
            }

            return true;
        }
    }
    //[HarmonyPatch(typeof(LordToil_DefendBase), "UpdateAllDuties")]
    //internal class Patch_LordToil_DefendBase
    //{
    //    public static bool Prefix(LordToil_DefendBase __instance)
    //    {

    //        for (int i = 0; i < __instance.lord.ownedPawns.Count; i++)
    //        {
    //            if (__instance.lord.ownedPawns[i].def.race.IsMechanoid)
    //            {
    //                __instance.lord.ownedPawns[i].mindState.duty = new PawnDuty(DutyDefOf.Defend, __instance.baseCenter);
    //                return false;
    //            }
    //        }
    //        return true;
    //    }
    //}
}

