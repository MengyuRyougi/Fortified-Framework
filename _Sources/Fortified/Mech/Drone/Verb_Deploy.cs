using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Fortified
{
    public class Verb_Deploy : Verb
    {
        protected override bool TryCastShot()
        {
            currentTarget = new LocalTargetInfo(Caster.Position);
            Thing thing = null;
            if (verbProps.spawnDef.race != null)
            {
                thing = PawnGenerator.GeneratePawn(DefDatabase<PawnKindDef>.GetNamed(verbProps.spawnDef.defName), Caster.Faction);
            }
            if (thing == null)
            {
                Log.Error($"Failed to spawn thing {verbProps.spawnDef.defName} at {currentTarget.Cell} for verb {verbProps.label}.");
                return false;
            }
            GenSpawn.Spawn(thing, currentTarget.Cell, caster.Map);
            if (thing.TryGetComp<CompDrone>(out var d))
            {
                d.SetPlatform(this.VerbOwner_ChargedCompSource.parent);
            }
            if (verbProps.colonyWideTaleDef != null)
            {
                Pawn pawn = caster.Map.mapPawns.FreeColonistsSpawned.RandomElementWithFallback();
                TaleRecorder.RecordTale(verbProps.colonyWideTaleDef, caster, pawn);
            }

            base.ReloadableCompSource?.UsedOnce();
            return true;
        }
    }
}