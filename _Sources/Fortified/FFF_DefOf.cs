// 定义框架引用
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Fortified;

[RimWorld.DefOf]
public static class FFF_DefOf
{
    static FFF_DefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(FFF_DefOf));
    }
    public static JobDef FFF_RepairSelf;
    public static JobDef FFF_MechLeave;
    public static JobDef FFF_EnterBunkerFacility;
    public static JobDef FFF_Modification;
    public static JobDef FFF_ModificationRemove;
    public static JobDef FFF_EjectDeactivatedMech;
    public static JobDef FFF_HackDeactivatedMech;
    public static JobDef FFF_ResurrectMech;
    public static JobDef FFF_HackMechCapsule;
    public static JobDef FFF_EjectMechCapsule;
    public static JobDef FFF_Replenish;
    public static JobDef FFF_SwitchAmmo;
    public static JobDef FFF_EquipTurret;
    public static JobDef FFF_EquipDeployable;
    public static JobDef FFF_UseAccessKey;
    public static JobDef FFF_RepairMech_Overseer;
	public static JobDef FFF_ControlMech_Overseer;
    public static JobDef FFF_Recycle;

    // 原地拆解（CompRecycleable）
    public static DesignationDef FFF_RecycleDesignation;
    public static WorkGiverDef FFF_RecycleWorkGiver;

	public static StatCategoryDef FFF_Turrets;
	public static StatCategoryDef FFF_Mechanics;

	// 訊息卡特殊機制條目
	public static FFF_InfoDef FFF_Info_AmmoSwitch;
	public static FFF_InfoDef FFF_Info_DamageBlocker;
	public static FFF_InfoDef FFF_Info_EnvironmentalBill;
	public static FFF_InfoDef FFF_Info_OverseerMech;
	public static FFF_InfoDef FFF_Info_OverseerBuilding;

	public static DutyDef FFF_DefendRoom;

    public static MentalStateDef FFF_FleeInPlace;

	public static HediffDef FFF_Camouflage;
    public static HediffDef FFF_DummyHediff;

	public static PawnKindDef FFF_Dummy;

	public static FleckDef FFF_Fleck_DeflectShell;

    public static StatDef FFF_FearResistance;
    [MayRequireBiotech]
    public static StatDef FFF_MechCommandRange;
}

[StaticConstructorOnStartup]
public static class FFF_Icons
{
    public static Texture2D icon_Cancel = ContentFinder<Texture2D>.Get("UI/Designators/Cancel");
}
