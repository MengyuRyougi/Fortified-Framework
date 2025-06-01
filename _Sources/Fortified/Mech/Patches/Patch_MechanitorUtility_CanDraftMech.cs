using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Fortified
{
    [HarmonyPatch(typeof(MechanitorUtility), nameof(MechanitorUtility.CanDraftMech), MethodType.Normal)]
    internal static class Patch_MechanitorUtility_CanDraftMech
    {
        static void Postfix(Pawn mech, ref AcceptanceReport __result)
        {
            if (__result == true || mech.DeadOrDowned) return;
            if ((!mech.IsColonyMech && mech.HostFaction == null)) return;

            if (mech.TryGetComp<CompDeadManSwitch>() is CompDeadManSwitch comp && comp.woken)
            {
                __result = true;
            }
            else if (mech.HostFaction == Faction.OfPlayer)
            {
                __result = true;
            }

            if (mech.kindDef.race.HasComp(typeof(CompCommandRelay)))
            {
                __result = true;
                return;
            }
            List<Pawn> overseenPawns = MechanitorUtility.GetOverseer(mech)?.mechanitor?.OverseenPawns;
            if (overseenPawns.NullOrEmpty()) return;
            List<Pawn> commandRelay = overseenPawns.Where(temp => temp.GetComp<CompCommandRelay>() != null).ToList();
            if (commandRelay.NullOrEmpty()) return;

            foreach (Pawn pawn in overseenPawns.Where(p => p.MapHeld == mech.MapHeld))
            {
                if (commandRelay.Contains(pawn))
                {
                    __result = true;
                    return;
                }
            }
        }
    }
}
