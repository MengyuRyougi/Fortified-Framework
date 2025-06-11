using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Fortified
{
    public class PawnFlyerWithEffect:PawnFlyer
    {
        protected override void RespawnPawn()
        {
            ModExtensionJumper me = def.GetModExtension<ModExtensionJumper>();
            if(me.compExplosive!=null)
            {
                
                var compProperties_Explosive = me.compExplosive;
                if (compProperties_Explosive.explosionEffect != null)
                {
                    Effecter effecter = compProperties_Explosive.explosionEffect.Spawn();
                    effecter.Trigger(new TargetInfo(Position, Map), new TargetInfo(Position, Map));
                    effecter.Cleanup();
                }
                GenExplosion.DoExplosion(Position, Map, compProperties_Explosive.explosiveRadius, compProperties_Explosive.explosiveDamageType, this, compProperties_Explosive.damageAmountBase, compProperties_Explosive.armorPenetrationBase, compProperties_Explosive.explosionSound, null, null, null, compProperties_Explosive.postExplosionSpawnThingDef, compProperties_Explosive.postExplosionSpawnChance, compProperties_Explosive.postExplosionSpawnThingCount, compProperties_Explosive.postExplosionGasType,null,0, compProperties_Explosive.applyDamageToExplosionCellsNeighbors, compProperties_Explosive.preExplosionSpawnThingDef, compProperties_Explosive.preExplosionSpawnChance, compProperties_Explosive.preExplosionSpawnThingCount, compProperties_Explosive.chanceToStartFire, compProperties_Explosive.damageFalloff, null, new List<Thing>() {FlyingPawn }, null, compProperties_Explosive.doVisualEffects, doSoundEffects: compProperties_Explosive.doSoundEffects, propagationSpeed: compProperties_Explosive.propagationSpeed);
            }
            base.RespawnPawn();
        }
    }
}
