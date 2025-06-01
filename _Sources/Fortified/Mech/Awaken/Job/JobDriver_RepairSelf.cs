using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse.AI;
using Verse;

namespace Fortified
{
    public class JobDriver_RepairSelf : JobDriver
    {
        protected int TicksPerHeal
        {
            get
            {
                return Mathf.RoundToInt(1f / this.pawn.GetStatValue(StatDefOf.MechRepairSpeed, true, -1) * 120f);
            }
        }
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            if (!ModLister.CheckBiotech("Mech repair"))
            {
                yield break;
            }
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnForbidden(TargetIndex.A);
            Toil toil = Toils_General.Wait(int.MaxValue, TargetIndex.None);
            toil.WithEffect(EffecterDefOf.MechRepairing, TargetIndex.A, null);
            toil.PlaySustainerOrSound( SoundDefOf.RepairMech_Touch, 1f);
            toil.AddPreInitAction(delegate
            {
                this.ticksToNextRepair = this.TicksPerHeal;
            });
            toil.handlingFacing = true;
            toil.tickAction = delegate ()
            {
                this.ticksToNextRepair--;
                if (this.ticksToNextRepair <= 0)
                {
                    this.pawn.needs.energy.CurLevel -= this.pawn.GetStatValue(StatDefOf.MechEnergyLossPerHP, true, -1);
                    MechRepairUtility.RepairTick(this.pawn);
                    this.ticksToNextRepair = this.TicksPerHeal;
                }
                this.pawn.rotationTracker.FaceTarget(this.pawn);
                if (this.pawn.skills != null)
                {
                    this.pawn.skills.Learn(SkillDefOf.Crafting, 0.05f, false, false);
                }
            };
            toil.AddEndCondition(delegate
            {
                if (!MechRepairUtility.CanRepair(this.pawn))
                {
                    return JobCondition.Succeeded;
                }
                return JobCondition.Ongoing;
            });
            yield return toil;
            yield break;
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.ticksToNextRepair, "ticksToNextRepair", 0, false);
        }
        private const int DefaultTicksPerHeal = 120;
        protected int ticksToNextRepair;
    }
}
