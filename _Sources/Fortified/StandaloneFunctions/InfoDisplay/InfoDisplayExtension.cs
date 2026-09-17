using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Fortified
{
    /// <summary>
    /// 手動掛載：在任意 Def 的 modExtensions 中列出要顯示的 InfoDef。
    /// suppress 可排除自動附加進來、但此 Def 不想顯示的 InfoDef。
    /// </summary>
    public class InfoDisplayExtension : DefModExtension, IInfoProvider
    {
        public List<InfoEntry> infos;

        public List<FFF_InfoDef> suppress;

        public IEnumerable<InfoEntry> GetInfoEntries(InfoContext ctx)
        {
            return infos ?? Enumerable.Empty<InfoEntry>();
        }

        public bool Suppresses(FFF_InfoDef def)
        {
            return suppress != null && suppress.Contains(def);
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string err in base.ConfigErrors())
                yield return err;

            if (infos != null)
            {
                for (int i = 0; i < infos.Count; i++)
                {
                    if (infos[i] == null || infos[i].def == null)
                        yield return $"{nameof(InfoDisplayExtension)}: infos[{i}] has no def.";
                }
            }
        }
    }
}
