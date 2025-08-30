using RimWorld;
using Verse;
using HarmonyLib;
using System.Collections.Generic;

namespace Fortified;

[HarmonyPatch(typeof(IncidentWorker), "TryExecute")]
public static class Patch_IncidentWorkerForDevice
{
    public static bool Prefix(IncidentWorker __instance, IncidentParms parms)
    {
        if (CompShieldingDevice.GetTowerByIncident(__instance.def).EnumerableNullOrEmpty())
        {
            return true;
        }
        Thing thing = CompShieldingDevice.GetTowerByIncident(__instance.def).RandomElement();
        if (thing != null)
        {
            //TODO
        }
        return true;
    }
}