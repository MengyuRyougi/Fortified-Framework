using System.Xml;
using Verse;

namespace Fortified
{
    /// <summary>
    /// 單一條目的掛載描述。XML 可寫 &lt;li&gt;FFF_Info_X&lt;/li&gt; 簡寫，或展開覆寫欄位。
    /// </summary>
    public class InfoEntry
    {
        public FFF_InfoDef def;

        // 可選：覆寫分類內排序
        public int? priorityOverride;

        // 掛在 Hediff / Apparel / Gene 時是否也彙總到 Pawn 卡
        public bool showOnPawnCard = true;

        public InfoEntry() { }

        public InfoEntry(FFF_InfoDef def)
        {
            this.def = def;
        }

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            if (xmlRoot == null)
                return;

            string mayRequire = xmlRoot.Attributes?["MayRequire"]?.Value?.ToLower();
            string mayRequireAny = xmlRoot.Attributes?["MayRequireAnyOf"]?.Value?.ToLower();

            // 簡寫：<li>FFF_Info_X</li>
            if (xmlRoot.ChildNodes.Count == 1 && xmlRoot.FirstChild != null && xmlRoot.FirstChild.NodeType == XmlNodeType.Text)
            {
                string shorthand = xmlRoot.InnerText?.Trim();
                if (!shorthand.NullOrEmpty())
                    DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, nameof(def), shorthand, mayRequire, mayRequireAny);
                return;
            }

            foreach (XmlNode node in xmlRoot.ChildNodes)
            {
                if (node == null || node.NodeType != XmlNodeType.Element)
                    continue;

                string text = node.InnerText?.Trim();
                switch (node.Name)
                {
                    case nameof(def):
                        if (!text.NullOrEmpty())
                            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, nameof(def), text, mayRequire, mayRequireAny);
                        break;
                    case nameof(priorityOverride):
                        priorityOverride = ParseHelper.FromString<int>(text);
                        break;
                    case nameof(showOnPawnCard):
                        showOnPawnCard = ParseHelper.FromString<bool>(text);
                        break;
                    default:
                        Log.Error($"[FFF] InfoEntry: unknown node <{node.Name}> in {xmlRoot.OuterXml}");
                        break;
                }
            }
        }

        public override string ToString()
        {
            return $"InfoEntry({def?.defName ?? "null"})";
        }
    }
}
