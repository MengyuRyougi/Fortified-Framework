using Verse;
using Verse.Sound;

namespace Fortified
{
    public class AirSupportData_Sound : AirSupportData
    {
        public SoundDef soundDef;

        public override void Trigger()
        {
            soundDef.PlayOneShot(SoundInfo.InMap(target.ToTargetInfo(map)));
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref soundDef, "soundDef");
        }
    }
    public class AirSupportData_SoundFromPlane : AirSupportData_Sound, IAttachedToFlyBy
    {
        public FlyByThing plane;
        FlyByThing IAttachedToFlyBy.plane
        {
            set => plane = value;
        }
        public override void Trigger()
        {
            if (plane != null && plane.Spawned) target = plane.DrawPos.ToIntVec3();
            base.Trigger();
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref plane, "plane");
        }
    }
}
