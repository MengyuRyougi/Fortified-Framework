using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using static Unity.Burst.Intrinsics.X86.Avx;

namespace Fortified
{
    //機械體控制延伸，但這個不限於同個機械師的機體。
    public class CompSubRelay : ThingComp
    {
        public CompProperties_SubRelay Props => (CompProperties_SubRelay)this.props;
        public float CurrentRadius => Props.relayRange;
        public bool AnySelectedDraftedMechs
        {
            get
            {
                List<Pawn> selectedPawns = Find.Selector.SelectedPawns;
                for (int i = 0; i < selectedPawns.Count; i++)
                {
                    if (selectedPawns[i].OverseerSubject != null && selectedPawns[i].Drafted)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        public float SquaredDistance
        {
            get
            {
                return cacheDistance != 0 ? cacheDistance : GetCacheDistance();
            }
        }
        private float cacheDistance = 0;
        private float GetCacheDistance()
        {
            cacheDistance = Mathf.Pow(CurrentRadius, 2);
            return cacheDistance;
        }
        public Pawn Pawn
        {
            get
            {
                if (this.parent is Pawn p) return p; //本體
                //if (this.parent is ThingWithComps t && t.TryGetComp<CompEquippable>(out var e)) return e.ParentHolder as Pawn ?? null; //腰帶
                if (this.parent is Apparel a) return a.Wearer ?? null;//衣服
                return null;
            }
        }
        public bool IsActive => isActive;
        private bool isActive = false;
        public override void CompTick()
        {
            if (!parent.Spawned) return;

            if (parent is Building b)
            {
                isActive = true;
                if (parent.Faction != Faction.OfPlayer) isActive = false;
                if (b.TryGetComp<CompPowerTrader>(out var p) && !p.PowerOn) isActive = false;
                if(b.IsBrokenDown()) isActive = false;
                if(!b.IsWorking()) isActive = false;
            }
        }
        public override void PostDraw()
        {
            base.PostDraw();
            if (parent.Map != Find.CurrentMap) return;
            if (parent is Building && !isActive) return;

            if (AnySelectedDraftedMechs)
            {
                DrawCommandRadius();
            }
        }
        public override void CompDrawWornExtras()
        {
            base.CompDrawWornExtras();
            if (AnySelectedDraftedMechs)
            {
                GenDraw.DrawRadiusRing(Pawn.Position, CurrentRadius, Color.white);
            }
        }
        public void DrawCommandRadius()
        {
            if (parent.Spawned && AnySelectedDraftedMechs)
            {
                GenDraw.DrawRadiusRing(parent.Position, CurrentRadius, Color.white);
            }
        }
    }

    public class CompProperties_SubRelay : CompProperties
    {
        public float relayRange = 10f;
        public CompProperties_SubRelay() => this.compClass = typeof(CompSubRelay);
    }
}