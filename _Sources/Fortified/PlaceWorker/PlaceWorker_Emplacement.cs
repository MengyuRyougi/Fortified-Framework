using RimWorld;
using System;
using System.Text;
using System.Xml;
using Verse;

namespace Fortified;
public class PlaceWorker_Emplacement : PlaceWorker
{
    public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
    {
        Building edifice = loc.GetEdifice(map);
        if (edifice == null || edifice.def.graphicData.linkType != 0)
        {
            Building edifice2 = loc.GetEdifice(map);
            if (edifice2 != null && edifice2.def.fillPercent > 0.9f)
            {
                return "FFF.Message.MustOnACoverBuilding".Translate();
            }
        }
        return true;
    }
}