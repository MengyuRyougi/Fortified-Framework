using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using UnityEngine;
using System.Reflection;

namespace Fortified;
public class CompProperties_SignalTower : CompProperties
{
    public List<IncidentDef> incidentsWhiteList;

    public CompProperties_SignalTower()
    {
        compClass = typeof(CompSignalTower);
    }
}

public class CompSignalTower : ThingComp
{
    private CompPowerTrader compPowerInt;

    public static List<CompSignalTower> cached = new List<CompSignalTower>();

    public CompProperties_SignalTower Props => (CompProperties_SignalTower)props;

    public CompPowerTrader CompPower
    {
        get
        {
            if (compPowerInt == null)
            {
                compPowerInt = parent.TryGetComp<CompPowerTrader>();
            }
            return compPowerInt;
        }
    }

    public bool HasPower
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

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        cached.Add(this);
    }

    public override void PostDeSpawn(Map map)
    {
        base.PostDeSpawn(map);
        cached.Remove(this);
    }

    public override void PostDestroy(DestroyMode mode, Map previousMap)
    {
        base.PostDestroy(mode, previousMap);
        cached.Remove(this);
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        if (Scribe.mode == LoadSaveMode.LoadingVars)
        {
            cached.Clear();
        }
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            cached.Add(this);
        }
    }

    public static IEnumerable<Thing> GetTowerByIncident(IncidentDef def)
    {
        if (cached.NullOrEmpty())
        {
            yield break;
        }
        IEnumerable<CompSignalTower> enumerable = cached.Where((CompSignalTower x) => x.Props.incidentsWhiteList.Contains(def) && x.HasPower);
        if (enumerable.EnumerableNullOrEmpty())
        {
            yield break;
        }
        foreach (CompSignalTower item in enumerable)
        {
            yield return item.parent;
        }
    }
}
