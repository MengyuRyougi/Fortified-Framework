using Verse;

namespace Fortified
{
    public interface IPawnCapacity
    {
        bool TryAcceptThing(Thing thing);
        public bool HasPawn(out Pawn pawn);
    }
}
