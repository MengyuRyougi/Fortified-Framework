using RimWorld;
using Verse;

namespace Fortified;
public class IngredientValueGetter_Mass : IngredientValueGetter
{
    public override string BillRequirementsDescription(RecipeDef r, IngredientCount ing)
    {
        return "FT_BillRequires_Mass".Translate(ing.GetBaseCount()) + " (" + ing.filter.Summary + ")";
    }

    public override float ValuePerUnitOf(ThingDef t)
    {
        return t.GetStatValueAbstract(StatDefOf.Mass);
    }
}