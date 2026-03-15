using RimWorld;
using RimWorld.Planet;
using Verse;
using System.Collections.Generic;

namespace Fortified
{
    public class WorldObjectCompProperties_PeriodicRaid : WorldObjectCompProperties
    {
        public IntRange daysBetweenRaids = new IntRange(25, 35);
        public float pointsFactor = 1.0f;
        public string letterLabel;
        public string letterText;
        public FactionDef forcedFaction;

        public List<SitePartDef> requiresAnySitePart;

        public WorldObjectCompProperties_PeriodicRaid()
        {
            this.compClass = typeof(WorldObjectComp_PeriodicRaid);
        }
    }
}
