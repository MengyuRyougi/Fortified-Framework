using Verse;

namespace Fortified
{
    /// <summary>
    /// 以整數等級計算嚴重度、但標籤顯示目前 stage 名稱（而非原版的「(等級 N)」）的 hediff。
    /// </summary>
    public class Hediff_LevelLabel : Hediff_Level
    {
        public override string Label
        {
            get
            {
                if (def.levelIsQuantity) return def.label + " x" + level;
                // 用 CurStage 而不是 stages[level - 1]：等級可能超出 stage 數（例如 lethalSeverity 那一級），或 stage 不與等級一一對應。
                string stageLabel = CurStage?.label;
                if (stageLabel.NullOrEmpty()) return def.label;
                return def.label + " (" + stageLabel + ")";
            }
        }
    }
}
