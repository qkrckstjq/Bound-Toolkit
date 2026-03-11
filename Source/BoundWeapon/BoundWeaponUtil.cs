using RimWorld;
using Verse;
using Verse.AI;

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
            if (twc.def.stackLimit > 1)
                return false;
            return true;
        }

        public static bool NeedsEquip(Pawn pawn, ThingWithComps weapon, BoundWeaponSlot slot)
        {
            if (pawn == null || weapon == null)
                return false;

            return !BoundWeaponRuntimeProvider.Current.IsEquipped(pawn, weapon, slot);
        }

        public static bool IsPrimaryEquipped(Pawn pawn, ThingWithComps weapon)
        {
            if (pawn == null || pawn.equipment == null || weapon == null)
                return false;

            return ReferenceEquals(pawn.equipment.Primary, weapon);
        }

        public static bool TryEquipPrimaryFromInventory(Pawn pawn, ThingWithComps weapon)
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

        public static bool TryEquipAssignedWeaponFromInventory(Pawn pawn, ThingWithComps weapon, BoundWeaponSlot slot)
        {
            return BoundWeaponRuntimeProvider.Current.TryEquipFromInventory(pawn, weapon, slot);
        }

        public static Job TryMakePrimaryEquipJob(Pawn pawn, ThingWithComps weapon)
        {
            if (pawn == null || weapon == null)
                return null;

            if (!pawn.Spawned)
                return null;

            if (!weapon.Spawned || weapon.Map != pawn.Map)
                return null;

            if (!pawn.CanReserveAndReach(weapon, PathEndMode.Touch, Danger.Deadly))
                return null;

            Job job = JobMaker.MakeJob(JobDefOf.Equip, weapon);
            job.ignoreForbidden = true;
            return job;
        }

        public static Job TryMakeAssignedWeaponEquipJob(Pawn pawn, ThingWithComps weapon, BoundWeaponSlot slot)
        {
            return BoundWeaponRuntimeProvider.Current.TryCreateEquipJob(pawn, weapon, slot);
        }

        public static string SlotLabel(BoundWeaponSlot slot)
        {
            return slot == BoundWeaponSlot.Primary
                ? "BW_SlotPrimary".Translate().ToString()
                : "BW_SlotOffHand".Translate().ToString();
        }
    }
}