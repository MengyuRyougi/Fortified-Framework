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
        public int workTime = 300;
        public int workAmountPerStage = 60000;

        public Dictionary<SkillDef, int> skills = new Dictionary<SkillDef, int>();

        public EffecterDef phaseEffecter_east = null;
        public EffecterDef phaseEffecter_west = null;
        public EffecterDef phaseEffecter_south = null;
        public EffecterDef phaseEffecter_north = null;

        public bool northOnly = false;
        public ThingDef activeMote = null;

        public EffecterDef doneEffecter_east = null;
        public EffecterDef doneEffecter_west = null;
        public EffecterDef doneEffecter_south = null;
        public EffecterDef doneEffecter_north = null;


        public EffecterDef GetEffecterDef_Phase(Rot4 rot)
        {
            if (rot == Rot4.East) return phaseEffecter_east;
            if (rot == Rot4.West) return phaseEffecter_west;
            if (rot == Rot4.South) return phaseEffecter_south;
            if (rot == Rot4.North) return phaseEffecter_north;
            return null;
        }
        public EffecterDef GetEffecterDef_DoneTrigger(Rot4 rot)
        {
            if (rot == Rot4.East) return doneEffecter_east;
            if (rot == Rot4.West) return doneEffecter_west;
            if (rot == Rot4.South) return doneEffecter_south;
            if (rot == Rot4.North) return doneEffecter_north;
            return null;
        }
    }
}