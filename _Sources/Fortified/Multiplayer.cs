using Multiplayer.API;
using Verse;
using System;

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
                MP.RegisterSyncWorker((SyncWorker sync, ref SubTurret turret) =>
                {
                    if (sync.isWriting)
                    {
                        if (!turret.parent.TryGetComp<CompMultipleTurretGun>(out var comp))
                            throw new Exception(
                                "Tried to sync a SubTurret without a parent that has a CompMultipleTurretGun");
                        sync.Write(comp);
                        sync.Write(turret.ID);
                    }
                    else
                    {
                        var comp = sync.Read<CompMultipleTurretGun>();
                        var turretId = sync.Read<string>();
                        turret = comp.turrets.Find(turret => turret.ID == turretId);
                    }
                });
            }
            catch (Exception ex)
            {
                Log.Warning($"Failed to initialize Multiplayer support for Fortified Framework in Multiplayer.cs: {ex.Message}");
            }
    }
}
