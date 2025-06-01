using Verse;

namespace Fortified
{
    public class AirSupportData_Effecter : AirSupportData
    {
        public EffecterDef effecterDef;

        public bool onTriggerer = false, hasTargetB = true;

        public override void Trigger()
        {
            if (effecterDef.maintainTicks > 0)
            {
                if (onTriggerer) effecterDef.SpawnMaintained(triggerer, hasTargetB ? target.ToTargetInfo(map) : triggerer);
                else effecterDef.SpawnMaintained(target.ToTargetInfo(map), hasTargetB ? triggerer : target.ToTargetInfo(map));
            }
            else
            {
                if (onTriggerer)
                {
                    if (hasTargetB) effecterDef.Spawn(triggerer, target.ToTargetInfo(map));
                    else effecterDef.SpawnAttached(triggerer, map);
                }
                else effecterDef.Spawn(target.ToTargetInfo(map), hasTargetB ? triggerer : target.ToTargetInfo(map));
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref effecterDef, "effecterDef");
            Scribe_Values.Look(ref onTriggerer, "onTriggerer");
            Scribe_Values.Look(ref hasTargetB, "hasTargetB");
        }
    }
}
