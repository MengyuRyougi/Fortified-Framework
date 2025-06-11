using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Fortified
{
    public abstract class AirSupportComp_SetOrigin : AirSupportComp
    {
        bool ignoreOverride = false;

        public override void Trigger(AirSupportDef def, Thing trigger, Map map, LocalTargetInfo target)
        {
            if (!ignoreOverride && def.originOverridedCache) return;
        }
    }
    public class AirSupportComp_SetOriginRandomEdge : AirSupportComp_SetOrigin
    {
        public override void Trigger(AirSupportDef def, Thing trigger, Map map, LocalTargetInfo target)
        {
            base.Trigger(def, trigger, map, target);

            def.tempOriginCache = CellFinder.RandomEdgeCell(map).ToVector3Shifted();
        }
    }
    public class AirSupportComp_SetOriginFromClosestBase : AirSupportComp_SetOrigin
    {
        FloatRange angleRange = new(-30, 30);
        public override void Trigger(AirSupportDef def, Thing trigger, Map map, LocalTargetInfo target)
        {
            base.Trigger(def, trigger, map, target);

            List<WorldObject> list = Find.WorldObjects.AllWorldObjects.Where(x => x is Settlement && x.Faction == Find.FactionManager.FirstFactionOfDef(QuestDefOf.DMS_Army)).ToList();
            if (list.NullOrEmpty()) CellFinder.RandomEdgeCell(map).ToVector3Shifted();
            list.OrderBy(x =>  map.GetRangeBetweenTiles(x.Tile)).ToList();
            list.Reverse();
            WorldObject worldObject = list.First();
            def.tempOriginCache = WorldAngleUtils.Position(map.GetAngleBetweenTiles(worldObject.Tile) + angleRange.RandomInRange, map);
            Messages.Message("DMS_AirSupportFromClosestBase".Translate(worldObject.Label, def.label), MessageTypeDefOf.NeutralEvent, false);
        }
    }

    public class AirSupportComp_SetOriginAngle : AirSupportComp_SetOrigin
    {
        FloatRange angleRange = new(0, 360);

        public override void Trigger(AirSupportDef def, Thing trigger, Map map, LocalTargetInfo target)
        {
            base.Trigger(def, trigger, map, target);

            def.tempOriginCache = WorldAngleUtils.Position(angleRange.RandomInRange, map);
        }
    }
}