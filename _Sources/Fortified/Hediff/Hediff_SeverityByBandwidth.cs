using RimWorld;
using System.Collections.Generic;
using Verse;
namespace Fortified
{
    /// <summary>
    /// NACS動態調整，占用帶寬越多越嚴重
    /// </summary>
    public class Hediff_SeverityByBandwidth : HediffWithComps
    {
        public override void PostTick()
        {
            base.PostTick();
            if (pawn.IsHashIntervalTick(60))
            {
                if (pawn.mechanitor != null)
                {
                    this.Severity = pawn.mechanitor.UsedBandwidth != 0 ? (float)pawn.mechanitor.UsedBandwidth / (float)pawn.mechanitor.TotalBandwidth : 0.1f;
                }
            }
        }
    }
}
