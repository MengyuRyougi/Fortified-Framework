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
    public static ThingSetMakerDef FFF_OutgoingLoots;
    public static RulePackDef FFF_Outgoing_Attack;
    public static RulePackDef FFF_Outgoing_Loot;
    public static JobDef FFF_EnterBunkerFacility;
    public static JobDef FFF_Modification;
    public static HediffDef FFF_StructuralDamage;
    public static JobDef FFF_EjectDeactivatedMech;
    public static JobDef FFF_HackDeactivatedMech;
    public static JobDef FFF_ResurrectMech;
    public static ThingDef FFF_BandNode;
}