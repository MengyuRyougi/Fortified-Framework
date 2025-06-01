using HarmonyLib;
using Verse;

namespace Fortified
{
    [HarmonyPatch(typeof(Verb_Shoot), "TryCastShot", MethodType.Normal)]
    internal static class Patch_Verb_Shoot_CompCastPushHeat
    {
        public static void Postfix(ref bool __result, Verb_Shoot __instance)
        {
            if (!__result) return;

            ThingWithComps comps = __instance.EquipmentSource;
            if (comps == null) return;
            if (__instance.EquipmentSource?.GetComp<CompCastPushHeat>() is CompCastPushHeat compCastPushHeat)            //射擊加溫
            {
                if (compCastPushHeat.EnergyPerCast != 0)
                {
                    GenTemperature.PushHeat(__instance.Caster.Position, __instance.Caster.Map, compCastPushHeat.EnergyPerCast);
                }
            }
            if (__instance.EquipmentSource?.GetComps<CompCastFlecker>().EnumerableCount() != 0)            //發射特效
            {
                foreach (var comp in __instance.EquipmentSource.GetComps<CompCastFlecker>())
                {
                    if (!comp.SpawnCheck(__instance.Caster)) break;
                    comp.DoBursting(Vector3Utility.AngleToFlat(__instance.Caster.DrawPos, __instance.CurrentTarget.Cell.ToVector3Shifted()));
                }
            }
        }
    }
}