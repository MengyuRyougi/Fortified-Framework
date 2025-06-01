using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Fortified
{
    public class AirSupportDef : Def
    {
        public List<AirSupportComp> comps = new List<AirSupportComp>();

        public Vector3 tempOriginCache = Vector3.zero;

        public bool originOverridedCache;

        public void Trigger(Thing trigger, Map map, LocalTargetInfo target)
        {
            originOverridedCache = tempOriginCache != Vector3.zero;
            foreach (AirSupportComp comp in comps)
            {
                comp.Trigger(this, trigger, map, target);
            }
            //Clear cache afterwards so that it's possible to set origin beforehand.
            tempOriginCache = Vector3.zero;
        }
        public void DrawHighlight(Map map, IntVec3 callerPos, LocalTargetInfo target)
        {
            foreach (AirSupportComp comp in comps)
            {
                comp.DrawHighlight(map, callerPos, target);
            }
        }
    }
}
