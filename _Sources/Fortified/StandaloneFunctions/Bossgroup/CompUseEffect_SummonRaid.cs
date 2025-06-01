using RimWorld;
using Verse;

namespace Fortified
{
    public class CompUseEffect_SummonRaid : CompUseEffect
    {
        CompProperties_UseEffectSummonRaid Props => props as CompProperties_UseEffectSummonRaid;

        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);

            Props.effecterDef?.Spawn(parent.Position, parent.Map);
            GameComponent_Bossgroup component = Current.Game.GetComponent<GameComponent_Bossgroup>();
            if (component != null)
            {
                Props.bossgroupDef.Worker.Resolve(parent.Map, component.NumTimesCalledBossgroup(Props.bossgroupDef));
            }
        }
    }
    public class CompProperties_UseEffectSummonRaid : CompProperties_UseEffect
    {
        public CompProperties_UseEffectSummonRaid()
        {
            compClass = typeof(CompUseEffect_SummonRaid);
        }

        public BossgroupDef bossgroupDef;
        public EffecterDef effecterDef;
    }
}
