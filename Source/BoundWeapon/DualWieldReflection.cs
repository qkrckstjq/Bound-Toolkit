using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Verse;
using Verse.AI;

//Dual-Wield
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
            equipOffHandJobDef = null;
        }

        static void EnsureInitialized()
        {
            if (initialized) return;
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
                byRefSecondArg: true
            );

            makeRoomForOffHandMethod = FindStaticMethod(
                dualWieldAssembly,
                "MakeRoomForOffHand",
                typeof(Pawn_EquipmentTracker),
                byRefSecondArg: false
            );

            addOffHandEquipmentMethod = FindStaticMethod(
                dualWieldAssembly,
                "AddOffHandEquipment",
                typeof(Pawn_EquipmentTracker),
                byRefSecondArg: false
            );

            equipOffHandJobDef = FindEquipOffHandJobDef();

            active = tryGetOffHandMethod != null;
        }

        static MethodInfo FindStaticMethod(Assembly asm, string name, Type firstArgType, bool byRefSecondArg)
        {
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

            foreach (var t in asm.GetTypes())
            {
                var methods = t.GetMethods(flags);
                for (int i = 0; i < methods.Length; i++)
                {
                    var m = methods[i];
                    if (m.Name != name) continue;

                    var p = m.GetParameters();
                    if (p.Length != 2) continue;
                    if (!p[0].ParameterType.IsAssignableFrom(firstArgType) &&
                        !firstArgType.IsAssignableFrom(p[0].ParameterType))
                        continue;

                    if (byRefSecondArg)
                    {
                        if (!p[1].ParameterType.IsByRef) continue;
                    }
                    else
                    {
                        if (!typeof(ThingWithComps).IsAssignableFrom(p[1].ParameterType)) continue;
                    }

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
                var def = defs[i];
                if (def == null || def.defName == null) continue;

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

            if (!active || pawn?.equipment == null)
                return false;

            object[] args = { pawn.equipment, null };
            object result = tryGetOffHandMethod.Invoke(null, args);

            if (result is not bool ok || !ok)
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

            var job = JobMaker.MakeJob(equipOffHandJobDef, weapon);
            job.ignoreForbidden = true;
            return job;
        }
    }
}