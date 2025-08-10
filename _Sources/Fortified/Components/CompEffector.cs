using RimWorld;
using UnityEngine;
using Verse;

namespace Fortified
{
    public class CompEffector : ThingComp
    {
        private Effecter _effect;
        public CompProperties_Effector Props => props as CompProperties_Effector;

        private CompPowerTrader compPower; 
        private bool PowerOn => compPower == null || compPower.PowerOn;
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            compPower = parent.TryGetComp<CompPowerTrader>();
            if (_effect == null)
            {
                MakeEffector();
            }
        }
        public override void CompTickInterval(int delta)
        {
            if (!parent.Spawned) return;
            _effect.EffectTick(parent, parent);
        }
        private void MakeEffector()
        {
            _effect?.Cleanup();
            _effect = Props.effecter.SpawnAttached(parent, parent.Map);
            _effect.offset = Props.offset;
        }

        public override void Notify_DefsHotReloaded()
        {
            base.Notify_DefsHotReloaded();
            MakeEffector();
        }

        public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
        {
            _effect.Cleanup();
            base.PostDeSpawn(map, mode);
        }
    }
    public class CompProperties_Effector : CompProperties
    {
        public EffecterDef effecter;
        public Vector3 offset;

        public CompProperties_Effector()
        {
            compClass = typeof(CompEffector);
        }
    }
}