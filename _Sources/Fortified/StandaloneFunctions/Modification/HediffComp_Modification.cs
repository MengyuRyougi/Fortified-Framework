using Verse;

namespace Fortified
{
    public class HediffComp_Modification : HediffComp
    {
        public bool isApplyTarget = false;
        public HediffCompProperties_Modification Props
        {
            get
            {
                return (HediffCompProperties_Modification)props;
            }
        }
    }
    public class HediffCompProperties_Modification : HediffCompProperties
    {
        public HediffCompProperties_Modification()
        {
            compClass = typeof(HediffComp_Modification);
        }
        public JobDef applyJob;
    }
}
