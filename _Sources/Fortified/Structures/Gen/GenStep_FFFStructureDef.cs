// 当白昼倾坠之时
using System.Linq;
using System.Collections.Generic;
using RimWorld;
using Verse;
using RimWorld.Planet;

namespace Fortified.Structures
{
    public class GenStep_FFFStructureDef : GenStep
    {
        public GenStep_FFFStructureDef() { }

        public override int SeedPart => 394857327;

        public List<FFF_StructureDef> structureDefs;

        public FactionDef forcedFaction;

        public RotEnum allowedRotation = RotEnum.North;

        public override void Generate(Map map, GenStepParams parms)
        {
            if(structureDefs.NullOrEmpty()) return;
            Faction faction = null;
			if (forcedFaction != null)
            {
				faction = Find.FactionManager.FirstFactionOfDef(forcedFaction);
            }
            if(faction == null)
            {
                faction = map.ParentFaction ?? parms.sitePart?.site?.Faction;
			}
			FFF_StructureDef def = structureDefs.RandomElement();
            Rot4 rot = allowedRotation.Random();
            // 隨機落點，但避開已登記的 UsedRects（地標建築等）；試不到就退回任一位置。
            // Random spot, but clear of registered UsedRects (landmark structures etc.); falls back to any spot.
            CellRect area = CellRect.WholeMap(map).ContractedBy(5);
            IntVec2 size = def.GetSize(rot);
            CellRect rect = CellRect.Empty;
            bool found = false;
            for (int i = 0; i < 30 && !found; i++)
            {
                if (area.TryFindRandomInnerRect(size, out rect) && !FFF_StructureUtility.OverlapsUsedRect(map, rect))
                    found = true;
            }
            if (!found && !area.TryFindRandomInnerRect(size, out rect)) return;

            FFF_StructureUtility.Generate(def, rect.CenterCell, map, faction, rot, reconnectPower: false);
            FFF_StructureUtility.ReserveUsedRect(map, FFF_StructureUtility.FootprintAt(def, rect.CenterCell, rot), 2);
        }
    }
}
