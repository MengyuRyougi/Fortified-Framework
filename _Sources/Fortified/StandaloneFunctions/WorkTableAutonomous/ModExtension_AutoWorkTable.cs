using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Fortified
{
    public class ModExtension_AutoWorkTable : DefModExtension
    {
        //每级降低工作后周期时间，降低时间 = 技能级别 * 对应的值
        public Dictionary<SkillDef,int> skills = new Dictionary<SkillDef,int>();
    }
}
