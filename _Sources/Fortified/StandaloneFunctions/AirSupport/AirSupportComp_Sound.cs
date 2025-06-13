using RimWorld;
using Verse;

namespace Fortified
{
    public class AirSupportComp_Sound : AirSupportComp
    {
        public bool useTempOriginCache;

        public float spreadRadius = 5;

        public int burstCount = 1;

        public IntRange delayRange = new(0, 30);

        public SoundDef soundDef;

        public override void Trigger(AirSupportDef def, Thing triggerer, Map map, LocalTargetInfo target)
        {
            var c = GenRadial.NumCellsInRadius(spreadRadius);
            for (int i = 0; i < burstCount; i++)
            {
                GameComponent_CAS.AddData(new AirSupportData_Sound()
                {
                    soundDef = soundDef,
                    map = map,
                    target = useTempOriginCache ? def.tempOriginCache.ToIntVec3() : target.Cell + GenRadial.RadialPattern[Rand.RangeInclusive(0, c)],
                    triggerTick = Find.TickManager.TicksGame + delayRange.RandomInRange,
                    triggerer = triggerer,
                    triggerFaction = triggerer.Faction,
                });
            }
        }
    }
}
