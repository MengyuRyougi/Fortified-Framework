using RimWorld;
using Verse;

namespace Fortified;

[RimWorld.DefOf, StaticConstructorOnStartup]
public static class FFF_DefOf
{
    static FFF_DefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(FFF_DefOf));
    }
    public static DamageDef Stun;

    public static JobDef FFF_RepairSelf;
    public static JobDef FFF_MechLeave;
    public static JobDef FFF_EnterBunkerFacility;
    public static JobDef FFF_Modification;
    public static JobDef FFF_EjectDeactivatedMech;
    public static JobDef FFF_HackDeactivatedMech;
    public static JobDef FFF_ResurrectMech;
    public static ThingDef FFF_BandNode;
}