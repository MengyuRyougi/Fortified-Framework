using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Fortified
{
    public class ScenPart_GeneratePrefab : ScenPart
    {
        public override void DoEditInterface(Listing_ScenEdit listing)
        {
            Rect scenPartRect = listing.GetScenPartRect(this, this.maps.Count * 30f + ScenPart.RowHeight);
            ScenPart_GeneratePrefab.DrawButtonWithIcon(scenPartRect.y, () => ScenPart_GeneratePrefab.DrawFloatMenu(DefDatabase<PrefabDef>.AllDefsListForReading, d => this.maps.Add(d), d => d.defName), () =>
            ScenPart_GeneratePrefab.DrawFloatMenu(this.maps, d => this.maps.Remove(d), d => d.defName), scenPartRect.width + 20f, 20);
            scenPartRect.y += 30f;
            this.maps.ForEach(m =>
            {
                Widgets.Label(scenPartRect, m.defName);
                scenPartRect.y += 30f;
            });
        }
        public static void DrawFloatMenu<T>(List<T> list, Action<T> action, Func<T, string> text, List<FloatMenuOption> extra = null, Func<T, bool> validator = null)
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            if (extra != null)
            {
                options.AddRange(extra);
            }
            foreach (T t in list)
            {
                if (validator == null || validator(t))
                {
                    FloatMenuOption option = new FloatMenuOption(text(t), () =>
                    {
                        action(t);
                    });
                    options.Add(option);
                }
            }
            if (options.Any())
            {
                Find.WindowStack.Add(new FloatMenu(options));
            }
        }
        public static void DrawButtonWithIcon(float y, Action addAction, Action removeAction, float x = 10f, float iconSize = 25f, float interval = 35f, Vector2? size = null)
        {
            if (Widgets.ButtonImage(new Rect(x, y, iconSize, iconSize), TexButton.Plus))
            {
                addAction();
            }
            if (Widgets.ButtonImage(new Rect(x + interval, y, iconSize, iconSize), TexButton.Delete))
            {
                removeAction();
            }
        }
        public override void PostMapGenerate(Map map)
        {
            base.PostMapGenerate(map);
            if (Find.TickManager.TicksGame < 5f && map != null)
            {
                Log.Message("B");
                var prefab = this.maps.RandomElement();


                LargeBuildingSpawnParms parms = new LargeBuildingSpawnParms();
                parms.minDistToEdge = 10;
                parms.canSpawnOnImpassable = true;
                parms.allowFogged = false;
                parms.overrideSize = prefab.size;

                CellRect cellRect = GenAdj.OccupiedRect(map.Center, Rot4.North, prefab.size);
                foreach (var c in cellRect)
                {
                    //map.fogGrid.Unfog(cell);
                    foreach (var item in c.GetThingList(map).ListFullCopy())
                    {
                        if (item is not Skyfaller && item.def.destroyable && (item.Faction != Faction.OfPlayer && item as Pawn == null))
                        {
                            item.Destroy();
                        }
                    }
                }
                PrefabUtility.SpawnPrefab(prefab, map, map.Center, Rot4.North);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref this.maps, "maps", LookMode.Def);
        }


        public List<PrefabDef> maps = new List<PrefabDef>();
    }
}