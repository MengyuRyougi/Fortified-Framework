using Verse;

namespace Fortified
{
    public interface IWeaponUsable
    {
        void Equip(ThingWithComps equipment);
        void Wear(ThingWithComps equipment);
    }
}
