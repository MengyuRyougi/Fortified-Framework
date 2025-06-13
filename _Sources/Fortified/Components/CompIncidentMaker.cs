using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Fortified
{
    //激活後過段時間發生事件
    public class CompProperties_IncidentMaker : CompProperties
    {
        public IncidentDef Incident;

        public EffecterDef Effecter;

        public IntVec3 effecterOffset = IntVec3.Zero;

        public int WarmupTicks = 36000;

        public int CooldownTicks = 36000;

        public string iconPathActive;

        public string iconPathDeactive;

        public CompProperties_IncidentMaker()
        {
            compClass = typeof(CompIncidentMaker);
        }
    }

    public class CompIncidentMaker : ThingComp
    {
        private bool isActive;

        private int ticksUntilTrigger;

        private int ticksUntilCooldown;

        public CompProperties_IncidentMaker Props => (CompProperties_IncidentMaker)props;

        private CompPowerTrader CompPower => parent.TryGetComp<CompPowerTrader>();

        private bool PowerOn
        {
            get
            {
                if (CompPower != null)
                {
                    return CompPower.PowerOn;
                }
                return true;
            }
        }

        private CompRefuelable CompFuel => parent.TryGetComp<CompRefuelable>();

        private bool HasFuel
        {
            get
            {
                if (CompFuel != null)
                {
                    return CompFuel.HasFuel;
                }
                return true;
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (DebugSettings.ShowDevGizmos)
            {
                Command_Action command_Action = new Command_Action();
                command_Action.defaultLabel = "DEV: Trigger " + Props.Incident.label;
                command_Action.icon = TexCommand.DesirePower;
                command_Action.action = delegate
                {
                    TryDoIncident();
                };
                yield return command_Action;
            }
            if (isActive)
            {
                Command_Action command_Action2 = new Command_Action();
                command_Action2.defaultLabel = "FFF.StopWarmup".Translate();
                command_Action2.icon = ContentFinder<Texture2D>.Get(Props.iconPathDeactive);
                command_Action2.action = delegate
                {
                    ResetCountdown();
                    isActive = false;
                };
                yield return command_Action2;
                yield break;
            }
            Command_Action command_Action3 = new Command_Action();
            command_Action3.defaultLabel = "FFF.StartWarmup".Translate();
            command_Action3.icon = ContentFinder<Texture2D>.Get(Props.iconPathActive);
            if (ticksUntilCooldown > 0)
            {
                command_Action3.Disable("FFF.NotCooldown".Translate());
            }
            command_Action3.action = delegate
            {
                ResetCountdown();
                isActive = true;
            };
            yield return command_Action3;
        }

        public override string CompInspectStringExtra()
        {
            string text = "";
            if (isActive)
            {
                if (ticksUntilTrigger > 0)
                {
                    text += "FFF.WillReadyAt".Translate() + " " + ticksUntilTrigger.ToStringTicksToPeriod().Colorize(ColoredText.DateTimeColor);
                }
            }
            else if (ticksUntilCooldown > 0)
            {
                text += "FFF.WillCooldownAt".Translate() + " " + ticksUntilCooldown.ToStringTicksToPeriod().Colorize(ColoredText.DateTimeColor);
            }
            return base.CompInspectStringExtra() + text;
        }

        public override void CompTick()
        {
            TickInterval(1);
        }

        public override void CompTickRare()
        {
            TickInterval(250);
        }

        private void TickInterval(int interval)
        {
            if (!parent.Spawned)
            {
                return;
            }
            CompCanBeDormant comp = parent.GetComp<CompCanBeDormant>();
            if (comp != null)
            {
                if (!comp.Awake)
                {
                    return;
                }
            }
            else if (parent.Position.Fogged(parent.Map))
            {
                return;
            }
            if (PowerOn && HasFuel)
            {
                if (isActive)
                {
                    ticksUntilTrigger -= interval;
                    CheckShouldSpawn();
                }
                else
                {
                    ticksUntilCooldown -= interval;
                }
            }
        }

        private void CheckShouldSpawn()
        {
            if (ticksUntilTrigger <= 0)
            {
                isActive = false;
                TryDoIncident();
            }
        }

        private void ResetCountdown()
        {
            ticksUntilTrigger = Props.WarmupTicks;
            ticksUntilCooldown = Props.CooldownTicks;
        }

        public override void PostExposeData()
        {
            string defName = Props.Incident.defName;
            Scribe_Values.Look(ref isActive, defName + "IsWarmingUp", defaultValue: false);
            Scribe_Values.Look(ref ticksUntilTrigger, defName + "TicksUntilSpawn", 0);
            Scribe_Values.Look(ref ticksUntilCooldown, defName + "TicksUntilReady", 0);
        }

        private void TryDoIncident()
        {
            IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(Props.Incident.category, parent.MapHeld);
            incidentParms.target = parent.MapHeld;
            incidentParms.points = parent.MapHeld.PlayerWealthForStoryteller;
            if (Props.Incident.Worker.CanFireNow(incidentParms))
            {
                Props.Incident.Worker.TryExecute(incidentParms);
            }
        }
    }
}