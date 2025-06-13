using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Fortified
{
    public class FloatMenuOptionProvider_WeaponUsable : FloatMenuOptionProvider
    {
        protected override bool Drafted => true;
        protected override bool Undrafted => true;
        protected override bool Multiselect => false;
        protected override bool MechanoidCanDo => true;
        public override bool TargetPawnValid(Pawn pawn, FloatMenuContext context)
        {
            return base.TargetPawnValid(pawn, context);
        }

        //實際開始判定前的可用性檢測(Tracker跟Type)
        protected override bool AppliesInt(FloatMenuContext context)
        {
            if (context.FirstSelectedPawn.equipment == null) return false;
            if (!context.FirstSelectedPawn.GetType().IsSubclassOf(typeof(IWeaponUsable))) return false;

            return true;
        }
        public override IEnumerable<FloatMenuOption> GetOptionsFor(Thing clickedThing, FloatMenuContext context)
        {
            var ext = context.FirstSelectedPawn.def.GetModExtension<MechWeaponExtension>();
            return Fortified.FloatMenuUtility.GetExtraFloatMenuOptionsFor(context, clickedThing, ext);
        }
    }
}