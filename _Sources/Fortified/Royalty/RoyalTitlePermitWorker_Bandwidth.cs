using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Noise;

namespace Fortified
{

    [StaticConstructorOnStartup]
    public class RoyalTitlePermitWorker_Bandwidth : RoyalTitlePermitWorker
    {
        public override IEnumerable<FloatMenuOption> GetRoyalAidOptions(Map map, Pawn pawn, Faction faction)
        {
            if (faction.HostileTo(Faction.OfPlayer))
            {
                yield return new FloatMenuOption("CommandCallRoyalAidFactionHostile".Translate(faction.Named("FACTION")), null);
                yield break;
            }
            if (!MechanitorUtility.IsMechanitor(pawn))
            {
                yield return new FloatMenuOption("DMS_PawnIsNotMechanitor".Translate(faction.Named("FACTION")), null);
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
            var ext = def.GetModExtension<BandwidthSupportExtension>();
            if (ext != null && caller.GetCurrentTitleIn(faction) != null)
            {
                Hediff h = caller.health.GetOrAddHediff(ext.hediff);
                h.Severity = ext.GetLevel(caller.GetCurrentTitleIn(faction));

                Messages.Message("FFF_BandwidthSupported".Translate(faction.Named("FACTION")), new LookTargets(caller.PositionHeld, caller.MapHeld), MessageTypeDefOf.NeutralEvent);
                caller.royalty.GetPermit(def, faction).Notify_Used();
                if (!free)
                {
                    caller.royalty.TryRemoveFavor(faction, def.royalAid.favorCost);
                }
            }
        }
    }
    public class BandwidthSupportExtension : DefModExtension
    {
        public HediffDef hediff = null;
        public List<TitleValuePair> titles = new List<TitleValuePair>();
        public int GetLevel(RoyalTitleDef def)
        {
            var p = titles.Where(t => t.title == def).FirstOrDefault();
            if (p != null)
            {
                return p.level;
            }
            return 0;
        }
    }
    public class TitleValuePair
    {
        public RoyalTitleDef title;
        public int level;
    }
}
