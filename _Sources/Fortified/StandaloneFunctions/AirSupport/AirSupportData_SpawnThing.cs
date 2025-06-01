using Verse;

namespace Fortified
{
    public class AirSupportData_SpawnThing : AirSupportData
    {
        public ThingDef thingDef;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref thingDef, "thingDef");
        }

        public override void Trigger()
        {
            var t = ThingMaker.MakeThing(thingDef);
            PreProcess(t);
            GenSpawn.Spawn(t, target.Cell, map);
        }

        protected virtual void PreProcess(Thing t)
        {

        }
    }
}
