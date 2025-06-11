using RimWorld;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace Fortified
{
    public class CompFlyingFleckThrower : ThingComp
    {
        public CompProperties_FlyingFleckThrower Props => (CompProperties_FlyingFleckThrower)this.props;
        Pawn Pawn => parent as Pawn;
        public bool CanThrow => Pawn.Spawned && !Pawn.DeadOrDowned && Pawn.CarriedBy == null && IsActive;
        private bool IsActive => Pawn.GetMechWorkMode() != MechWorkModeDefOf.SelfShutdown && Pawn.GetMechWorkMode() != MechWorkModeDefOf.Recharge;

        private int interrival;
        public override void CompTick()
        {
            base.CompTick();
            if (!CanThrow)
            {
                if (Pawn.Drawer.renderer.HasAnimation) Pawn.Drawer.renderer.SetAnimation(null);
                return;
            }
            if (!Pawn.Drawer.renderer.HasAnimation) Pawn.Drawer.renderer.SetAnimation(Props.activeAnimation);

            if (interrival < Props.throwTick)
            {
                interrival++;
                return;
            }
            interrival = 0;

            for (int i = 0; i < Props.throwRate; i++)
            {
                if (ShouldSpawnFleck(out var fleck))
                {
                    fleck.altitudeLayer = AltitudeLayer.Filth;
                    float angle = GetAngle();
                    Vector3 p = DrawPos(angle);

                    FleckCreationData dataStatic = FleckMaker.GetDataStatic(p, base.parent.Map, fleck, Rand.Range(Props.sizeRange.x, Props.sizeRange.y));
                    dataStatic.rotationRate = Rand.Range(-30f, 30f);
                    dataStatic.velocityAngle = angle;
                    dataStatic.velocitySpeed = Rand.Range(Props.speedRange.x, Props.speedRange.y);
                    dataStatic.rotationRate = GetAngle();
                    if (Props.lifeTime != 0) dataStatic.solidTimeOverride = Props.lifeTime;
                    base.parent.Map.flecks.CreateFleck(dataStatic);
                }
            }
        }
        private bool ShouldSpawnFleck(out FleckDef fleck)
        {
            fleck = null;
            if (Pawn.DrawPos.ShouldSpawnMotesAt(Pawn.Map, false))
            {
                TerrainDef terrain = Pawn.Position.GetTerrain(parent.MapHeld);
                if (terrain.IsWater) return false;
                else if (parent.Map.snowGrid.GetDepth(Pawn.Position) > 0)
                {
                    fleck = Props.fleckOnSnow;
                }
                else if (terrain.affordances.Contains(DefDatabase<TerrainAffordanceDef>.GetNamed("Diggable")))
                {
                    fleck = Props.fleckOnSoil;
                }
                else fleck = Props.fleckDefault;
            }
            return fleck != null;
        }
        float GetAngle()
        {
            return Rand.Range(0, 360);
        }
        Vector3 DrawPos(float angle)
        {
            Vector3 offset = Vector3.zero;
            if (Props.burstOffsetRange != Vector2.zero)
            {
                float range = Verse.Rand.Range(Props.burstOffsetRange.x, Props.burstOffsetRange.y);
                offset = range * CircleConst.GetAngle(angle);
            }
            return parent.DrawPos + offset;
        }
    }
    public class CompProperties_FlyingFleckThrower : CompProperties
    {
        public CompProperties_FlyingFleckThrower() { this.compClass = typeof(CompFlyingFleckThrower); }
        public FleckDef fleckDefault = null;
        public FleckDef fleckOnSnow = null;
        public FleckDef fleckOnSoil = null;
        public AnimationDef activeAnimation = null;
        public int lifeTime = 0;
        public Vector2 burstOffsetRange = Vector2.zero;
        public Vector2 speedRange = Vector2.one;
        public Vector2 sizeRange = Vector2.one;
        public int throwRate = 5;
        public int throwTick = 5;
    }
}