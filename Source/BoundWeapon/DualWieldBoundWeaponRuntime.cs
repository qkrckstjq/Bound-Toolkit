using RimWorld;
using Verse;
using Verse.AI;

namespace BoundWeapon
{
    public sealed class DualWieldBoundWeaponRuntime : IBoundWeaponRuntime
    {
        readonly VanillaBoundWeaponRuntime vanilla = new VanillaBoundWeaponRuntime();

        public bool CanAssignToSlot(Pawn pawn, ThingWithComps weapon, BoundWeaponSlot slot)
        {
            if (weapon == null)
                return false;

            if (slot == BoundWeaponSlot.Primary)
                return true;

            if (!DualWieldReflection.Active)
                return false;

            if (!DualWieldReflection.CanAssignToOffHand(weapon))
                return false;

            if (DualWieldReflection.IsTwoHanded(weapon))
                return false;

            ThingWithComps assignedPrimary;
            if (pawn != null && BoundWeaponApi.TryGetPrimary(pawn, out assignedPrimary) && assignedPrimary != null)
            {
                if (DualWieldReflection.IsTwoHanded(assignedPrimary))
                    return false;
            }

            if (pawn != null && pawn.equipment != null && pawn.equipment.Primary != null)
            {
                if (DualWieldReflection.IsTwoHanded(pawn.equipment.Primary))
                    return false;
            }

            return true;
        }

        public bool IsEquipped(Pawn pawn, ThingWithComps weapon, BoundWeaponSlot slot)
        {
            if (slot == BoundWeaponSlot.Primary)
                return vanilla.IsEquipped(pawn, weapon, slot);

            ThingWithComps offHand;
            return DualWieldReflection.TryGetOffHand(pawn, out offHand) &&
                   ReferenceEquals(offHand, weapon);
        }

        public bool TryEquipFromInventory(Pawn pawn, ThingWithComps weapon, BoundWeaponSlot slot)
        {
            if (slot == BoundWeaponSlot.Primary)
                return vanilla.TryEquipFromInventory(pawn, weapon, slot);

            if (pawn == null || pawn.equipment == null || pawn.equipment.Primary == null)
                return false;

            if (!CanAssignToSlot(pawn, weapon, slot))
                return false;

            if (pawn.health != null &&
                pawn.health.capacities != null &&
                !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                return false;

            return DualWieldReflection.TryEquipOffHandFromInventory(pawn, weapon);
        }

        public Job TryCreateEquipJob(Pawn pawn, ThingWithComps weapon, BoundWeaponSlot slot)
        {
            if (slot == BoundWeaponSlot.Primary)
                return vanilla.TryCreateEquipJob(pawn, weapon, slot);

            if (pawn == null || pawn.equipment == null || pawn.equipment.Primary == null)
                return null;

            if (!CanAssignToSlot(pawn, weapon, slot))
                return null;

            if (pawn.health != null &&
                pawn.health.capacities != null &&
                !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                return null;

            return DualWieldReflection.TryMakeOffHandEquipJob(pawn, weapon);
        }
    }
}