using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.SocialPlatforms;
using Verse;
using Verse.AI;

namespace Fortified
{
    public class CompPropertiesInteracte : CompProperties
    {
        public CompPropertiesInteracte()
        {
            this.compClass = typeof(CompInteracte);
        }

        public float interacteChance = 0.1f;
        public InteractionDef interacte;
    }
    public class CompInteracte : ThingComp
    {
        public CompPropertiesInteracte Props => (CompPropertiesInteracte)this.props;
        public CompDeadManSwitch DMS
        {
            get
            {
                if (this.dms == null)
                {
                    this.dms = this.parent.GetComp<CompDeadManSwitch>();
                }
                return this.dms;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (this.parent.Spawned && this.parent.IsHashIntervalTick(180) && (this.parent is Pawn p && p.health.capacities.CapableOf(PawnCapacityDefOf.Talking)) && (this.lastInteracte == 0 || Find.TickManager.TicksAbs - this.lastInteracte > 2500) && this.DMS.woken && Rand.Chance(this.Props.interacteChance))
            {
                this.TryInteractRandomly();
            }
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (Prefs.DevMode && DebugSettings.godMode)
            {
                yield return new Command_Action()
                {
                    defaultLabel = "Start Interacte",
                    action = () => { this.TryInteractRandomly(); }
                };
            }
            yield break;
        }
        private bool TryInteractRandomly()
        {
            if (this.parent is Pawn pawn && pawn.Map != null && pawn.Faction != null)
            {
                List<Pawn> pawns = new List<Pawn>();
                List<Pawn> collection = pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction);
                pawns.AddRange(collection);
                pawns.Shuffle<Pawn>();
                for (int i = 0; i < pawns.Count; i++)
                {
                    Pawn p = pawns[i];
                    if (p != pawn && InteractionUtility.CanReceiveRandomInteraction(p) && !pawn.HostileTo(p))
                    {
                        if (this.TryInteractWith(p, this.Props.interacte))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public bool TryInteractWith(Pawn recipient, InteractionDef intDef)
        {
            Pawn pawn = this.parent as Pawn;
            if (DebugSettings.alwaysSocialFight)
            {
                intDef = InteractionDefOf.Insult;
            }
            List<RulePackDef> list = new List<RulePackDef>();
            string text;
            string str;
            LetterDef letterDef;
            LookTargets lookTargets;
            intDef.Worker.Interacted(pawn, recipient, list, out text, out str, out letterDef, out lookTargets);
            MoteMaker.MakeInteractionBubble(pawn, recipient, intDef.interactionMote, intDef.GetSymbol(pawn.Faction, pawn.Ideo), intDef.GetSymbolColor(pawn.Faction));
            PlayLogEntry_Interaction playLogEntry_Interaction = new PlayLogEntry_Interaction(intDef, pawn, recipient, list);
            Find.PlayLog.Add(playLogEntry_Interaction);
            if (letterDef != null)
            {
                string text2 = playLogEntry_Interaction.ToGameStringFromPOV(pawn, false);
                if (!text.NullOrEmpty())
                {
                    text2 = text2 + "\n\n" + text;
                }
                Find.LetterStack.ReceiveLetter(str, text2, letterDef, lookTargets ?? pawn, null, null, null, null, 0, true);
            }
            return true;
        }

        public int lastInteracte = 0;
        public CompDeadManSwitch dms;
    }
}