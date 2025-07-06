using Verse;
using Verse.AI;

namespace Fortified
{
    public class ThinkNode_ConditionalHasWeapon : ThinkNode_Conditional
    {
        public bool onlyRanged = false;
        public override ThinkNode DeepCopy(bool resolve = true)
        {
            ThinkNode_ConditionalHasWeapon copy = (ThinkNode_ConditionalHasWeapon)base.DeepCopy(resolve);
            copy.onlyRanged = this.onlyRanged;
            return copy;
        }
        protected override bool Satisfied(Pawn pawn)
        {
            if (onlyRanged)
            {
                return pawn.equipment?.Primary?.def.IsRangedWeapon == true;
            }
            else if (pawn.TryGetComp<CompExplosiveOnMelee>() != null) return true;//FPV走近戰的。
            else
            {
                return pawn.equipment?.Primary != null;
            }
        }
    }
}