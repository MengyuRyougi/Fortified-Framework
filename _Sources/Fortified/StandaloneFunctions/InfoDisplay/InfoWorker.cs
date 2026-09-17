using RimWorld;
using Verse;

namespace Fortified
{
    /// <summary>
    /// 條目輸出時的上下文。source 是觸發此條目的來源物件
    /// （CompProperties / DefModExtension / Def 本身 / Type…），可為 null。
    /// </summary>
    public readonly struct InfoContext
    {
        public readonly Def owner;
        public readonly StatRequest req;
        public readonly object source;

        public InfoContext(Def owner, StatRequest req, object source)
        {
            this.owner = owner;
            this.req = req;
            this.source = source;
        }

        public Thing Thing => req.HasThing ? req.Thing : null;

        // 取來源物件並轉型，方便 Worker 讀取欄位
        public T SourceAs<T>() where T : class => source as T;
    }

    /// <summary>
    /// 動態文字擴充點。預設直接回傳 Def 的靜態文本；
    /// 需要帶數值時覆寫並從 ctx.source / ctx.Thing 取值。
    /// </summary>
    public class InfoWorker
    {
        public virtual bool Visible(FFF_InfoDef def, InfoContext ctx) => true;

        public virtual string GetLabel(FFF_InfoDef def, InfoContext ctx) => def.LabelCap;

        public virtual string GetDescription(FFF_InfoDef def, InfoContext ctx) => def.description;
    }
}
