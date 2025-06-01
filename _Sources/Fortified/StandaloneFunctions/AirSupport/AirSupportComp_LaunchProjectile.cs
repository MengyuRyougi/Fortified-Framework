using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Fortified
{
    public class AirSupportComp_Bombard : AirSupportComp
    {
        public ThingDef ProjectileDef;

        public float spreadRadius = 5;

        public IntRange delayRange = new(120, 150);
        public IntRange delayRangeSound = new(20, 30);//在這裡是聲音到抵達之間的間隔。

        public IntRange burstCount = new(1, 2), burstInterval = new(5, 20);

        public SoundDef soundDef;

        public override void DrawHighlight(Map map, IntVec3 callerPos, LocalTargetInfo target)
        {
            GenDraw.DrawRadiusRing(target.Cell, spreadRadius / 2, Color.white);
            GenDraw.DrawRadiusRing(target.Cell, spreadRadius * 1.5f, Color.red);
        }
        public override void Trigger(AirSupportDef def, Thing triggerer, Map map, LocalTargetInfo target)
        {
            int delay = Find.TickManager.TicksGame + delayRange.RandomInRange;
            var c = GenRadial.NumCellsInRadius(Mathf.Abs(Rand.Gaussian(0, spreadRadius)));
            var count = burstCount.RandomInRange;
            for (int i = 0; i < count; i++)
            {
                var cell = GenRadial.RadialPattern[Rand.RangeInclusive(0, c)] + target.Cell;
                if (!cell.InBounds(map))
                {
                    i--;
                    continue;
                }

                GameComponent_CAS.AddData(new AirSupportData_LaunchProjectile()
                {
                    projectileDef = ProjectileDef,
                    map = map,
                    target = GenRadial.RadialPattern[Rand.RangeInclusive(0, c)] + target.Cell,
                    triggerTick = delay,
                    triggerer = triggerer,
                    triggerFaction = triggerer.Faction,
                    origin = def.tempOriginCache
                });
                int interval = burstInterval.RandomInRange;
                if (soundDef != null) GameComponent_CAS.AddData(new AirSupportData_Sound()
                {
                    soundDef = soundDef,
                    map = map,
                    target = def.tempOriginCache.ToIntVec3(),
                    triggerTick = Find.TickManager.TicksGame + delayRangeSound.RandomInRange + interval,
                    triggerer = triggerer,
                    triggerFaction = triggerer.Faction,
                });

                delay += interval;
            }
        }
    }
    public class AirSupportComp_Strafe : AirSupportComp
    {
        public ThingDef ProjectileDef, flyByThingDef;

        public float spreadRadius = 5;

        public int burstCount = 1, burstInterval = 5, flyByDelay = 60;

        public IntRange delayRange = new(120, 150);

        public SoundDef soundDef;

        public override void Trigger(AirSupportDef def, Thing triggerer, Map map, LocalTargetInfo target)
        {
            int delay = Find.TickManager.TicksGame + delayRange.RandomInRange;
            var c = GenRadial.NumCellsInRadius(spreadRadius);

            for (int i = 0; i < burstCount; i++)
            {
                var cell = GenRadial.RadialPattern[Rand.RangeInclusive(0, c)] + target.Cell;
                if (!cell.InBounds(map))
                {
                    i--;
                    continue;
                }

                GameComponent_CAS.AddData(new AirSupportData_LaunchProjectile()
                {
                    projectileDef = ProjectileDef,
                    map = map,
                    target = GenRadial.RadialPattern[Rand.RangeInclusive(0, c)] + target.Cell,
                    triggerTick = delay,
                    triggerer = triggerer,
                    triggerFaction = triggerer.Faction,
                    origin = def.tempOriginCache
                });

                if (soundDef != null) GameComponent_CAS.AddData(new AirSupportData_Sound()
                {
                    soundDef = soundDef,
                    map = map,
                    target = def.tempOriginCache.ToIntVec3(),
                    triggerTick = delay,
                    triggerer = triggerer,
                    triggerFaction = triggerer.Faction,
                });

                delay += burstInterval;
            }
            //I need to know when the burst ends, so this have to be integrated.
            if (flyByThingDef != null)
            {
                GameComponent_CAS.AddData(new AirSupportData_SpawnFlyBy()
                {
                    thingDef = flyByThingDef,
                    map = map,
                    target = target.Cell,
                    triggerTick = delay + flyByDelay,
                    triggerer = triggerer,
                    triggerFaction = triggerer.Faction,
                    origin = def.tempOriginCache
                });
            }
        }
    }

    public class AirSupportComp_StrafeII : AirSupportComp
    {
        //                   bursts       totalBurstingTime
        //  |----delayRange----|---|---|---|---|---|---lastshotdelay---|
        //                       |---------vehicleFlightTick-----------|
        //summon tick     vehicle spawn or first               actualFlyByTick
        //                shot, whichever comes first


        public ThingDef ProjectileDef, flyByThingDef;

        public float spreadRadius = 5, intendedTargetChance = 0.25f;

        public int burstCount = 1, burstInterval = 5, singleBurstCount = 1;

        public IntRange delayRange = new(120, 150), lastShotDelayRange = new(30, 70);

        public SoundDef soundDef;

        public List<Vector3> projOffsets = new() { Vector3.zero };

        public override void Trigger(AirSupportDef def, Thing triggerer, Map map, LocalTargetInfo target)
        {
            int totalBurstingTime = (burstCount - 1) * burstInterval;
            int vehicleFlightTime = (int)((target.Cell.ToVector3Shifted() - def.tempOriginCache).Yto0().magnitude * 60 / flyByThingDef.skyfaller.speed);
            int actualFlyByTick = Find.TickManager.TicksGame + delayRange.RandomInRange + Mathf.Max(vehicleFlightTime, totalBurstingTime);
            if (flyByThingDef != null)
            {
                var vehicleSpawnTick = actualFlyByTick - vehicleFlightTime;
                var d = new AirSupportData_SpawnFlyBy()
                {
                    thingDef = flyByThingDef,
                    map = map,
                    target = target.Cell,
                    triggerTick = vehicleSpawnTick,
                    triggerer = triggerer,
                    triggerFaction = triggerer.Faction,
                    origin = def.tempOriginCache
                };
                var c = GenRadial.NumCellsInRadius(spreadRadius);


                int delay = actualFlyByTick - lastShotDelayRange.RandomInRange;

                for (int i = 0; i < burstCount; i++)
                {
                    for (int j = 0; j < singleBurstCount; j++)
                    {
                        LocalTargetInfo tg = GenRadial.RadialPattern[Rand.RangeInclusive(0, c)] + target.Cell;

                        if (target.HasThing && Rand.Chance(intendedTargetChance))
                        {
                            tg = target;
                        }

                        if (!tg.Cell.InBounds(map))
                        {
                            j--;
                            continue;
                        }
                        var index = (i * singleBurstCount + j) % projOffsets.Count;

                        if (delay > vehicleSpawnTick)
                        {
                            d.attachedDatas.Add(new AirSupportData_LaunchProjectileFromPlane()
                            {
                                projectileDef = ProjectileDef,
                                map = map,
                                target = tg,
                                triggerTick = delay,
                                triggerer = triggerer,
                                triggerFaction = triggerer.Faction,
                                origin = def.tempOriginCache,
                                soundDef = soundDef,
                                offset = projOffsets[index]
                            });
                        }
                        else
                        {
                            var ang = (target.Cell.ToVector3Shifted() - def.tempOriginCache).Yto0().AngleFlat();
                            GameComponent_CAS.AddData(new AirSupportData_LaunchProjectileOnEdge()
                            {
                                projectileDef = ProjectileDef,
                                map = map,
                                target = tg,
                                triggerTick = delay,
                                triggerer = triggerer,
                                triggerFaction = triggerer.Faction,
                                origin = def.tempOriginCache + projOffsets[index].RotatedBy(ang),
                                soundDef = soundDef,
                            });
                        }
                    }
                    delay -= burstInterval;
                }
                GameComponent_CAS.AddData(d);
            }
        }

        public override void DrawHighlight(Map map, IntVec3 callerPos, LocalTargetInfo target)
        {
            GenDraw.DrawRadiusRing(callerPos, spreadRadius, Color.white);
        }
    }
}
