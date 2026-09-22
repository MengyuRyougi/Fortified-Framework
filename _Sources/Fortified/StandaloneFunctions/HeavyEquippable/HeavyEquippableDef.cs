using System.Collections.Generic;
using Verse;

namespace Fortified
{
    public class HeavyEquippableDef : Def
    {
        public float EquippableBaseBodySize = 1;
        public List<ThingDef> EquippableByRace = new List<ThingDef>();//指定種族可以無視體型需求直接裝備，主要給HAR兼容使用
        public List<ThingDef> EquippableWithApparel = new List<ThingDef>();//穿戴指定衣物時可以無視體型需求直接裝備
        public List<HediffDef> EquippableWithHediff = new List<HediffDef>();//具備特定Hediff時可以無視體型需求直接裝備，主要是仿生體
        public List<GeneDef> EquippableWithGene = new List<GeneDef>();//指定基因攜帶者(譜系跟異種都可以無視體型需求直接裝備。)

        /// <summary>
        /// 掛載型武器（體型需求為負值，慣用 -1）。
        /// 這類武器沒有體型門檻可言，不管體型多大都不能靠體型裝備，
        /// 只能透過種族／服裝／Hediff／基因白名單，或機械體的武器系統白名單取得。
        /// </summary>
        public bool IsMountedWeapon => EquippableBaseBodySize < 0f;
    }
}
