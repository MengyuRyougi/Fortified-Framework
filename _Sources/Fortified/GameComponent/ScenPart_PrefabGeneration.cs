using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Fortified
{
    public class ScenPart_PrefabGeneration : ScenPart
    {
        public ScenPartDef scenPartDef;
        public PrefabDef prefab = null;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref prefab, "prefab");

        }

        public override string Summary(Scenario scen)
        {
            return ScenSummaryList.SummaryWithList(scen, "MapContain", prefab.defName);
        }

        public override void DoEditInterface(Listing_ScenEdit listing)
        {
            Rect scenPartRect = listing.GetScenPartRect(this, ScenPart.RowHeight);
            string label = ((prefab == null) ? ((string)"FFF.ScenPart_SelectPrefabDef".Translate()) : ((!string.IsNullOrEmpty(prefab.label)) ? ((string)prefab.LabelCap) : prefab.defName));
            if (!Widgets.ButtonText(scenPartRect, label))
            {
                return;
            }
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            foreach (PrefabDef item in DefDatabase<PrefabDef>.AllDefs)
            {
                PrefabDef localFd2 = item;
                list.Add(new FloatMenuOption(localFd2.defName, delegate
                {
                    prefab = localFd2;
                }));
            }

            Find.WindowStack.Add(new FloatMenu(list));
        }
        public override IEnumerable<string> ConfigErrors()
        {
            return base.ConfigErrors();
        }

        public override void PostMapGenerate(Map map)
        {
            if ((Find.GameInfo.startingTile != map.Tile)) return;

            LargeBuildingSpawnParms parms = new LargeBuildingSpawnParms();
            parms.minDistToEdge = 10;
            parms.canSpawnOnImpassable = true;
            parms.allowFogged = false;
            parms.overrideSize = prefab.size;
            if (!LargeBuildingCellFinder.TryFindCell(out var cell, map, parms))
            {
                Log.Error("Failed to generate prefab.");
            }
            else
            {
                PrefabUtility.SpawnPrefab(prefab, map, cell, Rot4.North, Faction.OfPlayer);
            }
        }
    }
}
