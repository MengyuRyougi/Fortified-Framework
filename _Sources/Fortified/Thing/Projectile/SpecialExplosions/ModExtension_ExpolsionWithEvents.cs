using System.Collections.Generic;
using Verse;

namespace Fortified
{
    public class ModExtension_ExpolsionWithConditions : DefModExtension
    {
        public List<Condition> conditions;
    }

    public class Condition
    {
        public GameConditionDef conditionDef;
        public int percent;
        public IntRange duration;
    }
}
