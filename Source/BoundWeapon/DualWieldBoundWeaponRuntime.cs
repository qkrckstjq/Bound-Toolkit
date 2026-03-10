using Verse;
using Verse.AI;

//Dual-Wield 모드 구현체
namespace BoundWeapon
{
    public sealed class DualWieldBoundWeaponRuntime : IBoundWeaponRuntime
    {
        readonly VanillaBoundWeaponRuntime vanilla = new VanillaBoundWeaponRuntime();

        public bool IsEquipped(Pawn pawn, ThingWithComps weapon)
        {
            if (vanilla.IsEquipped(pawn, weapon))
                return true;

            return DualWieldReflection.TryGetOffHand(pawn, out var offHand) &&
                   ReferenceEquals(offHand, weapon);
        }

        public bool TryEquipFromInventory(Pawn pawn, ThingWithComps weapon)
        {
            if (pawn?.equipment?.Primary != null)
            {
                if (DualWieldReflection.TryEquipOffHandFromInventory(pawn, weapon))
                    return true;
            }

            return vanilla.TryEquipFromInventory(pawn, weapon);
        }

        public Job TryCreateEquipJob(Pawn pawn, ThingWithComps weapon)
        {
            if (pawn?.equipment?.Primary != null)
            {
                var offHandJob = DualWieldReflection.TryMakeOffHandEquipJob(pawn, weapon);
                if (offHandJob != null)
                    return offHandJob;
            }

            return vanilla.TryCreateEquipJob(pawn, weapon);
        }
    }
}