using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Fortified;

/// <summary>
/// 掃描地圖上帶有 FFF_Recycle 標記的 pawn／屍體，指派 JobDriver_Recycle。
/// 直接右鍵下令的路徑由 FloatMenuOptionProvider_Recycle 負責，這裡只處理自動工作。
/// </summary>
public class WorkGiver_Recycle : WorkGiver_Scanner
{
    public override PathEndMode PathEndMode => PathEndMode.Touch;

    public override Danger MaxPathDanger(Pawn pawn) => Danger.Deadly;

    public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
    {
        foreach (Designation d in pawn.Map.designationManager.SpawnedDesignationsOfDef(FFF_DefOf.FFF_RecycleDesignation))
        {
            if (d.target.HasThing)
            {
                yield return d.target.Thing;
            }
        }
    }

    public override bool ShouldSkip(Pawn pawn, bool forced = false)
    {
        return !pawn.Map.designationManager.AnySpawnedDesignationOfDef(FFF_DefOf.FFF_RecycleDesignation);
    }

    public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        CompRecycleable comp = CompRecycleable.Get(t);
        if (comp == null || !comp.Designated) return false;
        if (!comp.CanRecycle().Accepted) return false;
        if (t.IsForbidden(pawn) || t.IsBurning()) return false;
        if (!pawn.CanReserve(t, 1, -1, null, forced)) return false;
        return true;
    }

    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        return JobMaker.MakeJob(FFF_DefOf.FFF_Recycle, t);
    }
}
