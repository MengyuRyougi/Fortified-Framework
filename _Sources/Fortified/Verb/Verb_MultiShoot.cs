using RimWorld;
using Verse;

namespace Fortified;
//多重射擊
public class Verb_MultiShoot : Verb_Shoot
{
    protected override bool TryCastShot()
    {
        bool result;
        CompChangeableProjectile compChangeableProjectile = EquipmentSource.GetComp<CompChangeableProjectile>();
        if (compChangeableProjectile != null)
        {
            ThingDef thingDef = null;
            if (burstShotsLeft > 1)
            {
                thingDef = compChangeableProjectile.LoadedShell;
            }
            result = base.TryCastShot();
            if (thingDef != null)
            {
                compChangeableProjectile.LoadShell(thingDef, 1);
            }
        }
        else
        {
            result = base.TryCastShot();
        }
        return result;
    }
}