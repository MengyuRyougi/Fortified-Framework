using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Fortified
{
    public class CompAbilityEffect_SelfRepairMode : CompAbilityEffect
    {
        public new CompProperties_AbilitySelfRepairMode Props => (CompProperties_AbilitySelfRepairMode)props;
        public override bool CanCast => base.CanCast && parent.pawn.IsPlayerControlled && IsInjuredAndAlive() && NeedsRepair();
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn pawn = target.Pawn;
            if (pawn == null) return;

            List<Hediff> hediffs = (from Hediff item in target.Pawn.health.hediffSet.hediffs.Where(p => p is Hediff_MissingPart) select item).ToList();
            if (hediffs.NullOrEmpty()) return;

            foreach (var item in hediffs)
            {
                float dmg = Rand.Range(10, 18);
                target.Pawn.health.RemoveHediff(item);
                if (item.Part.def.hitPoints * pawn.HealthScale > dmg)//避免低血量部位永遠修不好
                {
                    DamageInfo damage = new DamageInfo(DamageDefOf.ElectricalBurn, dmg, 0, -1, null, item.Part);
                    target.Pawn.TakeDamage(damage);
                }        
            }
        }
        private bool IsInjuredAndAlive()
        {
            Pawn pawn = parent.pawn;
            return pawn != null && pawn.Spawned && !pawn.Dead;
        }

        /// <summary>
        /// 維修模式的主要效果是靠 DMS_SelfRepair 的 HediffComp_MechHeal 治療 Hediff_Injury，
        /// 本 comp 的 Apply 只是額外補回缺失部位，所以不能只用 Hediff_MissingPart 當門檻，
        /// 否則沒斷肢、只是受傷的機兵會完全無法啟動維修模式。
        /// </summary>
        private bool NeedsRepair()
        {
            List<Hediff> hediffs = parent.pawn?.health?.hediffSet?.hediffs;
            if (hediffs == null)
            {
                return false;
            }

            for (int i = 0; i < hediffs.Count; i++)
            {
                Hediff hediff = hediffs[i];
                if (hediff is Hediff_Injury || hediff is Hediff_MissingPart)
                {
                    return true;
                }
            }
            return false;
        }
    }
    public class CompProperties_AbilitySelfRepairMode : CompProperties_AbilityEffect
    {
        public CompProperties_AbilitySelfRepairMode()
        {
            compClass = typeof(CompAbilityEffect_SelfRepairMode);
        }
    }
}
