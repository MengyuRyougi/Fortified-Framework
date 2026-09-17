using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Fortified
{
    /// <summary>
    /// 訊息卡「特殊機制」純文字條目。label 為標題、description 為內文，
    /// 同一機制可被多個 Def 引用，翻譯走 DefInjected。
    /// </summary>
    public class FFF_InfoDef : Def
    {
        // 預設 FFF_DefOf.FFF_Mechanics
        public StatCategoryDef category;

        // 分類內排序基準，越大越前
        public int displayPriority = 1000;

        // 可連結到相關 Def
        public List<DefHyperlink> hyperlinks;

        // 動態文字擴充點
        public Type workerClass = typeof(InfoWorker);

        // hideStats 的 Def 仍顯示
        public bool overridesHideStats = false;

        // 自動附加：owner Def 的來源物件是這些型別（含子類）時自動加入
        public List<Type> autoAttachTo;

        // false 則只比對完全相同型別
        public bool autoAttachInherit = true;

        private InfoWorker workerInt;

        public InfoWorker Worker
        {
            get
            {
                if (workerInt == null)
                {
                    Type type = workerClass ?? typeof(InfoWorker);
                    workerInt = (InfoWorker)Activator.CreateInstance(type);
                }
                return workerInt;
            }
        }

        public StatCategoryDef Category => category ?? FFF_DefOf.FFF_Mechanics;

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string err in base.ConfigErrors())
                yield return err;

            if (workerClass != null && !typeof(InfoWorker).IsAssignableFrom(workerClass))
                yield return $"workerClass {workerClass} does not inherit {nameof(InfoWorker)}.";

            if (autoAttachTo != null)
            {
                foreach (Type t in autoAttachTo)
                {
                    if (t == null)
                        yield return "autoAttachTo contains an unresolved type.";
                }
            }
        }
    }
}
