using Verse;

namespace BoundWeapon
{
    public class BoundWeaponLoadout : IExposable
    {
        public ThingWithComps primary;
        public ThingWithComps offHand;

        public void ExposeData()
        {
            Scribe_References.Look(ref primary, "primary");
            Scribe_References.Look(ref offHand, "offHand");
        }

        public ThingWithComps Get(BoundWeaponSlot slot)
        {
            return slot == BoundWeaponSlot.Primary ? primary : offHand;
        }

        public void Set(BoundWeaponSlot slot, ThingWithComps weapon)
        {
            if (slot == BoundWeaponSlot.Primary)
                primary = weapon;
            else
                offHand = weapon;
        }

        public void Clear(BoundWeaponSlot slot)
        {
            if (slot == BoundWeaponSlot.Primary)
                primary = null;
            else
                offHand = null;
        }

        public bool HasAny()
        {
            return primary != null || offHand != null;
        }

        public void CleanupDestroyed()
        {
            if (primary != null && primary.Destroyed)
                primary = null;

            if (offHand != null && offHand.Destroyed)
                offHand = null;
        }
    }
}