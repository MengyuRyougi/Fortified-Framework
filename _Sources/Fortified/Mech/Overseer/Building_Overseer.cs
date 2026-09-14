using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;
using static HarmonyLib.Code;
using static Mono.Math.BigInteger;

namespace Fortified
{
	public class Building_Overseer : Building, IOverseer, ITargetingSource
	{
		public bool ControllableByState => true;

		#region Targetable
		public bool CasterIsPawn => true;
		public bool IsMeleeAttack => false;
		public bool Targetable => true;
		public bool MultiSelect => false;
		public bool HidePawnTooltips => false;
		public Thing Caster => this;
		public Pawn CasterPawn => null;
		public Verb GetVerb => null;
		public TargetingParameters targetParams => new TargetingParameters()
		{
			canTargetPawns = true,
			canTargetBuildings = true,
			canTargetLocations = false,
			mapObjectTargetsMustBeAutoAttackable = false,
			// 除了機兵本體，也接受封存艙，讓司令核心之類的統御者建築能直接啟動封存機兵
			validator = t => t.Thing is Pawn || t.Thing is Building_MechCapsule
		};
		private Texture2D cachedUIIcon;
		public Texture2D UIIcon
		{
			get
			{
				if (cachedUIIcon == null)
				{
					cachedUIIcon = ContentFinder<Texture2D>.Get("UI/FFF_SelectOverseerSubject");
				}
				return cachedUIIcon;
			}
		}
		public ITargetingSource DestinationSelector => null;
		public bool CanHitTarget(LocalTargetInfo target)
		{
			return ValidateTarget(target, showMessages: false);
		}
		public bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
		{
			if (!target.IsValid || !target.HasThing)
			{
				return false;
			}
			AcceptanceReport acceptanceReport;
			if (target.Thing is Pawn pawn)
			{
				acceptanceReport = MechanitorUtility.CanControlMech(Comp.dummyPawn, pawn);
			}
			else if (target.Thing is Building_MechCapsule capsule)
			{
				acceptanceReport = capsule.CanActivate(this);
			}
			else
			{
				return false;
			}
			if (!acceptanceReport.Accepted)
			{
				if (showMessages && !acceptanceReport.Reason.NullOrEmpty())
				{
					Messages.Message(acceptanceReport.Reason.CapitalizeFirst(), target.Thing, MessageTypeDefOf.RejectInput, historical: false);
				}
				return false;
			}
			return true;
		}

		public void DrawHighlight(LocalTargetInfo target)
		{
			if (target.IsValid)
			{
				GenDraw.DrawTargetHighlight(target);
			}
		}

		public virtual void OrderForceTarget(LocalTargetInfo target)
		{
			if (!target.IsValid || !target.HasThing)
			{
				return;
			}
			if (target.Thing is Pawn pawn)
			{
				if (MechanitorUtility.CanControlMech(Comp.dummyPawn, pawn))
				{
					Comp.Connect(pawn);
				}
			}
			else if (target.Thing is Building_MechCapsule capsule && capsule.CanActivate(this).Accepted)
			{
				// 建築沒辦法走過去駭入，比照 Connect 直接即時啟動；ActivateMech 會銷毀艙體，聲音得先放
				SoundDefOf.ControlMech_Complete.PlayOneShot(new TargetInfo(capsule.Position, capsule.Map));
				capsule.ActivateMech(this);
			}
		}

		public void OnGUI(LocalTargetInfo target)
		{
			if (ValidateTarget(target, showMessages: false))
			{
				GenUI.DrawMouseAttachment(UIIcon);
			}
			else
			{
				GenUI.DrawMouseAttachment(TexCommand.CannotShoot);
			}
		}
		#endregion

		private CompOverseer comp;

		public CompOverseer Comp
		{
			get
			{
				if (comp == null)
				{
					comp = GetComp<CompOverseer>();
				}
				return comp;
			}
		}

		private CompPowerTrader power;

		public CompPowerTrader Power
		{
			get
			{
				if (power == null)
				{
					power = GetComp<CompPowerTrader>();
				}
				return power;
			}
		}

		public override void SetFaction(Faction newFaction, Pawn recruiter = null)
		{
			base.SetFaction(newFaction, recruiter);
			if (newFaction != null && newFaction.IsPlayer)
			{
				Comp?.UpdateDummy();
			}
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			if (Comp.activeInt)
			{
				if (!Power.PowerOn)
				{
					Comp.activeInt = false;
					Comp.Notify_BandwidthChanged();
				}
			}
			else if (Power.PowerOn)
			{
				Comp.activeInt = true;
				Comp.Notify_BandwidthChanged();
			}
		}

		public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		{
			if (mode != DestroyMode.WillReplace)
			{
				Comp.dummyPawn?.mechanitor?.UndraftAllMechs();
			}
			base.DeSpawn(mode);
		}

		protected override void ReceiveCompSignal(string signal)
		{
			base.ReceiveCompSignal(signal);
			if(Comp.activeInt)
			{
				if(signal == "PowerTurnedOff")
				{
					Comp.activeInt = false;
					Comp.Notify_BandwidthChanged();
				}
			}
			else if (signal == "PowerTurnedOn")
			{
				Comp.activeInt = true;
				Comp.Notify_BandwidthChanged();
			}
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			if (Comp.MechanitorActive)
			{
				bool flag = !(Power?.PowerOn ?? true);
				foreach (Gizmo g in Comp.dummyPawn.mechanitor.GetGizmos())
				{
					if (g is Command_CallBossgroup)
					{
						continue;
					}
					if(g is MechanitorBandwidthGizmo)
					{
						yield return new OverseerBuildingBandwidthGizmo(Comp.dummyPawn.mechanitor);
						continue;
					}
					if (flag)
					{
						g.Disable("NoPower".Translate().CapitalizeFirst());
					}
					yield return g;
				}
				if (Spawned)
				{
					string defaultLabel = "FFF_SelectMechToControlLabel".Translate();
					string defaultDesc = "FFF_SelectMechToControlDesc".Translate();
					Command_Action command_Action = new Command_Action
					{
						defaultLabel = defaultLabel,
						defaultDesc = defaultDesc,
						icon = UIIcon,
						groupable = false,
						Order = -86f,
						action = delegate
						{
							SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
							Find.Targeter.BeginTargeting(this);
						}
					};
					if (flag)
					{
						command_Action.Disable("NoPower".Translate().CapitalizeFirst());
					}
					yield return command_Action;
				}
			}
			foreach (Gizmo g in base.GetGizmos())
			{
				yield return g;
			}
		}
	}
}
