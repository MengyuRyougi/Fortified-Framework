using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using static HarmonyLib.Code;
using Verse.Noise;
using UnityEngine;
using System.Linq;

namespace Fortified
{
    public class CompExplosiveOnMelee : ThingComp
    {
        Pawn Pawn => parent as Pawn;
        public override string CompInspectStringExtra()
        {
            return "FFF.ExplosiveOnDead".Translate();//機體在死亡時會引爆攜帶的可爆炸物。
        }
        public override void Notify_UsedVerb(Pawn pawn, Verb verb)
        {
            if (verb.IsMeleeAttack)
            {
                if (pawn.IsPlayerControlled && verb.CurrentTarget.TryGetPawn(out var p) && p.IsPlayerControlled) return;
                ExplodeEquipment();
                ExplodeInventory();
                if (detonated) parent.Kill(null, null);
            }
            base.Notify_UsedVerb(pawn, verb);
        }
        private bool detonated = false;
        protected void ExplodeEquipment()
        {
            if (Pawn.equipment == null || Pawn.equipment.Primary == null) return;

            if (Pawn.equipment.Primary.TryGetComp<CompExplosive>(out var comp))
            {
                Detonate(comp);
            }
        }
        protected void ExplodeInventory()
        {
            if (Pawn.inventory == null || Pawn.inventory.innerContainer.NullOrEmpty()) return;
            List<Thing> tmpThings = new List<Thing>();
            foreach (var item in Pawn.inventory.innerContainer)
            {
                if (item.TryGetComp<CompExplosive>(out var comp))
                {
                    tmpThings.Add(item);
                }
            }
            if (tmpThings.NullOrEmpty()) return;
            foreach (var thing in tmpThings)
            {
                Detonate(thing.TryGetComp<CompExplosive>());
            }
            Pawn.inventory.DestroyAll();
        }
        protected virtual void Detonate(CompExplosive comp)
        {
            detonated = true;
            var compProperties_Explosive = comp.Props as CompProperties_Explosive;
            var Props = comp.Props;
            var map = parent.Map;

            if (comp.parent.def.projectileWhenLoaded != null)
            {
                ThingDef i = comp.parent.def.projectileWhenLoaded;
                if (i.HasComp<CompExplosive>())
                {
                    compProperties_Explosive = i.GetCompProperties<CompProperties_Explosive>();
                    Props = i.GetCompProperties<CompProperties_Explosive>();
                }
            }

            IntVec3 positionHeld = parent.PositionHeld;
            DamageDef explosiveDamageType = compProperties_Explosive.explosiveDamageType;
            int damageAmountBase = compProperties_Explosive.damageAmountBase;
            float armorPenetrationBase = compProperties_Explosive.armorPenetrationBase;
            SoundDef explosionSound = compProperties_Explosive.explosionSound;
            ThingDef postExplosionSpawnThingDef = compProperties_Explosive.postExplosionSpawnThingDef;
            float postExplosionSpawnChance = compProperties_Explosive.postExplosionSpawnChance;
            int postExplosionSpawnThingCount = compProperties_Explosive.postExplosionSpawnThingCount;
            GasType? postExplosionGasType = Props.postExplosionGasType;
            float? postExplosionGasRadiusOverride = Props.postExplosionGasRadiusOverride;
            int postExplosionGasAmount = Props.postExplosionGasAmount;
            bool applyDamageToExplosionCellsNeighbors = compProperties_Explosive.applyDamageToExplosionCellsNeighbors;
            ThingDef preExplosionSpawnThingDef = compProperties_Explosive.preExplosionSpawnThingDef;
            float preExplosionSpawnChance = compProperties_Explosive.preExplosionSpawnChance;
            int preExplosionSpawnThingCount = compProperties_Explosive.preExplosionSpawnThingCount;
            float chanceToStartFire = compProperties_Explosive.chanceToStartFire;
            bool damageFalloff = compProperties_Explosive.damageFalloff;
            List<Thing> ignoredThings = null;
            bool doVisualEffects = compProperties_Explosive.doVisualEffects;
            bool doSoundEffects = compProperties_Explosive.doSoundEffects;
            float propagationSpeed = compProperties_Explosive.propagationSpeed;
            ThingDef preExplosionSpawnSingleThingDef = compProperties_Explosive.preExplosionSpawnSingleThingDef;
            ThingDef postExplosionSpawnSingleThingDef = compProperties_Explosive.postExplosionSpawnSingleThingDef;
            GenExplosion.DoExplosion(positionHeld, map, comp.ExplosiveRadius(), explosiveDamageType, this.parent, damageAmountBase, armorPenetrationBase, explosionSound, null, null, null, postExplosionSpawnThingDef, postExplosionSpawnChance, postExplosionSpawnThingCount, postExplosionGasType, postExplosionGasRadiusOverride, postExplosionGasAmount, applyDamageToExplosionCellsNeighbors, preExplosionSpawnThingDef, preExplosionSpawnChance, preExplosionSpawnThingCount, chanceToStartFire, damageFalloff, null, ignoredThings, null, doVisualEffects, propagationSpeed, 0f, doSoundEffects, null, 1f, null, null, postExplosionSpawnSingleThingDef, preExplosionSpawnSingleThingDef);
        }
    }
}