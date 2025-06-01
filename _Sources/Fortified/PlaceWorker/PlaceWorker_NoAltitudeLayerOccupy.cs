using RimWorld;
using System.Linq;
using Verse;

namespace Fortified;

public class PlaceWorker_NoAltitudeLayerOccupy : PlaceWorker
{
    public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
    {
        if ((from x in loc.GetThingList(map)
             where x.def.altitudeLayer == checkingDef.altitudeLayer
             select x).ToList().Count > 0)
        {
            return "FT_AltitudeLayerOccupied".Translate();
        }
        return true;
    }
}
