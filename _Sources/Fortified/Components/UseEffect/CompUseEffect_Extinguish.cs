using RimWorld;
using Verse;

namespace Fortified
{
    public class CompUseEffect_Extinguish : CompUseEffect
    {
        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);
            GenExplosion.DoExplosion(parent.Position, parent.Map, 4.9f, DamageDefOf.Extinguish, null, 999, -1f, SoundDefOf.Explosion_FirefoamPopper, parent.def, null, null, RimWorld.ThingDefOf.Filth_FireFoam, 1f, 1, null, applyDamageToExplosionCellsNeighbors: true, null, 0f, 0, 0f, damageFalloff: false, null, null, null, doVisualEffects: true, 1f, 0f, doSoundEffects: true, null, 0f);
        }
    }
}