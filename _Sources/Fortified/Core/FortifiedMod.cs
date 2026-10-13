using UnityEngine;
using Verse;

namespace Fortified
{
    public class FortifiedMod : Mod
    {
        public static FortifiedSettings Settings { get; private set; }

        public FortifiedMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<FortifiedSettings>();
        }

        public override string SettingsCategory() => Content.Name;

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);
            listing.CheckboxLabeled("FFF.Settings.ShowDevCategory".Translate(), ref Settings.showDevCategory,
                "FFF.Settings.ShowDevCategory.Desc".Translate());
            listing.End();
        }
    }
}
