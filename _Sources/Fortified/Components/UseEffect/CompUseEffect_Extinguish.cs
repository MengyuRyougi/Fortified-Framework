using RimWorld;
using Verse;

namespace Fortified
{
    public class CompUseEffect_Extinguish : CompUseEffect
    {
        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);
            GenExplosion.DoExplosion(
                center: parent.Position,
                map: parent.Map,
                radius: 4.9f,
                damType: DamageDefOf.Extinguish,
                instigator: null,
                damAmount: 999,
                armorPenetration: -1f,
                explosionSound: SoundDefOf.Explosion_FirefoamPopper,
                weapon: parent.def,
                projectile: null,
                intendedTarget: null,
                postExplosionSpawnThingDef: RimWorld.ThingDefOf.Filth_FireFoam,
                postExplosionSpawnChance: 1f,
                postExplosionSpawnThingCount: 1,
                postExplosionGasType: null,
                postExplosionGasRadiusOverride: null,
                postExplosionGasAmount: 0,
                applyDamageToExplosionCellsNeighbors: true,
                preExplosionSpawnThingDef: null,
                preExplosionSpawnChance: 0f,
                preExplosionSpawnThingCount: 0,
                chanceToStartFire: 0f,
                damageFalloff: false,
                direction: null,
                ignoredThings: null,
                affectedAngle: null,
                doVisualEffects: true,
                propagationSpeed: 1f,
                excludeRadius: 0f,
                doSoundEffects: true);
        }
    }
}