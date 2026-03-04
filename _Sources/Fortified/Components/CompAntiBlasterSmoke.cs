using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using UnityEngine;

namespace Fortified
{
    public class CompAntiBlasterSmoke : ThingComp
    {
        #region 反射缓存
        private static readonly FieldInfo s_originField =
            typeof(Projectile).GetField("origin", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo s_destField =
            typeof(Projectile).GetField("destination", BindingFlags.NonPublic | BindingFlags.Instance);
        #endregion

        #region 字段
        [Unsaved(false)]
        private Effecter effecter;
        private CompProperties_AntiBlasterSmoke Props => (CompProperties_AntiBlasterSmoke)props;
        private bool isActive = true;
        private int tickRemain = 100;
        #endregion

        private Thing EffecterSourceThing
        {
            get
            {
                ThingWithComps pawn = parent;
                if (!parent.Spawned)
                {
                    IThingHolder parentHolder = parent.ParentHolder;
                    if (parentHolder != null)
                    {
                        if (parentHolder is Pawn_ApparelTracker pawn_ApparelTracker)
                            pawn = pawn_ApparelTracker.pawn;
                        else if (parentHolder is Pawn_CarryTracker pawn_CarryTracker)
                            pawn = pawn_CarryTracker.pawn;
                        else if (parentHolder is Pawn_EquipmentTracker pawn_EquipmentTracker)
                            pawn = pawn_EquipmentTracker.pawn;
                    }
                }
                return pawn;
            }
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            effecter?.Cleanup();
            effecter = null;
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            tickRemain = Props.activeTicks;
        }

        public override void CompTick()
        {
            base.CompTick();
            if (!isActive) return;
            if (Props.effecterDef != null)
            {
                if (effecter == null)
                    effecter = Props.effecterDef.Spawn();
                effecter.EffectTick(EffecterSourceThing, TargetInfo.Invalid);
            }

            Map map = parent.Map;
            ThingGrid thingGrid = map.thingGrid;
            Vector3 parentDrawPos = parent.DrawPos;
            int size = Props.Size;

            CellRect rect = GenAdj.OccupiedRect(parent).ExpandedBy(size).ClipInsideMap(map);
            foreach (IntVec3 cell in rect)
            {
                List<Thing> things = thingGrid.ThingsListAt(cell);
                for (int i = 0; i < things.Count; i++)
                {
                    Thing thing = things[i];
                    if (!(thing is Projectile)) continue;
                    if (!IsTargetProjectile(thing)) continue;
                    if (Vector3.Distance(thing.DrawPos, parentDrawPos) >= size) continue;

                    if (Rand.Range(0f, 1f) > Props.chanceToFail)
                        DoIntercept(thing as Projectile, parentDrawPos, map);
                }
            }
            tickRemain--;
            if (tickRemain <= 0) isActive = false;
        }

        // 拦截处理
        private void DoIntercept(Projectile target, Vector3 parentDrawPos, Map map)
        {
            if (target == null) return;
            if (Props.fleckDef != null)
            {
                if (parentDrawPos.ShouldSpawnMotesAt(map, false))
                {
                    Vector3 originVector = (Vector3)s_originField.GetValue(target);
                    Vector3 destVector = (Vector3)s_destField.GetValue(target);
                    Vector3 velo = (destVector - originVector).normalized;

                    FleckCreationData dataStatic = FleckMaker.GetDataStatic(target.DrawPos, map, Props.fleckDef, Rand.Range(0.5f, 1.5f));
                    dataStatic.rotation = target.Rotation.AsAngle;
                    dataStatic.targetSize = 0;
                    dataStatic.velocityAngle = velo.ToAngleFlat();
                    dataStatic.velocitySpeed = Rand.Range(target.def.projectile.speed / 2, target.def.projectile.speed);
                    dataStatic.scale = 2;
                    map.flecks.CreateFleck(dataStatic);
                }
            }
            if (Props.spawnLeaving != null)
            {
                if (Rand.Range(0f, 1f) > 0.8f)
                    GenSpawn.Spawn(Props.spawnLeaving, target.Position, target.Map);
            }
            target.Destroy();
        }

        // 判断是否为可拦截的投射物
        private bool IsTargetProjectile(Thing target)
        {
            if (target is null) return false;
            if (!(target is Projectile)) return false;

            string defName = target.def.defName;
            if (defName.Contains("Charge") || defName.Contains("Blaster"))
            {
                return !Props.ignoreThings.Contains(defName);
            }
            // 白名单检查
            List<string> intercepts = Props.interceptThings;
            if (intercepts != null)
            {
                for (int i = 0; i < intercepts.Count; i++)
                {
                    if (defName == intercepts[i]) return true;
                }
            }
            return false;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref isActive, "isActive", true);
            Scribe_Values.Look(ref tickRemain, "tickRemain", 100);
        }
    }

    public class CompProperties_AntiBlasterSmoke : CompProperties
    {
        public int Size;
        public EffecterDef effecterDef;
        public FleckDef fleckDef;
        public ThingDef spawnLeaving;
        public float chanceToFail = 0.8f;
        public int activeTicks = 1500;
        public List<string> interceptThings;
        public List<string> ignoreThings;

        public CompProperties_AntiBlasterSmoke()
        {
            compClass = typeof(CompAntiBlasterSmoke);
        }
    }
}
