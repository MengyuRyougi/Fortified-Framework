using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.Sound;
using static RimWorld.MechClusterSketch;

namespace Fortified
{
	public class FloatMenuOptionProvider_OverseerMech : FloatMenuOptionProvider
	{
		protected override bool Drafted => true;

		protected override bool Undrafted => true;

		protected override bool Multiselect => false;

		protected override bool RequiresManipulation => true;

		protected override bool MechanoidCanDo => true;

		protected override bool CanSelfTarget => true;

		protected override bool AppliesInt(FloatMenuContext context)
		{
			return context.FirstSelectedPawn is IOverseer;
		}

		public override IEnumerable<FloatMenuOption> GetOptions(FloatMenuContext context)
		{
			return base.GetOptions(context);
		}

		// 封存艙：讓軍士等具備帶寬的統御者機兵也能像機械師一樣駭入啟動。
		// 一般 Thing.GetFloatMenuOptions 走的 FloatMenuOptionProvider_FromThing 不允許機械體使用，所以在這裡補上。
		public override IEnumerable<FloatMenuOption> GetOptionsFor(Thing clickedThing, FloatMenuContext context)
		{
			if (clickedThing is Building_MechCapsule capsule && capsule.HasMech && context.FirstSelectedPawn is IOverseer && context.FirstSelectedPawn is Pawn mech)
			{
				if (!mech.CanReach(capsule, PathEndMode.InteractionCell, Danger.Deadly))
				{
					yield return new FloatMenuOption("FFF.CannotReach".Translate(), null);
				}
				else
				{
					yield return RimWorld.FloatMenuUtility.DecoratePrioritizedTask(capsule.GetActivateOption(mech), mech, new LocalTargetInfo(capsule));
				}
			}
		}

		public override IEnumerable<FloatMenuOption> GetOptionsFor(Pawn clickedPawn, FloatMenuContext context)
		{
			if (context.FirstSelectedPawn is IOverseer overseer && overseer is Pawn mech)
			{
				if (clickedPawn.Faction != Faction.OfPlayerSilentFail)
				{
					yield break;
				}
				if(clickedPawn != mech)
				{
					if (clickedPawn.GetOverseer() != overseer.Comp.dummyPawn)
					{
						if (!overseer.Comp.Props.instantControl && !mech.CanReach(clickedPawn, PathEndMode.Touch, Danger.Deadly))
						{
							yield return new FloatMenuOption("CannotControlMech".Translate(clickedPawn.LabelShort) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
						}
						else if (!MechanitorUtility.CanControlMech(overseer.Comp.dummyPawn, clickedPawn))
						{
							AcceptanceReport acceptanceReport = MechanitorUtility.CanControlMech(overseer.Comp.dummyPawn, clickedPawn);
							if (!acceptanceReport.Reason.NullOrEmpty())
							{
								yield return new FloatMenuOption("CannotControlMech".Translate(clickedPawn.LabelShort) + ": " + acceptanceReport.Reason, null);
							}
						}
						else
						{
							yield return RimWorld.FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("ControlMech".Translate(clickedPawn.LabelShort), delegate
							{
								if (overseer.Comp.Props.instantControl)
								{
									overseer.Comp.Connect(clickedPawn);
									SoundDefOf.ControlMech_Complete.PlayOneShot(clickedPawn);
								}
								else
								{
									Job job = JobMaker.MakeJob(FFF_DefOf.FFF_ControlMech_Overseer, clickedPawn);
									mech.jobs.TryTakeOrderedJob(job, JobTag.Misc);
								}
							}), mech, new LocalTargetInfo(clickedPawn));
						}
						yield return new FloatMenuOption("CannotDisassembleMech".Translate(clickedPawn.LabelCap) + ": " + "MustBeOverseer".Translate().CapitalizeFirst(), null);
					}
					else
					{
						yield return RimWorld.FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("DisconnectMech".Translate(clickedPawn.LabelShort), delegate
						{
							MechanitorUtility.ForceDisconnectMechFromOverseer(clickedPawn);
						}, MenuOptionPriority.Low, null, null, 0f, null, null, playSelectionSound: true, -10), mech, new LocalTargetInfo(clickedPawn));
						if (!clickedPawn.IsFighting())
						{
							yield return RimWorld.FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("DisassembleMech".Translate(clickedPawn.LabelCap), delegate
							{
								Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmDisassemblingMech".Translate(clickedPawn.LabelCap) + ":\n" + (from x in MechanitorUtility.IngredientsFromDisassembly(clickedPawn.def)
																																								select x.Summary).ToLineList("  - "), delegate
																																								{
																																									mech.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.DisassembleMech, clickedPawn), JobTag.Misc);
																																								}, destructive: true));
							}, MenuOptionPriority.Low, null, null, 0f, null, null, playSelectionSound: true, -20), mech, new LocalTargetInfo(clickedPawn));
						}
					}
				}
				if (!MechRepairUtility.CanRepair(clickedPawn) || !overseer.Comp.Props.canRepair
					|| (clickedPawn is ArtificialOrganism amo && !amo.Repairable))
				{
					yield break;
				}
				if (clickedPawn == mech)
				{
					yield return RimWorld.FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("RepairThing".Translate(mech.LabelShort), delegate
					{
						Job job = JobMaker.MakeJob(FFF_DefOf.FFF_RepairMech_Overseer, mech);
						mech.jobs.TryTakeOrderedJob(job, JobTag.Misc);
					}), mech, new LocalTargetInfo(clickedPawn));
					yield break;
				}
				if (!mech.CanReach(clickedPawn, PathEndMode.Touch, Danger.Deadly))
				{
					yield return new FloatMenuOption("CannotRepairMech".Translate(clickedPawn.LabelShort) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
					yield break;
				}
				yield return RimWorld.FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("RepairThing".Translate(clickedPawn.LabelShort), delegate
				{
					Job job = JobMaker.MakeJob(FFF_DefOf.FFF_RepairMech_Overseer, clickedPawn);
					mech.jobs.TryTakeOrderedJob(job, JobTag.Misc);
				}), mech, new LocalTargetInfo(clickedPawn));
			}
		}
	}
}