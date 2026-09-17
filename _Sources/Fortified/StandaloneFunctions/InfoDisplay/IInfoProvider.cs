using System.Collections.Generic;

namespace Fortified
{
    /// <summary>
    /// 由 InfoDisplayUtility 列舉到的來源物件（CompProperties / HediffCompProperties /
    /// DefModExtension / AbilityCompProperties…）實作此介面，即可自動提供訊息卡條目。
    /// 可依自身欄位決定是否顯示。
    /// </summary>
    public interface IInfoProvider
    {
        IEnumerable<InfoEntry> GetInfoEntries(InfoContext ctx);
    }
}
