using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace Fortified
{
    public class WeaponModificationExtension : DefModExtension
    {
        public List<ApplyWeapon> applyWeapons = new List<ApplyWeapon>();

        public ThingStyleDef GetStyle(Thing thing)
        {
            foreach (var item in applyWeapons)
            {
                if (item.weaponDef != thing.def.defName) continue;

                ThingStyleDef style = DefDatabase<ThingStyleDef>.GetNamed(item.styleDef.RandomElement());
                if (style != null)
                {
                    return style;
                }
            }
            return null;
        }
    }
    [Serializable]
    public class ApplyWeapon
    {
        public string weaponDef;
        public List<string> styleDef;

    }
}