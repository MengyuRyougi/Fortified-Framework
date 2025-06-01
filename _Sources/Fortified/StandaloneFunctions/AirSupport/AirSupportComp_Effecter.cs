using Verse;

namespace Fortified
{
    public class AirSupportComp_Effecter : AirSupportComp
    {
        public EffecterDef effecterDef;

        public bool onTriggerer = false, hasTargetB = true;

        public override void Trigger(AirSupportDef def, Thing triggerer, Map map, LocalTargetInfo target)
        {
            GameComponent_CAS.AddData(new AirSupportData_Effecter()
            {
                effecterDef = effecterDef,
                onTriggerer = onTriggerer,
                hasTargetB = hasTargetB,
                map = map,
                target = target,
                triggerTick = Find.TickManager.TicksGame,
                triggerer = triggerer,
                triggerFaction = triggerer.Faction,
            });
        }
    }
}
