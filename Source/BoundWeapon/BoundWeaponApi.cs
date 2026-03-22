using Verse;

namespace BoundWeapon
{
    public static class BoundWeaponApi
    {
        static WorldComp_BoundWeapon Comp
        {
            get
            {
                if (Find.World == null)
                    return null;

                return Find.World.GetComponent<WorldComp_BoundWeapon>();
            }
        }

        public static bool TryGet(Pawn pawn, BoundWeaponSlot slot, out ThingWithComps weapon)
        {
            weapon = null;

            if (pawn == null)
                return false;

            var comp = Comp;
            if (comp == null)
                return false;

            if (pawn.Dead || pawn.DestroyedOrNull())
            {
                comp.ClearAll(pawn);
                return false;
            }

            return comp.TryGet(pawn, slot, out weapon);
        }

        public static bool TryGetPrimary(Pawn pawn, out ThingWithComps weapon)
        {
            return TryGet(pawn, BoundWeaponSlot.Primary, out weapon);
        }

        public static bool TryGetAny(Pawn pawn, out ThingWithComps weapon)
        {
            weapon = null;

            if (pawn == null)
                return false;

            var comp = Comp;
            if (comp == null)
                return false;

            if (pawn.Dead || pawn.DestroyedOrNull())
            {
                comp.ClearAll(pawn);
                return false;
            }

            return comp.TryGetAny(pawn, out weapon);
        }

        public static bool TrySet(Pawn pawn, ThingWithComps weapon, BoundWeaponSlot slot)
        {
            if (pawn == null)
                return false;

            var comp = Comp;
            if (comp == null)
                return false;

            if (pawn.Dead || pawn.DestroyedOrNull())
            {
                comp.ClearAll(pawn);
                return false;
            }

            return comp.TrySet(pawn, weapon, slot);
        }

        public static void Clear(Pawn pawn, BoundWeaponSlot slot)
        {
            if (pawn == null)
                return;

            var comp = Comp;
            if (comp == null)
                return;

            comp.Clear(pawn, slot);
        }

        public static void ClearAll(Pawn pawn)
        {
            if (pawn == null)
                return;

            var comp = Comp;
            if (comp == null)
                return;

            comp.ClearAll(pawn);
        }

        public static void ClearByWeapon(ThingWithComps weapon)
        {
            if (weapon == null)
                return;

            var comp = Comp;
            if (comp == null)
                return;

            comp.ClearByWeapon(weapon);
        }
    }
}