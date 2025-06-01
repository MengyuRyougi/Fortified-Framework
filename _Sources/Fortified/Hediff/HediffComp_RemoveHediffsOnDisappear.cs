using Verse;

namespace Fortified
{
    /// <summary>
    /// 用來銷毀NACS的機控中樞
    /// </summary>
    public class HediffComp_RemoveHediffsOnDisappear : HediffComp_Disappears
    {
        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            base.Notify_PawnDied(dinfo, culprit);
            OnDeathHediffOperation();
        }
        private void OnDeathHediffOperation()
        {
            HediffsExtension extension = Def.GetModExtension<HediffsExtension>() ?? null;
            if (extension == null) return;
            for (int i = 0; i < this.Def.GetModExtension<HediffsExtension>().hediffs.Count; i++)
            {
                if (Pawn.health.hediffSet.HasHediff(extension.hediffs[i].hediffDef))
                {
                    Pawn.health.RemoveHediff(Pawn.health.hediffSet.GetFirstHediffOfDef(extension.hediffs[i].hediffDef));
                }
            }
        }
        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            OnDeathHediffOperation();
        }
    }
}
