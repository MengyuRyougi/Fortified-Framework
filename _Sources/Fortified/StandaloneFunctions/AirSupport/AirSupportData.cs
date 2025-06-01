using RimWorld;
using System;
using UnityEngine.Analytics;
using Verse;
using Verse.Noise;
using static HarmonyLib.Code;

namespace Fortified
{
    public abstract class AirSupportData : IExposable
    {
        public Map map;

        public LocalTargetInfo target;

        public int triggerTick;

        public Thing triggerer;

        public Faction triggerFaction;

        public virtual void ExposeData()
        {
            Scribe_References.Look(ref map, "map");
            Scribe_References.Look(ref triggerer, "triggerer");
            Scribe_References.Look(ref triggerFaction, "triggerFaction");
            Scribe_TargetInfo.Look(ref target, "target");
            Scribe_Values.Look(ref triggerTick, "triggerTick");
        }

        public abstract void Trigger();
    }

    public interface IAttachedToFlyBy
    {
        FlyByThing plane { set; }
    }
}
