//dual-wield모드 감지 후 구현체 결정

namespace BoundWeapon
{
    public static class BoundWeaponRuntimeProvider
    {
        static IBoundWeaponRuntime current;
        static bool resolved;

        public static IBoundWeaponRuntime Current
        {
            get
            {
                if (!resolved)
                {
                    resolved = true;
                    current = DualWieldReflection.Active
                        ? (IBoundWeaponRuntime)new DualWieldBoundWeaponRuntime()
                        : new VanillaBoundWeaponRuntime();
                }

                return current;
            }
        }

        public static void Reset()
        {
            resolved = false;
            current = null;
            DualWieldReflection.Reset();
        }
    }
}