using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Fortified
{
    public class RecipeWorker_Vacuum : RecipeWorker
    {
        public override AcceptanceReport AvailableReport(Thing thing, BodyPartRecord part = null)
        {
            if (!IsInVacuum(thing))
            {
                return "FFF.Cannot.TableNotInVacuum".Translate();
            }
            return base.AvailableReport(thing, part);
        }
        private bool IsInVacuum(Thing thing)
        {
            if (!ModsConfig.OdysseyActive) Log.WarningOnce($"Warning, {thing} is using RecipeWorker_Vacuum without OdysseyActive.", 123456);
            if (thing.GetType().IsAssignableFrom(typeof(Building_WorkTable)))
            {
                Building_WorkTable table = thing as Building_WorkTable;
                if (table.Spawned && table.Position.GetVacuum(table.Map) != 0)
                {
                    return true;
                }
            }
            return false;
        }
    }
}