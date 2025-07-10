using HarmonyLib;
using System.Reflection;
using Verse;

namespace FortifiedCE
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            Harmony harmony = new Harmony("FortificationCE");

            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}