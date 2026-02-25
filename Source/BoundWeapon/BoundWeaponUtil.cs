using RimWorld;
using Verse;

namespace BoundWeapon
{
    public static class BoundWeaponUtil
    {
        public static bool IsValidWeapon(Thing t)
        {
            ThingWithComps twc = t as ThingWithComps;
            if (twc == null)
                return false;
            if (twc.Destroyed)
                return false;
            if (!twc.def.IsWeapon)
                return false;
            if (twc.TryGetComp<CompEquippable>() == null)
                return false;
            return true;
        }

        public static bool NeedsEquip(Pawn pawn, ThingWithComps weapon)
        {
            if (pawn == null || pawn.equipment == null)
                return false;
            return pawn.equipment.Primary != weapon;
        }

        public static bool TryEquipFromInventory(Pawn pawn, ThingWithComps weapon)
        {
            if (pawn == null || pawn.inventory == null || pawn.equipment == null)
                return false;

            if (!pawn.inventory.innerContainer.Contains(weapon))
                return false;

            if (!pawn.Spawned)
                return false;

            pawn.equipment.MakeRoomFor(weapon);
            pawn.inventory.innerContainer.Remove(weapon);
            pawn.equipment.AddEquipment(weapon);
            pawn.equipment.Notify_EquipmentAdded(weapon);
            return true;
        }
    }
}