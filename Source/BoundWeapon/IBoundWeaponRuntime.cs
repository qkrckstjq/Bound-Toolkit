using Verse;
using Verse.AI;

namespace BoundWeapon
{
    public interface IBoundWeaponRuntime
    {
        bool CanAssignToSlot(Pawn pawn, ThingWithComps weapon, BoundWeaponSlot slot);
        bool IsEquipped(Pawn pawn, ThingWithComps weapon, BoundWeaponSlot slot);
        bool TryEquipFromInventory(Pawn pawn, ThingWithComps weapon, BoundWeaponSlot slot);
        Job TryCreateEquipJob(Pawn pawn, ThingWithComps weapon, BoundWeaponSlot slot);
    }
}