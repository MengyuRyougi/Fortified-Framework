using System.Collections.Generic;
using Verse;
using RimWorld;
using UnityEngine;
using Verse.AI;
using System.Linq;
using Fortified;

namespace Fortified
{
    public class WeaponUsableMech : Pawn, IWeaponUsable
    {
        public MechWeaponExtension MechWeapon { get; private set; }
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            MechWeapon = def.GetModExtension<MechWeaponExtension>();
            interactions ??= new(this);
            skills ??= new(this);
            skills.skills.ForEach(s => s.Level = def.race.mechFixedSkillLevel == 0 ? 5 : def.race.mechFixedSkillLevel);
        }
        public override void Kill(DamageInfo? dinfo, Hediff exactCulprit = null)
        {
            if (dinfo == null)//解體殺
            {
                List<Hediff> hediffs = health.hediffSet.hediffs.Where(h => h.def.spawnThingOnRemoved != null).ToList();
                foreach (Hediff item in hediffs)
                {
                    health.RemoveHediff(item);
                    Thing thing = ThingMaker.MakeThing(item.def.spawnThingOnRemoved);
                    thing.stackCount = 1;
                    GenPlace.TryPlaceThing(thing, this.Position, this.Map, ThingPlaceMode.Near);
                }
            }
            base.Kill(dinfo, exactCulprit);
        }
        public override IEnumerable<FloatMenuOption> GetExtraFloatMenuOptionsFor(IntVec3 sq)
        {
            if (!IsColonyMechPlayerControlled) yield break;

            if (this.TryGetComp<CompDeadManSwitch>() is CompDeadManSwitch comp && comp.woken && sq == this.Position
                && MechRepairUtility.CanRepair(this))
            {
                yield return new FloatMenuOption("RepairMech".Translate(this.LabelShort), () =>
                {
                    Job job = JobMaker.MakeJob(FFF_DefOf.FFF_RepairSelf, this);
                    this.jobs.StartJob(job);
                });
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                Drawer?.renderer?.SetAllGraphicsDirty();
            }
        }
        public void Equip(ThingWithComps equipment)
        {
            equipment.SetForbidden(false);
            jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.Equip, equipment), JobTag.Misc);
        }
        public void Wear(ThingWithComps apparel)
        {
            apparel.SetForbidden(false);
            this.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.Wear, apparel), JobTag.Misc);
            this.equipment.AddEquipment(apparel);
        }
    }
}
