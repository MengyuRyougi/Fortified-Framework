using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Fortified;

public class PlaceWorker_ByWallOnSouth : PlaceWorker
{
    public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
    {
        LinkDirections dir = LinkDirections.None;
        switch (rot.AsInt)
        {
            case 0:
                dir = LinkDirections.Down;
                break;
            case 1:
                dir = LinkDirections.Left;
                break;
            case 2:
                dir = LinkDirections.Up;
                break;
            case 3:
                dir = LinkDirections.Right;
                break;
        }
        IEnumerable<IntVec3> enumerable = CellsAdjacentEdge(GetBuildingTrueCenter(loc, checkingDef.Size, rot), rot, ((ThingDef)checkingDef).size, dir);
        GenDraw.DrawFieldEdges(enumerable.ToList(), Color.blue);
        foreach (IntVec3 item in enumerable)
        {
            Building edifice = item.GetEdifice(map);
            if (edifice == null || !edifice.def.building.isPlaceOverableWall)
            {
                Building edifice2 = item.GetEdifice(map);
                if (edifice2 == null || !edifice2.def.IsDoor)
                {
                    return "FFF.Message.CannotPlaceWithoutWall".Translate();
                }
            }
        }
        return true;
    }

    public static IntVec3 GetBuildingTrueCenter(IntVec3 loc, IntVec2 thingSize, Rot4 rot)
    {
        IntVec3 result = new IntVec3(loc.x - (thingSize.x - 1) / 2, loc.y, loc.z - (thingSize.z - 1) / 2);
        switch (rot.AsInt)
        {
            case 1:
                if (thingSize.z % 2 == 0)
                {
                    result.z--;
                    result.x++;
                }
                break;
            case 3:
                if (thingSize.z % 2 == 0)
                {
                    result.z--;
                    result.x++;
                }
                break;
        }
        return result;
    }
    public static IEnumerable<IntVec3> CellsAdjacentEdge(IntVec3 thingCent, Rot4 thingRot, IntVec2 thingSize, LinkDirections dir)
    {
        GenAdj.AdjustForRotation(ref thingCent, ref thingSize, thingRot);
        int minX = thingCent.x;
        int maxX = minX + thingSize.x - 1;
        int minZ = thingCent.z;
        int maxZ = minZ + thingSize.z - 1;
        switch (dir)
        {
            case LinkDirections.Up:
                {
                    for (int x4 = minX; x4 <= maxX; x4++)
                    {
                        if (x4 != (maxX - minX + 1) / 2)
                        {
                            yield return new IntVec3(x4, thingCent.y, maxZ + 1);
                        }
                    }
                    break;
                }
            case LinkDirections.Right:
                {
                    for (int x4 = minZ; x4 <= maxZ; x4++)
                    {
                        if (x4 != (maxZ - minZ + 1) / 2)
                        {
                            yield return new IntVec3(maxX + 1, thingCent.y, x4);
                        }
                    }
                    break;
                }
            case LinkDirections.Down:
                {
                    for (int x4 = minX; x4 <= maxX; x4++)
                    {
                        if (x4 != (maxX - minX + 1) / 2)
                        {
                            yield return new IntVec3(x4, thingCent.y, minZ - 1);
                        }
                    }
                    break;
                }
            case LinkDirections.Left:
                {
                    for (int x4 = minZ; x4 <= maxZ; x4++)
                    {
                        if (x4 != (maxZ - minZ + 1) / 2)
                        {
                            yield return new IntVec3(minX - 1, thingCent.y, x4);
                        }
                    }
                    break;
                }
        }
    }
}