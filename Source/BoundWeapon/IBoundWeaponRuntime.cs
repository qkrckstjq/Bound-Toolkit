using Verse;
using Verse.AI;

//인터페이스 추상화
namespace BoundWeapon
{
    public interface IBoundWeaponRuntime
    {
        bool IsEquipped(Pawn pawn, ThingWithComps weapon);
        bool TryEquipFromInventory(Pawn pawn, ThingWithComps weapon);
        Job TryCreateEquipJob(Pawn pawn, ThingWithComps weapon);
    }
}