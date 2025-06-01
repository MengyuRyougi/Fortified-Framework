using RimWorld;
using UnityEngine;
using Verse;

namespace Fortified
{
    public class CompFlyByThrownFleckEmitter : ThingComp
    {
        public bool emittedBefore;

        public int ticksSinceLastEmitted;

        private CompProperties_ThrownFleckEmitter Props => (CompProperties_ThrownFleckEmitter)props;

        private Vector3 EmissionOffset => new Vector3(Rand.Range(Props.offsetMin.x, Props.offsetMax.x), Rand.Range(Props.offsetMin.y, Props.offsetMax.y), Rand.Range(Props.offsetMin.z, Props.offsetMax.z));

        private Color EmissionColor => Color.Lerp(Props.colorA, Props.colorB, Rand.Value);

        public override void CompTick()
        {
            if (parent is not FlyByThing vehicle)
            {
                return;
            }
            if (Props.emissionInterval != -1)
            {
                if (ticksSinceLastEmitted >= Props.emissionInterval)
                {
                    Emit(vehicle);
                    ticksSinceLastEmitted = 0;
                }
                else
                {
                    ticksSinceLastEmitted++;
                }
            }
            else if (!emittedBefore)
            {
                Emit(vehicle);
                emittedBefore = true;
            }
        }

        private void Emit(FlyByThing v)
        {
            for (int i = 0; i < Props.burstCount; i++)
            {
                var pos = v.DrawPos + EmissionOffset.RotatedBy(v.angle);
                pos.y = Altitudes.AltitudeFor(Props.fleck.altitudeLayer);
                FleckCreationData dataStatic = FleckMaker.GetDataStatic(pos, parent.Map, Props.fleck, Props.scale.RandomInRange);
                dataStatic.rotationRate = Props.rotationRate.RandomInRange;
                dataStatic.instanceColor = EmissionColor;
                dataStatic.velocityAngle = Props.velocityX.RandomInRange + v.angle;
                dataStatic.velocitySpeed = Props.velocityY.RandomInRange;
                parent.Map.flecks.CreateFleck(dataStatic);
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref ticksSinceLastEmitted, "ticksSinceLastEmitted", 0);
            Scribe_Values.Look(ref emittedBefore, "emittedBefore", defaultValue: false);
        }
    }
}
