using Verse;
using RimWorld;
using System.Reflection;
using HarmonyLib;

namespace Fortified
{
    [HarmonyPatch(typeof(CompBiocodable), "CodeFor")]
    internal static class Patch_Biocode
    {
        public static bool Prefix(Pawn p, CompBiocodable __instance)
        {
            if (p.Name == null && __instance.Biocodable)
            {
                typeof(CompBiocodable).GetField("biocoded", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, true);
                typeof(CompBiocodable).GetField("codedPawn", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, p);
                typeof(CompBiocodable).GetField("codedPawnLabel", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, p.Label);
                return false;
            }
            return true;
        }
    }
}
