﻿using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace Fortified
{
    public static class OrbitUtility
    {
        public static AcceptanceReport InMicroGravity(Thing thing)
        {
            Log.Message("Checking microgravity for " + thing.Label.ToString());

            if (thing?.Map == null) return false;
            if (!thing.Spawned) return false;

            if (!ModsConfig.OdysseyActive) Log.WarningOnce($"Warning, {thing} checking Gravity without OdysseyActive.", 123457);
            if (thing.Map.TileInfo.Layer.Def == PlanetLayerDefOf.Surface)
            {
                return "FFF.Cannot.TableNotInMicroGravity".Translate();
            } 
            return true;
        }
        public static AcceptanceReport InVacuum(Thing thing)
        {
            Log.Message("Checking vacuum for " + thing.Label.ToString());

            if (thing?.Map == null) return false;
            if (!thing.Spawned) return false;

            if (!ModsConfig.OdysseyActive) Log.WarningOnce($"Warning, {thing} checking Vacuum without OdysseyActive.", 123457);
            if (thing.Position.GetVacuum(thing.Map) <0.25f) return "FFF.Cannot.TableNotInVacuum".Translate();
            return true;
        }
    }
}