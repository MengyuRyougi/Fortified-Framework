using RimWorld;
using Verse;
using HarmonyLib;

namespace Fortified;

[HarmonyPatch(typeof(IncidentWorker), "TryExecute")]
public static class Patch_IncidentWork
{
    public static bool Prefix(IncidentWorker __instance, IncidentParms parms)
    {
        if (CompSignalTower.GetTowerByIncident(__instance.def).EnumerableNullOrEmpty())
        {
            return true;
        }
        Thing thing = CompSignalTower.GetTowerByIncident(__instance.def).RandomElement();
        if (thing != null)
        {
            parms.target = thing.Map;
        }
        return true;
    }
}
