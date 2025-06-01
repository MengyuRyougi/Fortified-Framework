using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using Verse.AI;
//using VFE.Mechanoids;
//using VFE.Mechanoids.AI.JobGivers;
//using VFE.Mechanoids.Needs;
//using VFECore;
//using VFEMech;

namespace Fortified
{
    /*
     
    [HarmonyPatch(typeof(CompMachineChargingStation))]
   
    static class CompMachineChargingStation_Patch
    {
        [HarmonyPatch(nameof(CompMachineChargingStation.PostSpawnSetup))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> CompMachineChargingStation_PostDeSpawn(IEnumerable<CodeInstruction> instructions)
        {

            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Call, m_ClearCache);
            foreach (var item in instructions)
            {
                yield return item;
            }

        }
        static MethodInfo m_ClearCache = SymbolExtensions.GetMethodInfo((CompMachineChargingStation comp) => CompMachineChargingStation_Patch.ClearCache(comp));
        public static void ClearCache(CompMachineChargingStation comp)
        {
            if (comp == null || comp.parent == null) return;
            //Log.Message($"cachedChargingStationsDict: {CompMachineChargingStation.cachedChargingStationsDict.Count}; cachedChargingStations: {CompMachineChargingStation.cachedChargingStations.Count}");
            foreach (var (key,val) in CompMachineChargingStation.cachedChargingStationsDict)
            {
                if (val.parent.thingIDNumber == comp.parent.thingIDNumber)
                {
                    CompMachineChargingStation.cachedChargingStations.Remove(val);
                    CompMachineChargingStation.cachedChargingStationsDict.Remove(key);
                    return;
                }
            }
            //Log.Error($"Unable to find a comp for thingid {comp.parent.ThingID} in CompMachineChargingStation");

        }
    }

    [HarmonyPatch(typeof(CompMachine))]
    [HarmonyPatch(nameof(CompMachine.OnBuildingDestroyed))]
    static class CompMachine_Patch {
        [HarmonyPrefix]
        static bool NoDeadKill(CompPawnDependsOn compPawnDependsOn)
        {
            return compPawnDependsOn.MyPawnIsAlive;
        }

    }
    [HarmonyPatch(typeof(JobGiver_ReturnToStationIdle),"TryGiveJob")]
    static class JobGiver_ReturnToStationIdle_TryGiveJob
    {
        [HarmonyPrefix]
        static bool Prefix(Pawn pawn,ref Job __result)
        {
            Building myBuilding = CompMachine.cachedMachinesPawns[pawn].myBuilding;
            Need_Power need_Power = pawn.needs.TryGetNeed<Need_Power>();

            if (need_Power != null && need_Power.CurLevelPercentage <= maxLevelPercentage && myBuilding != null && myBuilding.Spawned)
            {

                if (myBuilding.Map == pawn.Map && pawn.CanReserveAndReach(myBuilding, PathEndMode.OnCell, Danger.Deadly))
                {
                    if (pawn.Position != myBuilding.Position)
                    {
                        __result = JobMaker.MakeJob(JobDefOf.Goto, myBuilding.Position);
                        return true;
                    }
                    pawn.Rotation = Rot4.South;
                    if (myBuilding.TryGetComp<CompPowerTrader>().PowerOn)
                    {
                        __result = JobMaker.MakeJob(VFEDefOf.VFE_Mechanoids_Recharge, myBuilding);
                        return true;
                    }
                }


            }
            __result = JobMaker.MakeJob(JobDefOf.Wait, 600, false);
            return false;
        }
        private const float maxLevelPercentage = 0.99f;
    }
    [HarmonyPatch(typeof(ThinkNode_ConditionalHasPower), "Satisfied")]
    static class ThinkNode_ConditionalHasPower_Satisfied
    {
        static Dictionary<Pawn, Need_Power> cachedPawnPowerNeed = new Dictionary<Pawn, Need_Power>();
        static bool Prefix(Pawn pawn,ref bool __result)
        {
            if (cachedPawnPowerNeed.TryGetValue(pawn,out var n))
            {
                __result = n.CurLevel > 0f;
            }
            else
            {
                cachedPawnPowerNeed[pawn] = pawn.needs.TryGetNeed<Need_Power>();
                __result= cachedPawnPowerNeed[pawn].CurLevel > 0f;
            }
            return false;
        }
    }
     */
}