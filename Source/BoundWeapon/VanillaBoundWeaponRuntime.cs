using Verse;
using Verse.AI;

//기존 바닐라 BoundWeapon 모드 구현체
namespace BoundWeapon
{
    public sealed class VanillaBoundWeaponRuntime : IBoundWeaponRuntime
    {
        public bool CanAssignToSlot(Pawn pawn, ThingWithComps weapon, BoundWeaponSlot slot)
        {
            return weapon != null && slot == BoundWeaponSlot.Primary;
        }

        public bool IsEquipped(Pawn pawn, ThingWithComps weapon, BoundWeaponSlot slot)
        {
            if (slot != BoundWeaponSlot.Primary)
                return false;

            return BoundWeaponUtil.IsPrimaryEquipped(pawn, weapon);
        }

        public bool TryEquipFromInventory(Pawn pawn, ThingWithComps weapon, BoundWeaponSlot slot)
        {
            if (slot != BoundWeaponSlot.Primary)
                return false;

            return BoundWeaponUtil.TryEquipPrimaryFromInventory(pawn, weapon);
        }

        public Job TryCreateEquipJob(Pawn pawn, ThingWithComps weapon, BoundWeaponSlot slot)
        {
            if (slot != BoundWeaponSlot.Primary)
                return null;

            return BoundWeaponUtil.TryMakePrimaryEquipJob(pawn, weapon);
        }
    }
}