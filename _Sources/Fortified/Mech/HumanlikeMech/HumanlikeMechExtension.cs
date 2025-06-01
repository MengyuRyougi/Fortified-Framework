using Verse;
using RimWorld;
using UnityEngine;
using System.Collections.Generic;

namespace Fortified
{
    public class HumanlikeMechExtension : DefModExtension
    {
        public bool canChangeHairStyle = false;
        public BodyTypeDef bodyTypeOverride;
        public HeadTypeDef headTypeOverride;
        public Vector3 headOffset;
        public GraphicData headGraphic = null;
        public GraphicData headGraphicHaired = null;
        public List<SkillRange> skills = null;
    }
}