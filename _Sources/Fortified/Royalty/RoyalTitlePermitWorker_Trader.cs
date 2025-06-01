using Mono.Unix.Native;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using static HarmonyLib.Code;

namespace Fortified
{
    public class RoyalTitlePermitWorker_Trader : RoyalTitlePermitWorker
    {
        public override IEnumerable<FloatMenuOption> GetRoyalAidOptions(Map map, Pawn pawn, Faction faction)
        {
            if (faction.HostileTo(Faction.OfPlayer))
            {
                yield return new FloatMenuOption("CommandCallRoyalAidFactionHostile".Translate(faction.Named("FACTION")), null);
                yield break;
            }
            Action action = null;
            string description = def.LabelCap + ": ";
            if (FillAidOption(pawn, faction, ref description, out var free))
            {
                action = delegate
                {
                    DoEffect(pawn, faction, free);
                };
            }
            yield return new FloatMenuOption(description, action, faction.def.FactionIcon, faction.Color);
        }
        private void DoEffect(Pawn caller, Faction faction, bool free)
        {
            var ext = def.GetModExtension<PermitTraderExtension>();
            if (ext != null && ext.traderKindDef != null)
            {
                IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.Misc, caller.MapHeld);
                incidentParms.target = caller.MapHeld;
                incidentParms.points = caller.MapHeld.PlayerWealthForStoryteller;
                if (IncidentDefOf.TraderCaravanArrival.Worker.CanFireNow(incidentParms))
                {
                    incidentParms.faction = faction;
                    incidentParms.traderKind = ext.traderKindDef;
                    IncidentDefOf.TraderCaravanArrival.Worker.TryExecute(incidentParms);
                }
            }
            if (!free)
            {
                caller.royalty.TryRemoveFavor(faction, def.royalAid.favorCost);
            }
        }
    }

    public class PermitTraderExtension : DefModExtension
    {
        public TraderKindDef traderKindDef = null;
    }
}
