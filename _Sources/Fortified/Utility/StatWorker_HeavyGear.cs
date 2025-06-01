using Verse;
using System.Collections.Generic;
using RimWorld;

namespace Fortified
{
    public class StatWorker_HeavyGear : StatWorker
    {
        public override bool ShouldShowFor(StatRequest req)
        {
            return base.ShouldShowFor(req) && req.Def.HasModExtension<HeavyEquippableExtension>();
        }
        public override IEnumerable<Dialog_InfoCard.Hyperlink> GetInfoCardHyperlinks(StatRequest statRequest)
        {
            HeavyEquippableExtension ext = statRequest.Def.GetModExtension<HeavyEquippableExtension>();
            if (ext != null)
            {
                foreach (HediffDef hediff in ext.EquippableDef.EquippableWithHediff)
                {
                    yield return new Dialog_InfoCard.Hyperlink(hediff);
                }
                foreach (ThingDef apparel in ext.EquippableDef.EquippableWithApparel)
                {
                    yield return new Dialog_InfoCard.Hyperlink(apparel);
                }
                if (ModsConfig.BiotechActive)
                {
                    foreach (GeneDef gene in ext.EquippableDef.EquippableWithGene)
                    {
                        yield return new Dialog_InfoCard.Hyperlink(gene);
                    }
                }
                foreach (ThingDef race in ext.EquippableDef.EquippableByRace)
                {
                    yield return new Dialog_InfoCard.Hyperlink(race);
                }
                foreach (ThingDef race in WeaponTagUtil.UseableByListsOfMechs(ThingMaker.MakeThing(statRequest.Def as ThingDef) as ThingWithComps))
                {
                    yield return new Dialog_InfoCard.Hyperlink(race);
                }
            }
        }
        public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
        {
            return "";
        }
        public override string GetExplanationFinalizePart(StatRequest req, ToStringNumberSense numberSense, float finalVal)
        {
            if (req.Def.GetModExtension<HeavyEquippableExtension>().EquippableDef.EquippableBaseBodySize == -1)
            {
                return "DMS_MountedWeaponCanOnlyBeEquippedBySpecificApparelOrRaces".Translate();
            }
            return "DMS_WeaponCanBeEquippedBySpecificApparelOrRaces".Translate();
        }
        public override string GetStatDrawEntryLabel(StatDef stat, float value, ToStringNumberSense numberSense, StatRequest optionalReq, bool finalized = true)
        {
            if (optionalReq.Def.GetModExtension<HeavyEquippableExtension>().EquippableDef.EquippableBaseBodySize == -1)
            {
                return "DMS_MountedWeapon".Translate();
            }
            return optionalReq.Def.GetModExtension<HeavyEquippableExtension>().EquippableDef.EquippableBaseBodySize.ToString("0.##");
        }
    }
}
