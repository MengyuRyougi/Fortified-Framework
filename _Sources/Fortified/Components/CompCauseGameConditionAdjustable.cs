using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Fortified
{
	public class CompCauseGameConditionAdjustable : CompCauseGameCondition
	{
		protected CompFlickable flickableComp;

		protected CompPowerTrader powerComp;

		public override bool Active
		{
			get
			{
				if (!parent.Spawned)
				{
					return false;
				}
				if (flickableComp != null && !flickableComp.SwitchIsOn)
				{
					return false;
				}
				if (powerComp != null && !powerComp.PowerOn)
				{
					return false;
				}
				return base.Active;
			}
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			flickableComp = parent.GetComp<CompFlickable>();
			powerComp = parent.GetComp<CompPowerTrader>();
			base.PostSpawnSetup(respawningAfterLoad);
		}
	}
}