using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;

namespace Fortified;
public class CompProperties_ShieldingDevice : CompProperties
{
    public List<IncidentDef> incidentsWhiteList;
    public int coolDownTicks = 60000;

    public CompProperties_ShieldingDevice()
    {
        compClass = typeof(CompSignalTower);
    }
}
public class CompShieldingDevice : ThingComp
{
    private CompPowerTrader compPowerInt;

    public static List<CompShieldingDevice> cached = new List<CompShieldingDevice>();

    public CompProperties_ShieldingDevice Props => (CompProperties_ShieldingDevice)props;

    public CompPowerTrader CompPower
    {
        get
        {
            compPowerInt ??= parent.TryGetComp<CompPowerTrader>();
            return compPowerInt;
        }
    }
    public bool HasPower
    {
        get
        {
            return CompPower == null || CompPower.PowerOn;
        }
    }

    private int remainingCoolDownTicks = 0;
    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        cached.Add(this);
    }

    public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
    {
        base.PostDeSpawn(map, mode);
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
    public bool Active()
    {
        return HasPower && remainingCoolDownTicks < 1;
    }

    public static IEnumerable<Thing> GetTowerByIncident(IncidentDef def)
    {
        if (cached.NullOrEmpty())
        {
            yield break;
        }
        IEnumerable<CompShieldingDevice> enumerable = cached.Where((CompShieldingDevice x) => x.Props.incidentsWhiteList.Contains(def) && x.Active());
        if (enumerable.EnumerableNullOrEmpty())
        {
            yield break;
        }
        foreach (CompShieldingDevice item in enumerable)
        {
            yield return item.parent;
        }
    }
}