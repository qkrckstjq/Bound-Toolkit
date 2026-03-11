using System;
using System.Linq;
using System.Reflection;
using Verse;
using Verse.AI;

namespace BoundWeapon
{
    public static class DualWieldReflection
    {
        const string PackageId = "MemeGoddess.DualWield";

        static bool initialized;
        static bool active;
        static Assembly dualWieldAssembly;

        static MethodInfo tryGetOffHandMethod;
        static MethodInfo makeRoomForOffHandMethod;
        static MethodInfo addOffHandEquipmentMethod;

        static MethodInfo canBeOffHandMethod;
        static MethodInfo isTwoHandMethod;

        static JobDef equipOffHandJobDef;

        public static bool Active
        {
            get
            {
                EnsureInitialized();
                return active;
            }
        }

        public static void Reset()
        {
            initialized = false;
            active = false;
            dualWieldAssembly = null;

            tryGetOffHandMethod = null;
            makeRoomForOffHandMethod = null;
            addOffHandEquipmentMethod = null;

            canBeOffHandMethod = null;
            isTwoHandMethod = null;

            equipOffHandJobDef = null;
        }

        static void EnsureInitialized()
        {
            if (initialized)
                return;

            initialized = true;

            if (!ModsConfig.IsActive(PackageId))
                return;

            dualWieldAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "DualWield");

            if (dualWieldAssembly == null)
                return;

            tryGetOffHandMethod = FindStaticMethod(
                dualWieldAssembly,
                "TryGetOffHandEquipment",
                typeof(Pawn_EquipmentTracker),
                true
            );

            makeRoomForOffHandMethod = FindStaticMethod(
                dualWieldAssembly,
                "MakeRoomForOffHand",
                typeof(Pawn_EquipmentTracker),
                false
            );

            addOffHandEquipmentMethod = FindStaticMethod(
                dualWieldAssembly,
                "AddOffHandEquipment",
                typeof(Pawn_EquipmentTracker),
                false
            );

            canBeOffHandMethod = FindThingDefBoolMethod(dualWieldAssembly, "CanBeOffHand");
            isTwoHandMethod = FindThingDefBoolMethod(dualWieldAssembly, "IsTwoHand");

            equipOffHandJobDef = FindEquipOffHandJobDef();

            active = tryGetOffHandMethod != null;
        }

        static MethodInfo FindStaticMethod(Assembly asm, string name, Type firstArgType, bool byRefSecondArg)
        {
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            Type[] types = asm.GetTypes();

            for (int i = 0; i < types.Length; i++)
            {
                MethodInfo[] methods = types[i].GetMethods(flags);
                for (int j = 0; j < methods.Length; j++)
                {
                    MethodInfo m = methods[j];
                    if (m.Name != name)
                        continue;

                    ParameterInfo[] p = m.GetParameters();
                    if (p.Length != 2)
                        continue;

                    if (!p[0].ParameterType.IsAssignableFrom(firstArgType) &&
                        !firstArgType.IsAssignableFrom(p[0].ParameterType))
                        continue;

                    if (byRefSecondArg)
                    {
                        if (!p[1].ParameterType.IsByRef)
                            continue;
                    }
                    else
                    {
                        if (!typeof(ThingWithComps).IsAssignableFrom(p[1].ParameterType))
                            continue;
                    }

                    return m;
                }
            }

            return null;
        }

        static MethodInfo FindThingDefBoolMethod(Assembly asm, string name)
        {
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            Type[] types = asm.GetTypes();

            for (int i = 0; i < types.Length; i++)
            {
                MethodInfo[] methods = types[i].GetMethods(flags);
                for (int j = 0; j < methods.Length; j++)
                {
                    MethodInfo m = methods[j];
                    if (m.Name != name)
                        continue;

                    ParameterInfo[] p = m.GetParameters();
                    if (p.Length != 1)
                        continue;

                    if (p[0].ParameterType != typeof(ThingDef))
                        continue;

                    if (m.ReturnType != typeof(bool))
                        continue;

                    return m;
                }
            }

            return null;
        }

        static JobDef FindEquipOffHandJobDef()
        {
            string[] candidates =
            {
                "EquipOffHand",
                "EquipOffhand",
                "DW_EquipOffHand",
                "DW_EquipOffhand"
            };

            var defs = DefDatabase<JobDef>.AllDefsListForReading;
            for (int i = 0; i < defs.Count; i++)
            {
                JobDef def = defs[i];
                if (def == null || def.defName == null)
                    continue;

                for (int j = 0; j < candidates.Length; j++)
                {
                    if (def.defName == candidates[j])
                        return def;
                }
            }

            return null;
        }

        public static bool TryGetOffHand(Pawn pawn, out ThingWithComps weapon)
        {
            weapon = null;
            EnsureInitialized();

            if (!active || pawn == null || pawn.equipment == null)
                return false;

            object[] args = new object[] { pawn.equipment, null };
            object result = tryGetOffHandMethod.Invoke(null, args);

            bool ok = result is bool && (bool)result;
            if (!ok)
                return false;

            weapon = args[1] as ThingWithComps;
            return weapon != null && !weapon.Destroyed;
        }

        public static bool TryEquipOffHandFromInventory(Pawn pawn, ThingWithComps weapon)
        {
            EnsureInitialized();

            if (!active || pawn == null || weapon == null)
                return false;

            if (pawn.equipment == null || pawn.inventory == null)
                return false;

            if (!pawn.inventory.innerContainer.Contains(weapon))
                return false;

            if (makeRoomForOffHandMethod == null || addOffHandEquipmentMethod == null)
                return false;

            makeRoomForOffHandMethod.Invoke(null, new object[] { pawn.equipment, weapon });
            pawn.inventory.innerContainer.Remove(weapon);
            addOffHandEquipmentMethod.Invoke(null, new object[] { pawn.equipment, weapon });

            return true;
        }

        public static Job TryMakeOffHandEquipJob(Pawn pawn, ThingWithComps weapon)
        {
            EnsureInitialized();

            if (!active || equipOffHandJobDef == null || pawn == null || weapon == null)
                return null;

            Job job = JobMaker.MakeJob(equipOffHandJobDef, weapon);
            job.ignoreForbidden = true;
            return job;
        }

        public static bool CanAssignToOffHand(ThingWithComps weapon)
        {
            EnsureInitialized();

            if (!active || weapon == null || weapon.def == null || canBeOffHandMethod == null)
                return false;

            object result = canBeOffHandMethod.Invoke(null, new object[] { weapon.def });
            return result is bool && (bool)result;
        }

        public static bool IsTwoHanded(ThingWithComps weapon)
        {
            EnsureInitialized();

            if (!active || weapon == null || weapon.def == null || isTwoHandMethod == null)
                return false;

            object result = isTwoHandMethod.Invoke(null, new object[] { weapon.def });
            return result is bool && (bool)result;
        }
    }
}