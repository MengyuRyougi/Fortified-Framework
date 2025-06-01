using Mono.Unix.Native;
using RimWorld;
using RimWorld.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static HarmonyLib.Code;

namespace Fortified
{
    public class Verb_CastAbilityArcSprayProjectile : Verb_CastAbility
    {
        protected List<IntVec3> path = new List<IntVec3>();

        protected Vector3 initialTargetPosition;

        protected override int ShotsPerBurst => verbProps.burstShotCount;

        public override float? AimAngleOverride
        {
            get
            {
                if (state == VerbState.Bursting && Available())
                {
                    return (path[ShotsPerBurst - burstShotsLeft].ToVector3Shifted() - caster.DrawPos).AngleFlat();
                }
                return null;
            }
        }
        protected override bool TryCastShot()
        {
            if (burstShotsLeft == verbProps.burstShotCount && !base.TryCastShot())
            {
                return false;
            }
            if (currentTarget.HasThing && currentTarget.Thing.Map != caster.Map)
            {
                return false;
            }
            if (verbProps.stopBurstWithoutLos && !TryFindShootLineFromTo(caster.Position, currentTarget, out _))
            {
                return false;
            }
            if (base.EquipmentSource != null && burstShotsLeft <= 1)
            {
                base.EquipmentSource.GetComp<CompChangeableProjectile>()?.Notify_ProjectileLaunched();
                base.EquipmentSource.GetComp<CompApparelReloadable>()?.UsedOnce();
            }
            HitCell(path[ShotsPerBurst - burstShotsLeft]);
            lastShotTick = Find.TickManager.TicksGame;
            return true;
        }
        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            base.ValidateTarget(target, showMessages);
            if (!ReloadableUtility.CanUseConsideringQueuedJobs(CasterPawn, base.EquipmentSource))
            {
                return false;
            }
            return true;
        }
        public override bool Available()
        {
            return ShotsPerBurst - burstShotsLeft >= 0;
        }

        public override void WarmupComplete()
        {
            burstShotsLeft = ShotsPerBurst;
            state = VerbState.Bursting;
            initialTargetPosition = currentTarget.CenterVector3;
            PreparePath();
            TryCastNextBurstShot();
        }
        protected void PreparePath()
        {
            path.Clear();
            Vector3 normalized = (currentTarget.CenterVector3 - caster.Position.ToVector3Shifted()).Yto0().normalized;
            Vector3 tan = normalized.RotatedBy(90f);
            for (int i = 0; i < verbProps.sprayNumExtraCells; i++)
            {
                for (int j = 0; j < 15; j++)
                {
                    float value = Rand.Value;
                    float num = Rand.Value - 0.5f;
                    float num2 = value * verbProps.sprayWidth * 2f - verbProps.sprayWidth;
                    float num3 = num * (float)verbProps.sprayThicknessCells + num * 2f * verbProps.sprayArching;
                    IntVec3 item = (currentTarget.CenterVector3 + num2 * tan - num3 * normalized).ToIntVec3();
                    if (!path.Contains(item) || Rand.Value < 0.25f)
                    {
                        path.Add(item);
                        break;
                    }
                }
            }
            path.Add(currentTarget.Cell);
            path.SortBy((IntVec3 c) => (c.ToVector3Shifted() - caster.DrawPos).Yto0().normalized.AngleToFlat(tan));
        }

        protected virtual void HitCell(IntVec3 cell)
        {
            verbProps.sprayEffecterDef?.Spawn(caster.Position, cell, caster.Map);
            ((Projectile)GenSpawn.Spawn(verbProps.defaultProjectile, caster.Position, caster.Map, WipeMode.Vanish)).Launch(caster, caster.DrawPos, cell, cell, ProjectileHitFlags.All, false, EquipmentSource ?? null, null);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref path, "path", LookMode.Value);
            Scribe_Values.Look(ref initialTargetPosition, "initialTargetPosition");
            if (Scribe.mode == LoadSaveMode.PostLoadInit && path == null)
            {
                path = new List<IntVec3>();
            }
        }
    }
}