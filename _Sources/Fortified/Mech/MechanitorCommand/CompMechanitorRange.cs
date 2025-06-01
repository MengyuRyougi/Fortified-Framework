using RimWorld;
using UnityEngine;
using Verse;

namespace Fortified
{
    //根據機械體與機械師的距離
    public class CompMechanitorRange : ThingComp
    {
        private HediffCompProperties_MechanitorRange Props
        {
            get
            {
                return (HediffCompProperties_MechanitorRange)this.props;
            }
        }
        private float severity;
        public override void CompTickRare()
        {
            base.CompTickRare();
            if (Pawn.Drafted)
            {
                if (SameMap)
                {
                    float mr = MechanitorRange;
                    severity =
                               mr < Props.minMaxRange.min ? Props.minMaxSeverity.min ://如果低於最小值，用最小值
                               mr > Props.minMaxRange.max ? Props.minMaxSeverity.max ://如果高於最大值,用最大值
                               Mathf.Lerp(Props.minMaxSeverity.min, Props.minMaxSeverity.max, mr / Props.minMaxRange.max);//如果在中間的話，用Lerp計算
                }
                else severity = Props.minMaxSeverity.max;//不在同一張地圖，用最大值
                Pawn.health.AddHediff(Props.hediffToGive);
                var h = Pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediffToGive);
                h.Severity = severity;
            }
            else
            {             
                Pawn.health.RemoveHediff(HediffMaker.MakeHediff(Props.hediffToGive, Pawn));
            }
        }
        bool SameMap => Pawn.Map == Pawn.GetOverseer().Map;
        Pawn Pawn => this.parent as Pawn;
        float MechanitorRange => Vector2.Distance(this.parent.Position.ToVector3(), this.Pawn.GetOverseer().Position.ToVector3());
    }
    public class HediffCompProperties_MechanitorRange : CompProperties
    {
        public HediffDef hediffToGive;
        public IntRange minMaxRange = new IntRange(5, 15);//距離越近值應該越小
        public FloatRange minMaxSeverity = new FloatRange(0.001f, 1f);//越大越差
        public HediffCompProperties_MechanitorRange() { compClass = typeof(CompMechanitorRange); }
    }
}
