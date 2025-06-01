using System.Collections.Generic;
using Verse;

namespace Fortified
{
    public class HediffComp_GiveHediffWtihSeverity : HediffComp
	{
		private HediffCompProperties_GiveHediffWtihSeverity Props
		{
			get
			{
				return (HediffCompProperties_GiveHediffWtihSeverity)this.props;
			}
		}
        public override void CompPostMake()
        {
            HediffOperation(this.parent.pawn);
            base.CompPostMake();
        }
        private void HediffOperation(Pawn pawn)
        {
			for (int i = 0; i < this.Props.hediffDefAddSeverity.Count; i++)
			{
				if (this.Props.skipIfAlreadyExists && pawn.health.hediffSet.HasHediff(this.Props.hediffDefAddSeverity[i].hediffDef, false))
					continue;

				Hediff hediff = HediffMaker.MakeHediff(this.Props.hediffDefAddSeverity[i].hediffDef, this.parent.pawn);
				hediff.Severity = this.Props.hediffDefAddSeverity[i].severity;

                if (this.Props.hediffDefAddSeverity[i].isAdd)
                {
                    pawn.health.AddHediff(hediff, null, null, null);
                }
                else
                {
                    pawn.health.RemoveHediff(hediff);
                }
            }
		}
    }
    public class HediffDefWtihSeverity
    {
        public HediffDef hediffDef;
        public float severity = 0;
        public bool isAdd = true;
    }
    public class HediffCompProperties_GiveHediffWtihSeverity : HediffCompProperties
    {
        public HediffCompProperties_GiveHediffWtihSeverity()
        {
            this.compClass = typeof(HediffComp_GiveHediffWtihSeverity);
        }
        public List<HediffDefWtihSeverity> hediffDefAddSeverity;
        public bool skipIfAlreadyExists = true;
    }
}
