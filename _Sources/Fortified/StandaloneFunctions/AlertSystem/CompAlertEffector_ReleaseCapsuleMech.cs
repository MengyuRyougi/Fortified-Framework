using RimWorld;
using Verse;
using Verse.Sound;

namespace Fortified
{
    /// <summary>
    /// 掛在 <see cref="Building_MechCapsule"/> 上：收到同地圖的警報訊號時，依 triggerChance 讓封存中的機兵敵對啟動。
    /// 玩家自己封存的機兵（容器屬玩家陣營）永遠不會被警報叫醒。
    /// For <see cref="Building_MechCapsule"/>: on an alarm from the same map, roll triggerChance to wake the
    /// mothballed mech as a hostile. Capsules owned by the player never react.
    /// </summary>
    public class CompProperties_AlertEffector_ReleaseCapsuleMech : CompProperties_AlertEffector
    {
        /// <summary>醒來後的陣營；null 則沿用容器（再退到機兵本身）的陣營。Faction on wake-up; null = the capsule's (then the mech's).</summary>
        public FactionDef spawnFactionDef;

        public SoundDef wakeSound;

        public EffecterDef wakeEffecter;

        /// <summary>醒來時的訊息 key，{0} = 機兵名稱；留空則不顯示。Message key on wake-up, {0} = mech label; empty = silent.</summary>
        [NoTranslate]
        public string messageKey = "FFF.CapsuleMechAlarmed";

        public CompProperties_AlertEffector_ReleaseCapsuleMech()
        {
            compClass = typeof(CompAlertEffector_ReleaseCapsuleMech);
        }
    }

    public class CompAlertEffector_ReleaseCapsuleMech : CompAlertEffector
    {
        public new CompProperties_AlertEffector_ReleaseCapsuleMech Props => (CompProperties_AlertEffector_ReleaseCapsuleMech)props;

        protected override void DoEffect()
        {
            if (!(parent is Building_MechCapsule capsule) || !capsule.Spawned || !capsule.HasMech) return;
            if (capsule.Faction == Faction.OfPlayer) return;

            Map map = capsule.Map;
            IntVec3 pos = capsule.Position;

            Faction faction = null;
            if (Props.spawnFactionDef != null)
            {
                faction = Find.FactionManager.FirstFactionOfDef(Props.spawnFactionDef);
            }

            Pawn mech = capsule.ReleaseHostile(faction);
            if (mech == null) return;

            Props.wakeSound?.PlayOneShot(new TargetInfo(pos, map));
            Props.wakeEffecter?.Spawn(pos, map).Cleanup();
            if (!Props.messageKey.NullOrEmpty())
            {
                Messages.Message(Props.messageKey.Translate(mech.LabelCap), mech, MessageTypeDefOf.ThreatSmall);
            }
        }
    }
}
