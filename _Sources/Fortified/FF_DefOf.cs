using RimWorld;
using Verse;

namespace Fortified;

[DefOf]
public static class DMS_DefOf
{
    static DMS_DefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(DMS_DefOf));
    }
    public static DamageDef Stun;
    public static ThingDef Neurocomputer;
    public static ThoughtDef DMS_OverEat;
    public static JobDef DMS_RepairSelf;
    public static JobDef DMS_MechLeave;
    public static ThingSetMakerDef DMS_OutgoingLoots;
    public static RulePackDef DMS_Outgoing_Attack;
    public static RulePackDef DMS_Outgoing_Loot;
    public static JobDef EnterBunkerFacility;
    public static JobDef DMS_Modification;
}