using System.Collections.Generic;
using Verse;

namespace Fortified
{
    public class TurretMannableExtension : DefModExtension
    {
        public bool mannableByDefault = true;
        public List<string> BypassMannable = new List<string>();//特例可以直接用的
    }
}
