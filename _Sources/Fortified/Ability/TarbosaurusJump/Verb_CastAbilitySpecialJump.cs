using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Fortified
{
    public class Verb_CastAbilitySpecialJump: Verb_CastAbilityJump
    {
        public override ThingDef JumpFlyerDef
        {
            get
            {
                return Ability.def.GetModExtension<ModExtensionJumper>().pawnFlyer;
            }
        }
    }
}
