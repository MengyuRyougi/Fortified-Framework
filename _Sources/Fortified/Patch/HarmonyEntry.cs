using Verse;
using HarmonyLib;

namespace Fortified
{
    [StaticConstructorOnStartup]
    public static class HarmonyEntry
    {
        static HarmonyEntry()
        {
            Harmony entry = new Harmony("Fortified");
            entry.PatchAll();
        }
    }
}