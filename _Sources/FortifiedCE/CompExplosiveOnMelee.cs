using RimWorld;
using System.Collections.Generic;
using Verse;
using CombatExtended;
using System.Reflection;

namespace FortifiedCE
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

            if (Pawn.equipment.Primary.TryGetComp<CompExplosiveCE>(out var comp))
            {
                Detonate(comp);
            }
            else if (Pawn.equipment.Primary.TryGetComp<CompExplosive>(out var comp2))
            {
                Detonate(comp2);
            }
        }
        protected void ExplodeInventory()
        {
            if (Pawn.inventory == null || Pawn.inventory.innerContainer.NullOrEmpty()) return;
            List<Thing> tmpThings = new List<Thing>();
            foreach (var item in Pawn.inventory.innerContainer)
            {
                if (item.HasComp<CompExplosiveCE>() || item.HasComp<CompExplosive>())
                {
                    tmpThings.Add(item);
                }
            }

            if (tmpThings.NullOrEmpty()) return;
            foreach (var thing in tmpThings)
            {
                if(thing.TryGetComp<CompExplosiveCE>(out var comp))Detonate(comp);
                else if (thing.TryGetComp<CompExplosive>(out var comp2)) Detonate(comp2);
            }
            Pawn.inventory.DestroyAll();
        }

        public static CompProperties_ExplosiveCE GetProps(CompExplosiveCE instance)
        {
            if (instance == null) return null;
            PropertyInfo propInfo = typeof(CompExplosiveCE).GetProperty("Props", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (propInfo != null)
            {
                return propInfo.GetValue(instance) as CompProperties_ExplosiveCE;
            }
            return null;
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
        protected virtual void Detonate(CompExplosiveCE comp)
        {
            detonated = true;
            var compProperties_Explosive = GetProps(comp);
            var Props = GetProps(comp);
            var map = parent.Map;

            if (comp.parent.def.projectileWhenLoaded != null)
            {
                ThingDef i = comp.parent.def.projectileWhenLoaded;
                if (i.HasComp<CompExplosiveCE>())
                {
                    compProperties_Explosive = i.GetCompProperties<CompProperties_ExplosiveCE>();
                    Props = i.GetCompProperties<CompProperties_ExplosiveCE>();
                }
            }
            if (Props == null)
            {
                Log.Error($"CompExplosiveOnMelee: {comp.parent} has no CompProperties_ExplosiveCE defined.");
                return;
            }

            IntVec3 positionHeld = parent.PositionHeld;
            DamageDef explosiveDamageType = compProperties_Explosive.explosiveDamageType;
            int damageAmountBase = (int)compProperties_Explosive.damageAmountBase;
            float armorPenetrationBase = compProperties_Explosive.GetExplosionArmorPenetration();
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
            bool doVisualEffects = false;
            bool doSoundEffects = compProperties_Explosive.explosionSound != null;
            float propagationSpeed = compProperties_Explosive.fragSpeedFactor;
            ThingDef preExplosionSpawnSingleThingDef = compProperties_Explosive.preExplosionSpawnThingDef;
            ThingDef postExplosionSpawnSingleThingDef = compProperties_Explosive.postExplosionSpawnThingDef;

            GenExplosionCE.DoExplosion(positionHeld, map, Props.explosiveRadius, explosiveDamageType, this.parent, damageAmountBase, armorPenetrationBase, explosionSound, null, null,null, postExplosionSpawnThingDef, postExplosionSpawnChance, postExplosionSpawnThingCount, postExplosionGasType, postExplosionGasRadiusOverride, postExplosionGasAmount, applyDamageToExplosionCellsNeighbors, preExplosionSpawnThingDef, preExplosionSpawnChance, preExplosionSpawnThingCount, chanceToStartFire, damageFalloff, null, ignoredThings, null, doVisualEffects, propagationSpeed, 0f, doSoundEffects, null, 1f, null, null, postExplosionSpawnSingleThingDef, preExplosionSpawnSingleThingDef);
        }
    }
}
