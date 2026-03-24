using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Fortified
{
	public class CompProperties_CausesGameConditionAdjustable : CompProperties_CausesGameCondition
	{
		public bool shouldBeSpawned = true;

		public List<PlanetLayerDef> planetLayerWhitelist;

		public CompProperties_CausesGameConditionAdjustable()
		{
			compClass = typeof(CompCauseGameConditionAdjustable);
		}
	}
	
	public class CompCauseGameConditionAdjustable : CompCauseGameCondition
	{
		public new CompProperties_CausesGameConditionAdjustable Props => (CompProperties_CausesGameConditionAdjustable)props;

		protected CompFlickable flickableComp;

		protected CompPowerTrader powerComp;

		public override bool Active
		{
			get
			{
				if (Props.shouldBeSpawned && !parent.Spawned)
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
				if(!Props.planetLayerWhitelist.NullOrEmpty() && !Props.planetLayerWhitelist.Contains(parent.Map.Tile.LayerDef))
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
