using CombatExtended;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace FortifiedCE
{
    /// <summary>
    /// CE 版武器用弧形掃射 Verb (對應原版 Fortified.Verb_ArcSprayProjectile)。
    /// 繼承 Verb_ShootCE，所以彈藥 (CompAmmoUser)、射擊模式 (CompFireModes)、後座力、
    /// 砲塔 (Building_TurretGunCE) 與 Pawn 持槍全部走 CE 原本的流程。
    ///
    /// 每一發都把 currentTarget 暫時換成掃射路徑上的格子再交給 Verb_ShootCE 開火：
    ///   - defaultProjectile / 彈藥投射物是 ProjectileCE → 走 CE 彈道 (含 CE 火焰噴射彈)
    ///   - 是原版 Projectile → 走原版 Projectile.Launch，仍會消耗 CE 彈藥
    /// sprayEffecterDef 每發都會從 caster 射向該格子，可用來畫火焰束 / 泡沫束之類的視覺效果。
    ///
    /// 路徑長度固定等於本次連發的實際射擊數 (ShotsPerBurst)，不再依賴 sprayNumExtraCells，
    /// 所以切換 CE 射擊模式 (單發 / 點放 / 全自動) 都不會超出範圍，而且整段弧線一定掃完。
    /// </summary>
    public class Verb_ArcSprayProjectileCE : Verb_ShootCE
    {
        protected List<IntVec3> path = new List<IntVec3>();
        protected Vector3 initialTargetPosition;

        private int CurrentPathIndex => Mathf.Clamp(ShotsPerBurst - burstShotsLeft, 0, path.Count - 1);

        // 砲塔頂 (TurretTop.DrawTurret) 與 Pawn 持槍角度都吃這個，讓槍口跟著掃射路徑轉
        public override float? AimAngleOverride
        {
            get
            {
                if (state == VerbState.Bursting && path.Count > 0)
                {
                    return (path[CurrentPathIndex].ToVector3Shifted() - caster.DrawPos).AngleFlat();
                }
                return null;
            }
        }

        public override void WarmupComplete()
        {
            // Verb_ShootCE.WarmupComplete 在瞄準模式下可能先延長暖機再回頭呼叫一次，
            // 兩次都重算路徑沒有副作用；真正開火前路徑一定已就緒。
            initialTargetPosition = currentTarget.CenterVector3;
            PreparePath();
            base.WarmupComplete();
        }

        public override bool TryCastShot()
        {
            if (path.Count == 0)
            {
                // 沒有路徑 (例如讀檔中途) 就退化成普通射擊
                return base.TryCastShot();
            }
            if (currentTarget.HasThing && currentTarget.Thing.Map != caster.Map)
            {
                return false;
            }

            IntVec3 cell = path[CurrentPathIndex];
            LocalTargetInfo originalTarget = currentTarget;
            currentTarget = new LocalTargetInfo(cell);
            try
            {
                verbProps.sprayEffecterDef?.Spawn(caster.Position, cell, caster.Map);

                ThingDef projectileDef = Projectile;
                if (projectileDef == null)
                {
                    return false;
                }

                bool fired;
                if (typeof(ProjectileCE).IsAssignableFrom(projectileDef.thingClass))
                {
                    // CE 投射物：完整走 Verb_ShootCE → Verb_LaunchProjectileCE 的彈道與彈藥流程
                    fired = base.TryCastShot();
                }
                else
                {
                    fired = TryCastVanillaProjectile(projectileDef, cell);
                }

                if (!fired && CompAmmo != null && !CompAmmo.CanBeFiredNow)
                {
                    // 沒彈藥了才真的中斷連發
                    return false;
                }
                // 個別格子沒有射線 (被牆擋住) 就跳過那一格，繼續掃下一格
                lastShotTick = Find.TickManager.TicksGame;
                return true;
            }
            finally
            {
                currentTarget = originalTarget;
            }
        }

        // 原版投射物回退：不走 CE 彈道，但仍然消耗 CE 彈藥
        private bool TryCastVanillaProjectile(ThingDef projectileDef, IntVec3 cell)
        {
            if (CompAmmo != null && !CompAmmo.TryPrepareShot())
            {
                return false;
            }
            Projectile projectile = (Projectile)GenSpawn.Spawn(projectileDef, caster.Position, caster.Map);
            projectile.Launch(caster, caster.DrawPos, cell, cell, ProjectileHitFlags.IntendedTarget, preventFriendlyFire, EquipmentSource);
            numShotsFired++;

            if (CompAmmo == null)
            {
                return true;
            }
            int ammoPerShot = (CompAmmo.Props.ammoSet?.ammoConsumedPerShot ?? 1) * VerbPropsCE.ammoConsumedPerShotCount;
            CompAmmo.Notify_ShotFired(ammoPerShot);
            if (ShooterPawn != null && !CompAmmo.CanBeFiredNow)
            {
                CompAmmo.TryStartReload();
            }
            if (!CompAmmo.HasMagazine && CompAmmo.UseAmmo)
            {
                return CompAmmo.Notify_PostShotFired();
            }
            return true;
        }

        // 與原版 Verb_ArcSpray.PreparePath 相同的散布邏輯，只是格子數改成跟本次連發射擊數一致
        protected virtual void PreparePath()
        {
            path.Clear();
            Vector3 normalized = (currentTarget.CenterVector3 - caster.Position.ToVector3Shifted()).Yto0().normalized;
            Vector3 tan = normalized.RotatedBy(90f);
            int extraCells = Mathf.Max(ShotsPerBurst - 1, 0);
            for (int i = 0; i < extraCells; i++)
            {
                for (int j = 0; j < 15; j++)
                {
                    float value = Rand.Value;
                    float num = Rand.Value - 0.5f;
                    float num2 = value * verbProps.sprayWidth * 2f - verbProps.sprayWidth;
                    float num3 = num * verbProps.sprayThicknessCells + num * 2f * verbProps.sprayArching;
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
