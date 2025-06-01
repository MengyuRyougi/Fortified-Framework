using RimWorld;
using Verse;

namespace Fortified;
public class IngredientValueGetter_MarketValue : IngredientValueGetter
{
    public override string BillRequirementsDescription(RecipeDef r, IngredientCount ing)
    {
        return "FT_BillRequires_MarketValue".Translate(ing.GetBaseCount()) + " (" + ing.filter.Summary + ")";
    }

    public override float ValuePerUnitOf(ThingDef t)
    {
        if (t.BaseMarketValue <= 0f) return 0f;
        return t.GetStatValueAbstract(StatDefOf.MarketValue);
    }
}