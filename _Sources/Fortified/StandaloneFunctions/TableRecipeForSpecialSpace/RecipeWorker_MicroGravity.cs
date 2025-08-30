using RimWorld;
using Verse;

namespace Fortified
{
    public class RecipeWorker_MicroGravity : RecipeWorker
    {
        public override AcceptanceReport AvailableReport(Thing thing, BodyPartRecord part = null)
        {
            if (!IsInMicroGravity(thing))
            {
                return "FFF.Cannot.TableNotInMicroGravity".Translate();
            }
            return base.AvailableReport(thing, part);
        }
        private bool IsInMicroGravity(Thing thing)
        {
            if (!ModsConfig.OdysseyActive) Log.WarningOnce($"Warning, {thing} is using RecipeWorker_MicroGravity without OdysseyActive.", 123457);
            if (thing.GetType().IsAssignableFrom(typeof(Building_WorkTable)))
            {
                Building_WorkTable table = thing as Building_WorkTable;
                if (table.Spawned && table.Map.TileInfo.Layer.Def != PlanetLayerDefOf.Surface)
                {
                    return true;
                }
            }
            return false;
        }
    }
}