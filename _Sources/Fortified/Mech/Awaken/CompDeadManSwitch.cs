using RimWorld;
using Verse;
using Verse.AI.Group;
using Verse.AI;
using System.Collections.Generic;
using Verse.Noise;
using System.Linq;
using RimWorld.Planet;
using System;
using Fortified;

namespace Fortified
{
    public class CompDeadManSwitch : ThingComp
    {
        public CompProperties_DeadManSwitch Props => (CompProperties_DeadManSwitch)props;
        public Pawn Overseer
        {
            get
            {
                Pawn pawn = (Pawn)this.parent;
                if (pawn == null)
                {
                    return null;
                }
                Pawn_RelationsTracker relations = pawn.relations;
                if (relations == null)
                {
                    return null;
                }
                return relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Overseer, null);
            }
        }
        private int delayCheck;

        public bool woken;

        public bool woken_Lurk;
        public int timeToWake;

        public bool outgoing;
        public int outgoingTime;
        public override void CompTick()
        {
            if (delayCheck > 0)
            {
                delayCheck--;
            }
            if (delayCheck <= 0)
            {
                TryTriggerDMS();
                delayCheck = Props.minDelayUntilDMS;
            }
        }
        public void Wake()
        {
            this.woken_Lurk = false;
            if (!this.woken)
            {
                this.woken = true;
                Pawn pawn = ((Pawn)this.parent);
                pawn.Name = new NameSingle(NameGenerator.GenerateName(this.Props.nameRule ?? RulePackDefOf.NamerTraderGeneral));

                Pawn_RelationsTracker relations = pawn.relations;
                if (relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Overseer, null) is Pawn overseer)
                {
                    pawn.relations.RemoveDirectRelation(PawnRelationDefOf.Overseer, overseer);
                }
                Find.LetterStack.ReceiveLetter("DMS_MechWake".Translate(this.parent.Label), "DMS_MechWakeDesc".Translate(this.parent.Label), LetterDefOf.PositiveEvent, this.parent);
                pawn.interactions = new Pawn_InteractionsTracker(pawn);
            }
        }
        private void TryTriggerDMS()
        {
            if (this.woken && this.Overseer != null)
            {
                this.woken_Lurk = false;
                this.woken = false;
            }

            if (!this.woken && this.woken_Lurk)
            {
                if (this.timeToWake <= 0) this.Wake();
                this.timeToWake -= Props.minDelayUntilDMS;
            }
            if (this.parent.Spawned && this.woken && this.outgoing)
            {
                this.outgoingTime += Props.minDelayUntilDMS;
                if (this.outgoingTime >= 60000 * 2 && this.parent is Pawn pawn
                    && !pawn.Downed && pawn.CurJobDef != DMS_DefOf.DMS_MechLeave &&
                    RCellFinder.TryFindBestExitSpot(pawn, out IntVec3 spot))
                {
                    Find.LetterStack.ReceiveLetter("DMS_MechStartLeave".Translate(this.parent.Label), "DMS_MechStartLeaveDesc".Translate(this.parent.Label), LetterDefOf.PositiveEvent, this.parent);
                    Job job = JobMaker.MakeJob(DMS_DefOf.DMS_MechLeave, spot);
                    job.exitMapOnArrival = true;
                    pawn.jobs.StartJob(job);
                }
            }
        }
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            delayCheck = Props.minDelayUntilDMS;
            if (!respawningAfterLoad && Current.Game.GetComponent<GameComponent_DMS>() is GameComponent_DMS comp
                && comp.OutgoingMeches.Find(m => m.mech == this.parent) is OutgoingMech mech)
            {
                comp.removedCache ??= new List<Pawn>();
                comp.removedCache.Add(this.parent as Pawn);
                this.outgoing = false;
                this.outgoingTime = 0;
            }
        }
        public override string CompInspectStringExtra()
        {
            if (!parent.Faction.IsPlayer || parent.GetComp<CompOverseerSubject>() == null) return null;
            if (parent.GetComp<CompOverseerSubject>().State != OverseerSubjectState.Overseen)
            {
                string str = "DMS_WillTerminateTheBetrayedUnit".Translate();
                return str;
            }
            return null;
        }
        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            IThingHolder holder = this.parent.holdingOwner?.Owner;
            if (this.Overseer != null && (holder as Thing == null && holder as Caravan == null) && Current.Game.GetComponent<GameComponent_DMS>() is GameComponent_DMS dms && !dms.lostMechs.Contains(this.parent) && !this.woken)
            {
                dms.lostMechs.Add(this.parent);
            }
        }
        public override void Notify_Killed(Map prevMap, DamageInfo? dinfo = null)
        {
            base.Notify_Killed(prevMap, dinfo);
            this.woken = false;
            this.woken_Lurk = false;
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (Prefs.DevMode && DebugSettings.godMode)
            {
                yield return new Command_Action()
                {
                    defaultLabel = "Debug: Check State",
                    action = () =>
                    {
                        Log.Message(this.woken);
                        Log.Message(this.woken_Lurk);
                        Log.Message(this.timeToWake);
                    }
                };
                yield return new Command_Action()
                {
                    defaultLabel = "Debug: Wake",
                    action = () =>
                    {
                        this.Wake();
                    }
                };
                yield return new Command_Action()
                {
                    defaultLabel = "Debug: Outgoing",
                    action = () =>
                    {
                        this.outgoing = true;
                        this.outgoingTime = 60000 * 2;
                    }
                };
            }
            yield break;
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref delayCheck, "delayCheck", 0);
            Scribe_Values.Look(ref timeToWake, "timeToWake", 0);
            Scribe_Values.Look(ref woken_Lurk, "woken_Lurk");
            Scribe_Values.Look(ref woken, "woken", false);
            Scribe_Values.Look(ref outgoing, "outgoing");
            Scribe_Values.Look(ref outgoingTime, "outgoingTime");
        }
    }

    public class CompProperties_DeadManSwitch : CompProperties
    {
        public int minDelayUntilDMS = 3000;
        public float wakingChance = 0.3f;
        public RulePackDef nameRule;
        public CompProperties_DeadManSwitch()
        {
            compClass = typeof(CompDeadManSwitch);
        }
    }
}