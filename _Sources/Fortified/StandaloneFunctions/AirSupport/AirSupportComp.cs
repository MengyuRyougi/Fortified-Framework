using Verse;

namespace Fortified
{
    public abstract class AirSupportComp
    {
        public virtual void DrawHighlight(Map map, IntVec3 callerPos, LocalTargetInfo target)
        {
        }
        public abstract void Trigger(AirSupportDef def, Thing trigger, Map map, LocalTargetInfo target);
    }
}
