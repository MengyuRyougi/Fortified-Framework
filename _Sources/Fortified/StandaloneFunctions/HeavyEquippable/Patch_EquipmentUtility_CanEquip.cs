using HarmonyLib;
using RimWorld;
using System;
using Verse;

namespace Fortified
{
    [HarmonyPatch(
        typeof(EquipmentUtility),
        nameof(EquipmentUtility.CanEquip),
        new Type[] { typeof(Thing), typeof(Pawn), typeof(string), typeof(bool) },
        new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Normal })]
    internal static class Patch_EquipmentUtility_CanEquip
    {
        static void Postfix(ref bool __result, Thing __0, Pawn __1, ref string __2, bool __3)
        {
            if (!__result) return;//因為其他原因的不可行(生物鎖之類的)
            if (__0 is not Thing thing || __1 is not Pawn pawn) return;//天知道拿了個不知道啥的鬼東西，反正也跳過。
			if (__0.def.weaponTags?.Contains("FFF_NeverEquip") == true)
            {
                __result = false;
				__2 = " " + "FFF.WeaponNotSupported".Translate();
				return;
            }
			if (!thing.HasComp<CompEquippable>()) return; //沒有comp

            if (thing.def.TryGetModExtension<HeavyEquippableExtension>(out var heavyExtension))
            {
                if (heavyExtension.CanEquippedBy(pawn))
                {
                    __result = true;
                }
                else
                {
                    // EquippableDef 為 null 時 CanEquippedBy 會直接放行，走到這裡一定有 def。
                    // 掛載型武器沒有體型門檻可報，硬報只會印出 -1 這種怪數字。
                    __2 = " " + (heavyExtension.EquippableDef.IsMountedWeapon
                        ? "FFF.MountedWeaponNotSupported".Translate()
                        : "FFF.BodysizeNotSupported".Translate(heavyExtension.EquippableDef.EquippableBaseBodySize.ToString("0.##")));
                    __result = false;
                }
            }

            if (pawn is IWeaponUsable)
            {
                if (CheckUtility.IsMechUseable(pawn, thing as ThingWithComps))
                {
                    __result = true;
                }
                else
                {
                    __2 = " " + "FFF.WeaponNotSupported".Translate();
                    __result = false;
                }
            }
        }
    }
}