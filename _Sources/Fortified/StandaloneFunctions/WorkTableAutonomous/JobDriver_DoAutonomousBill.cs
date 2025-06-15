using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace Fortified
{
    public class    JobDriver_DoAutonomousBill : JobDriver_DoBill
    {
        protected override IEnumerable<Toil> MakeNewToils()
        {
            base.AddEndCondition(delegate
            {
                Thing thing = base.GetActor().jobs.curJob.GetTarget(TargetIndex.A).Thing;
                if (thing is Building && !thing.Spawned)
                {
                    return JobCondition.Incompletable;
                }
                return JobCondition.Ongoing;
            });
            this.FailOnBurningImmobile(TargetIndex.A);
            this.FailOn(delegate ()
            {
                IBillGiver billGiver = this.job.GetTarget(TargetIndex.A).Thing as IBillGiver;
                if (billGiver != null)
                {
                    if (this.job.bill.DeletedOrDereferenced)
                    {
                        return true;
                    }
                    if (!billGiver.CurrentlyUsableForBills())
                    {
                        return true;
                    }
                }
                return false;
            });
            Toil gotoBillGiver = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell, false);
            Toil toil = ToilMaker.MakeToil("MakeNewToils");
            toil.initAction = delegate ()
            {
                if (this.job.targetQueueB != null && this.job.targetQueueB.Count == 1)
                {
                    UnfinishedThing unfinishedThing = this.job.targetQueueB[0].Thing as UnfinishedThing;
                    if (unfinishedThing != null)
                    {
                        unfinishedThing.BoundBill = (Bill_ProductionWithUft)this.job.bill;
                    }
                }
                this.job.bill.Notify_DoBillStarted(this.pawn);
            };
            yield return toil;
            yield return Toils_Jump.JumpIf(gotoBillGiver, () => this.job.GetTargetQueue(TargetIndex.B).NullOrEmpty<LocalTargetInfo>());
            foreach (Toil toil2 in JobDriver_DoBill.CollectIngredientsToils(TargetIndex.B, TargetIndex.A, TargetIndex.C, false, true, this.BillGiver is RimWorld.Building_WorkTableAutonomous))
            {
                yield return toil2;
            }
            yield return gotoBillGiver;
            yield return Toils_Recipe.MakeUnfinishedThingIfNeeded();
            Toil toilRecipe = Toils_Recipe.DoRecipeWork().FailOnDespawnedNullOrForbiddenPlacedThings(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
            toilRecipe.tickIntervalAction = (delta) =>
            {
                Pawn actor = toil.actor;
                Job curJob = actor.jobs.curJob;
                JobDriver_DoBill jobDriver_DoBill = (JobDriver_DoBill)actor.jobs.curDriver;
                UnfinishedThing unfinishedThing = curJob.GetTarget(TargetIndex.B).Thing as UnfinishedThing;
                if (unfinishedThing != null && unfinishedThing.Destroyed)
                {
                    actor.jobs.EndCurrentJob(JobCondition.Incompletable, true, true);
                    return;
                }
                jobDriver_DoBill.ticksSpentDoingRecipeWork += delta;
                curJob.bill.Notify_PawnDidWork(actor);
                if (curJob.RecipeDef.workSkill != null && curJob.RecipeDef.UsesUnfinishedThing && actor.skills != null)
                {
                    actor.skills.Learn(curJob.RecipeDef.workSkill, 0.1f * curJob.RecipeDef.workSkillLearnFactor * (float)delta, false, false);
                }
                float num = (curJob.RecipeDef.workSpeedStat == null) ? 1f : actor.GetStatValue(curJob.RecipeDef.workSpeedStat, true, -1);
                if (curJob.RecipeDef.workTableSpeedStat != null)
                {
                    Building_WorkTable building_WorkTable = jobDriver_DoBill.BillGiver as Building_WorkTable;
                    if (building_WorkTable != null)
                    {
                        num *= building_WorkTable.GetStatValue(curJob.RecipeDef.workTableSpeedStat, true, -1);
                    }
                }
                if (DebugSettings.fastCrafting)
                {
                    num *= 30f;
                }
                jobDriver_DoBill.workLeft -= num * (float)delta;
                if (unfinishedThing != null)
                {
                    if (unfinishedThing.debugCompleted)
                    {
                        unfinishedThing.workLeft = (jobDriver_DoBill.workLeft = 0f);
                    }
                    else
                    {
                        unfinishedThing.workLeft = jobDriver_DoBill.workLeft;
                    }
                }
                actor.GainComfortFromCellIfPossible(delta, true);
                if (jobDriver_DoBill.workLeft <= 0f)
                {
                    var skillTracker = this.pawn.skills;
                    FormingState state = (FormingState)curJob.bill.GetType().GetField("state", BindingFlags.NonPublic
                        | BindingFlags.Instance).GetValue(curJob.bill);
                    bool prepar = state == FormingState.Preparing || state == FormingState.Gathering;
                    curJob.bill.Notify_BillWorkFinished(actor);
                    if (prepar && skillTracker != null && this.TargetThingA is Building_WorkTableAutonomous building
                    && building.def.GetModExtension<ModExtension_AutoWorkTable>() is var extension)
                    {
                        Bill_Autonomous bill = curJob.bill as Bill_Autonomous;

                        foreach (var item in extension.skills)
                        {
                            if (skillTracker.GetSkill(item.Key) is SkillRecord record && !record.TotallyDisabled)
                            {
                                bill.formingTicks -= record.Level * item.Value;
                            }
                        }
                    }
                    jobDriver_DoBill.ReadyForNextToil();
                    return;
                }
                if (curJob.bill.recipe.UsesUnfinishedThing && Find.TickManager.TicksGame - jobDriver_DoBill.billStartTick >= 3000 && actor.IsHashIntervalTick(1000, delta))
                {
                    actor.jobs.CheckForJobOverride(0f, true);
                }
            };
            yield return toilRecipe;
            yield return Toils_Recipe.CheckIfRecipeCanFinishNow();
            yield return Toils_Recipe.FinishRecipeAndStartStoringProduct(TargetIndex.None);
            yield break;
        }
    }
}