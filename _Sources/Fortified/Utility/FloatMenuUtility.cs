using System.Collections.Generic;

using Verse;
using Verse.AI;
using RimWorld;
using System;

namespace Fortified
{
    public static class FloatMenuUtility
    {
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
                            yield return new FloatMenuOption("CannotEquip".Translate(tmp) + " " + "DMS_WeaponNotSupported".Translate(), null);
                        }
                    }
                    //裝備相關
                    if (tmp.def?.apparel != null && pawn.HasComp<CompMechApparel>())
                    {
                        if (CheckUtility.Wearable(MechWeapon, tmp))
                        {
                            yield return TryMakeFloatMenuForApparel(pawn, tmp);
                        }
                        else
                        {
                            yield return new FloatMenuOption("CannotEquip".Translate(tmp) + " " + "DMS_FrameNotSupported".Translate(), null);
                        }
                    }
                    //撿起物品
                    if (tmp.def.selectable && tmp.def.category == ThingCategory.Item)
                    {
                        if (MassUtility.GearAndInventoryMass(pawn) + tmp.GetStatValue(StatDefOf.Mass) > MassUtility.Capacity(pawn))
                        {
                            yield return new FloatMenuOption("CannotEquip".Translate(tmp) + " " + "DMS_NoPayloadCapacity".Translate(), null);
                        }
                        else if (tmp.TryGetComp<CompEquippable>(out var comp) && !CheckUtility.IsMechUseable(pawn, tmp))
                        {
                            yield return new FloatMenuOption("CannotEquip".Translate(tmp) + " " + "DMS_WeaponNotSupported".Translate(), null);
                        }
                        else
                        {
                            yield return new FloatMenuOption("DMS_TakeToInventory".Translate(tmp), () =>
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
                return new FloatMenuOption("DMS_DropGears".Translate(), () =>
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
                if (equipment is Apparel apparel)
                {
                    if (!apparel.PawnCanWear(pawn, true) || !CheckUtility.Wearable(pawn.def.GetModExtension<MechWeaponExtension>(),apparel))
                    {
                        return new FloatMenuOption("CannotEquip".Translate(labelShort) + ": " + "DMS_FrameNotSupported".Translate(), null);
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
