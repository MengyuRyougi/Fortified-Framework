using RimWorld;
using Verse;

namespace Fortified
{
    public class FloatMenuOptionProvider_AwakenMech : FloatMenuOptionProvider
    {
        protected override bool Drafted => true;
        protected override bool Undrafted => true;
        protected override bool Multiselect => false;

        //實際開始判定前的可用性檢測(Tracker跟Type)
        public override bool Applies(FloatMenuContext context)
        {
            if (!base.Applies(context)) return false;
            return context.FirstSelectedPawn.TryGetComp<CompDeadManSwitch>() != null && AwakenCheck(context);
        }
        protected bool AwakenCheck(FloatMenuContext context)
        {
            return context.FirstSelectedPawn.GetComp<CompDeadManSwitch>().woken;
        }
    }
}