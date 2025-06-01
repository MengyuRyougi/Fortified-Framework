using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Fortified
{
    public class CompAirSupportSummoner : ThingComp
    {
        CompProperties_AirSupportSummoner Props => props as CompProperties_AirSupportSummoner;
        public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
        {
            yield return new Command_Action
            {
                action = new Action(DoEffect),
                defaultDesc = parent.def.description,
                icon = parent.def.uiIcon,
                defaultLabel = Props.label,
            };
        }

        public void DoEffect()
        {
            Targeter targeter = Find.Targeter;
            var pram = new TargetingParameters
            {
                canTargetBuildings = false,
                canTargetPawns = false,
                canTargetLocations = true
            };
            Find.Targeter.BeginTargeting(pram, DoEffect);
        }

        public void DoEffect(LocalTargetInfo cell)
        {
            Thing triggerer = parent.ParentHolder is Pawn_ApparelTracker pawn ? pawn.pawn : parent;

            Props.supportDef.Trigger(triggerer, triggerer.MapHeld, cell);
        }
    }

    public class CompProperties_AirSupportSummoner : CompProperties
    {
        public CompProperties_AirSupportSummoner()
        {
            compClass = typeof(CompAirSupportSummoner);
        }

        public AirSupportDef supportDef;

        public string label = "boom";
    }
}
