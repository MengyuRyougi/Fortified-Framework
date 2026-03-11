using Multiplayer.API;


namespace Fortified.MultiplayerCompatibility;


[StaticConstructorOnStartup]
public static class Multiplayer
{
    static Multiplayer()
    {
            try
            {
                if (!MP.enabled) return;
                MP.RegisterAll();
            }
            catch (Exception ex)
            {
                Log.Warning($"Failed to initialize Multiplayer support for Fortified Framework in Multiplayer.cs: {ex.Message}");
            }
    }
}
