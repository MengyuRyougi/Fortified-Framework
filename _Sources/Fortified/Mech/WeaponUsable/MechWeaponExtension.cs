using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Fortified
{
    public class MechWeaponExtension : DefModExtension
    {
        public bool EnableWeaponFilter = true;  //根據WeaponTag
        public List<string> UsableWeaponTags = new List<string>();

        public bool EnableTechLevelFilter = false; //根據科技等級
        public List<TechLevel> UsableTechLevels = new List<TechLevel>();

        public bool EnableClassFilter = false; //根據文化分類
        public List<WeaponClassDef> UsableWeaponClasses = new List<WeaponClassDef>();

        public List<string> BypassUsableWeapons = new List<string>();
    }
}
