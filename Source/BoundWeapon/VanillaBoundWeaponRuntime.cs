using Verse;
using Verse.AI;

//기존 바닐라 BoundWeapon 모드 구현체

namespace BoundWeapon
{
    public sealed class VanillaBoundWeaponRuntime : IBoundWeaponRuntime
    {
        public bool IsEquipped(Pawn pawn, ThingWithComps weapon)
        {
            return BoundWeaponUtil.IsPrimaryEquipped(pawn, weapon);
        }

        public bool TryEquipFromInventory(Pawn pawn, ThingWithComps weapon)
        {
            return BoundWeaponUtil.TryEquipPrimaryFromInventory(pawn, weapon);
        }

        public Job TryCreateEquipJob(Pawn pawn, ThingWithComps weapon)
        {
            return BoundWeaponUtil.TryMakePrimaryEquipJob(pawn, weapon);
        }
    }
}