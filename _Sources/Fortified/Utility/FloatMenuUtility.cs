using System.Collections.Generic;

using Verse;
using Verse.AI;
using RimWorld;
using System;
using System.Security.Principal;

namespace Fortified
{
    public static class FloatMenuUtility
    {
        public static IEnumerable<FloatMenuOption> GetExtraFloatMenuOptionsFor(FloatMenuContext context, Thing clickedThing, MechWeaponExtension mechWeapon)
        {
            if (!clickedThing.Spawned) yield break;
            if (clickedThing is not ThingWithComps tmp) yield break;

            Pawn pawn = context.FirstSelectedPawn as Pawn;

            //武器相關
            if (tmp.TryGetComp<CompEquippable>() != null)
            {
                if (CheckUtility.IsMechUseable(pawn, tmp))
                {
                    yield return TryMakeFloatMenuForWeapon(pawn, tmp);
                }
                else
                {
                    yield return new FloatMenuOption("CannotEquip".Translate(tmp) + ": " + "FFF.Reason.WeaponNotSupported".Translate(), null);
                }
            }
            //裝備相關
            if (tmp.def?.apparel != null && pawn.TryGetComp<CompMechApparel>(out var comp))
            {
                if (CheckUtility.Wearable(comp, tmp))
                {
                    yield return TryMakeFloatMenuForApparel(pawn, tmp);
                }
                else
                {
                    yield return new FloatMenuOption("CannotEquip".Translate(tmp) + ": " + "FFF.Reason.FrameNotSupported".Translate(), null);
                }
            }
            //撿起物品
            if (tmp.def.selectable && tmp.def.category == ThingCategory.Item)
            {
                if (MassUtility.GearAndInventoryMass(pawn) + tmp.GetStatValue(StatDefOf.Mass) > MassUtility.Capacity(pawn))
                {
                    yield return new FloatMenuOption("CannotEquip".Translate(tmp) + ": " + "FFF.Reason.NoPayloadCapacity".Translate(), null);
                }
                else
                {
                    yield return new FloatMenuOption("FFF.TakeToInventory".Translate(tmp), () =>
                    {
                        tmp.SetForbidden(false);
                        Job job = null;
                        if (tmp.stackCount <= 1)
                        {
                            job = JobMaker.MakeJob(JobDefOf.TakeInventory, tmp);
                            job.count = 1;
                        }
                        else
                        {
                            job = JobMaker.MakeJob(JobDefOf.TakeCountToInventory, tmp);
                            var count = MassUtility.CountToPickUpUntilOverEncumbered(pawn, tmp);
                            job.count = tmp.stackCount > count ? count : tmp.stackCount;
                        }
                        pawn.jobs.TryTakeOrderedJob(job, JobTag.DraftedOrder);
                    });
                }
            }
            //清空物品
            if (tmp == pawn && !pawn.inventory.innerContainer.NullOrEmpty())
            {
                yield return TryMakeFloatMenuForGearManagement(pawn);
            }
            //操作砲塔相關
            if (tmp.def.building?.turretGunDef != null)
            {
                if (CheckUtility.IsMannable(pawn.def.GetModExtension<TurretMannableExtension>(), tmp as Building_Turret))
                {
                    var turret = tmp as Building_Turret;
                    yield return new FloatMenuOption("OrderManThing".Translate(turret.LabelShort, turret), delegate
                    {
                        Job job = JobMaker.MakeJob(JobDefOf.ManTurret, turret);
                        pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true, true);
                        pawn.jobs.TryTakeOrderedJob(job, JobTag.DraftedOrder);
                    });
                }
            }
        }

        public static IEnumerable<FloatMenuOption> GetExtraFloatMenuOptionsFor(Pawn pawn, IntVec3 sq, MechWeaponExtension MechWeapon)
        {
            IWeaponUsable weaponUsable = pawn as IWeaponUsable;
            if (pawn.Map == null)
            {
                Log.Error("Error");
                yield break;
            }
            List<Thing> things = sq.GetThingList(pawn.Map);

            for (int i = 0; i < things.Count; i++)
            {
                if (things[i] is ThingWithComps tmp)
                {
                    if (tmp == null) continue;

                    //武器相關
                    if (tmp.TryGetComp<CompEquippable>() != null)
                    {
                        if (CheckUtility.IsMechUseable(pawn, tmp))
                        {
                            yield return TryMakeFloatMenuForWeapon(pawn, tmp);
                        }
                        else
                        {
                            yield return new FloatMenuOption("CannotEquip".Translate(tmp) + ": " + "FFF.Reason.WeaponNotSupported".Translate(), null);
                        }
                    }
                    //裝備相關
                    if (tmp.def?.apparel != null && pawn.TryGetComp<CompMechApparel>(out var compApparel))
                    {
                        if (CheckUtility.Wearable(compApparel, tmp))
                            yield return TryMakeFloatMenuForApparel(pawn, tmp);
                    }
                    else
                    {
                        yield return new FloatMenuOption("CannotEquip".Translate(tmp) + ":" + "FFF.Reason.FrameNotSupported".Translate(), null);
                    }
                
                    //撿起物品
                    if (tmp.def.selectable && tmp.def.category == ThingCategory.Item)
                    {
                        if (MassUtility.GearAndInventoryMass(pawn) + tmp.GetStatValue(StatDefOf.Mass) > MassUtility.Capacity(pawn))
                        {
                            yield return new FloatMenuOption("CannotEquip".Translate(tmp) + ":" + "FFF.Reason.NoPayloadCapacity".Translate(), null);
                        }
                        else if (tmp.TryGetComp<CompEquippable>(out var comp) && !CheckUtility.IsMechUseable(pawn, tmp))
                        {
                            yield return new FloatMenuOption("CannotEquip".Translate(tmp) + ":" + "FFF.Reason.WeaponNotSupported".Translate(), null);
                        }
                        else
                        {
                            yield return new FloatMenuOption("FFF.TakeToInventory".Translate(tmp), () =>
                            {
                                tmp.SetForbidden(false);
                                Job job = JobMaker.MakeJob(JobDefOf.TakeInventory, tmp);
                                job.count = tmp.stackCount;
                                pawn.jobs.TryTakeOrderedJob(job, JobTag.DraftedOrder);
                            });
                        }
                    }
                    //清空物品
                    if (tmp == pawn && !pawn.inventory.innerContainer.NullOrEmpty())
                    {
                        yield return TryMakeFloatMenuForGearManagement(pawn);
                    }
                    //操作砲塔相關
                    if (tmp.def.building?.turretGunDef != null)
                    {
                        if (CheckUtility.IsMannable(pawn.def.GetModExtension<TurretMannableExtension>(), tmp as Building_Turret))
                        {
                            var turret = tmp as Building_Turret;
                            yield return new FloatMenuOption("OrderManThing".Translate(turret.LabelShort, turret), delegate
                            {
                                Job job = JobMaker.MakeJob(JobDefOf.ManTurret, turret);
                                pawn.jobs.TryTakeOrderedJob(job, JobTag.DraftedOrder);
                            });
                        }
                    }
                }
            }
            yield break;
        }

        private static FloatMenuOption TryMakeFloatMenuForGearManagement(Pawn pawn)
        {
                return new FloatMenuOption("FFF.DropGears".Translate(), () =>
                {
                    pawn.inventory.DropAllNearPawn(pawn.Position);
                });
        }

        public static FloatMenuOption TryMakeFloatMenu(Pawn pawn, ThingWithComps equipment, string key = "Equip")
        {
            string labelShort = equipment.LabelShort;
            if (!pawn.CanReach(equipment, PathEndMode.ClosestTouch, Danger.Deadly, false, false, TraverseMode.ByPawn))
            {
                return new FloatMenuOption("CannotEquip".Translate(labelShort) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
            }

            if (pawn is IWeaponUsable weaponUsable)
            {
                if (equipment is Apparel apparel && pawn.TryGetComp<CompMechApparel>(out var mechApparel))
                {
                    if (!apparel.PawnCanWear(pawn, true) || !CheckUtility.Wearable(mechApparel, apparel))
                    {
                        return new FloatMenuOption("CannotEquip".Translate(labelShort) + ": " + "FFF.FrameNotSupported".Translate(), null);
                    }
                    else
                    {
                        return new FloatMenuOption(key.Translate(labelShort, equipment), () =>
                        {
                            weaponUsable.Wear(equipment);
                        });
                    }
                }
                else
                {
                    return new FloatMenuOption(key.Translate(labelShort, equipment), () =>
                    {
                        weaponUsable.Equip(equipment);
                    });
                }
            }
            return null;
        }


        public static FloatMenuOption TryMakeFloatMenuForWeapon(this Pawn pawn, ThingWithComps equipment)
        {
            return TryMakeFloatMenu(pawn, equipment);
        }
        public static FloatMenuOption TryMakeFloatMenuForApparel(this Pawn pawn, ThingWithComps equipment)
        {
            string key = equipment.def.apparel.LastLayer.IsUtilityLayer ? "ForceWear" : "ForceEquipApparel";
            return TryMakeFloatMenu(pawn, equipment, key);
        }
    }
}
