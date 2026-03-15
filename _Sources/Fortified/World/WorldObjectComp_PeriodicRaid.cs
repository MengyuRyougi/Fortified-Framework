using RimWorld;
using RimWorld.Planet;
using Verse;
using UnityEngine;

namespace Fortified
{
    public class WorldObjectComp_PeriodicRaid : WorldObjectComp
    {
        public WorldObjectCompProperties_PeriodicRaid Props => (WorldObjectCompProperties_PeriodicRaid)props;

        private int ticksToNextRaid;

        // 检查据点组件有效性
        private bool IsActive()
        {
            if (parent is MapParent mapParent && mapParent.HasMap) return false;
            if (Props.requiresAnySitePart.NullOrEmpty()) return true;
            if (parent is Site site)
            {
                foreach (var required in Props.requiresAnySitePart)
                {
                    if (site.parts.Any(p => p.def == required)) return true;
                }
            }
            return false;
        }

        public override void Initialize(WorldObjectCompProperties props)
        {
            base.Initialize(props);
            ResetTimer();
        }

        public override void CompTick()
        {
            base.CompTick();
            
            // 激活状态时更新冷却时间
            if (parent.Destroyed || !IsActive()) return;

            ticksToNextRaid--;
            if (ticksToNextRaid <= 0)
            {
                GenerateRaid();
                ResetTimer();
            }
        }

        private void ResetTimer()
        {
            ticksToNextRaid = Mathf.RoundToInt(Props.daysBetweenRaids.RandomInRange * GenDate.TicksPerDay);
        }

        private void GenerateRaid()
        {
            Map map = Find.AnyPlayerHomeMap;
            if (map == null) return;
            
            IncidentParms parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, map);
            
            if (Props.forcedFaction != null)
            {
                parms.faction = Find.FactionManager.FirstFactionOfDef(Props.forcedFaction) ?? parms.faction;
            }
            else
            {
                parms.faction = parent.Faction ?? Find.FactionManager.RandomEnemyFaction(false, false, true, TechLevel.Industrial);
            }
            
            parms.points = StorytellerUtility.DefaultThreatPointsNow(map) * Props.pointsFactor;
            
            if (IncidentDefOf.RaidEnemy.Worker.TryExecute(parms))
            {
                if (!Props.letterLabel.NullOrEmpty())
                {
                    Find.LetterStack.ReceiveLetter(
                        Props.letterLabel.Translate(), 
                        Props.letterText.Translate(), 
                        LetterDefOf.ThreatBig, 
                        new RimWorld.Planet.GlobalTargetInfo(parms.target.Tile));
                }
            }
        }

        public override string CompInspectStringExtra()
        {
            if (parent.Destroyed) return null;
            if (parent is MapParent mapParent && mapParent.HasMap)
                return "FFF_Site_LocalMonitoring".Translate();

            if (!IsActive()) return null;

            return "FFF_NextRaidIn".Translate(ticksToNextRaid.ToStringTicksToPeriod());
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref ticksToNextRaid, "ticksToNextRaid", 0);
        }
    }
}
