using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace BoundWeapon
{
    public static class BoundWeaponApi
    {
        static WorldComp_BoundWeapon Comp
        {
            get
            {
                if (Find.World == null) return null;
                return Find.World.GetComponent<WorldComp_BoundWeapon>();
            }
        }

        public static bool TryGet(Pawn pawn, out ThingWithComps weapon)
        {
            weapon = null;

            if (pawn == null) return false;
            var comp = Comp;
            if (comp == null) return false;

            MethodInfo m = FindTryGet(comp.GetType());
            if (m != null)
            {
                object[] args = new object[] { pawn, null };
                object r = m.Invoke(comp, args);
                bool ok = r is bool && (bool)r;
                if (ok)
                {
                    weapon = args[1] as ThingWithComps;
                    if (weapon != null && weapon.Destroyed)
                    {
                        Clear(pawn);
                        weapon = null;
                        return false;
                    }
                    if (pawn.Dead || pawn.DestroyedOrNull())
                    {
                        Clear(pawn);
                        weapon = null;
                        return false;
                    }
                    return weapon != null;
                }
                return false;
            }

            var dict = FindPawnWeaponDict(comp);
            if (dict == null) return false;

            object v = dict[pawn];
            weapon = v as ThingWithComps;
            if (weapon == null) return false;

            if (weapon.Destroyed || pawn.Dead || pawn.DestroyedOrNull())
            {
                dict.Remove(pawn);
                weapon = null;
                return false;
            }

            return true;
        }

        public static void Set(Pawn pawn, ThingWithComps weapon)
        {
            if (pawn == null) return;

            var comp = Comp;
            if (comp == null) return;

            if (pawn.Dead || pawn.DestroyedOrNull())
            {
                Clear(pawn);
                return;
            }

            if (weapon == null || weapon.Destroyed)
            {
                Clear(pawn);
                return;
            }

            MethodInfo m = FindSet(comp.GetType());
            if (m != null)
            {
                m.Invoke(comp, new object[] { pawn, weapon });
                return;
            }

            var dict = FindPawnWeaponDict(comp);
            if (dict == null) return;

            dict[pawn] = weapon;
        }

        public static void Clear(Pawn pawn)
        {
            if (pawn == null) return;

            var comp = Comp;
            if (comp == null) return;

            MethodInfo m = FindClear(comp.GetType());
            if (m != null)
            {
                m.Invoke(comp, new object[] { pawn });
                return;
            }

            var dict = FindPawnWeaponDict(comp);
            if (dict == null) return;

            dict.Remove(pawn);
        }

        public static void ClearByWeapon(ThingWithComps weapon)
        {
            if (weapon == null) return;

            var comp = Comp;
            if (comp == null) return;

            var dict = FindPawnWeaponDict(comp);
            if (dict == null) return;

            List<Pawn> remove = null;

            foreach (DictionaryEntry e in dict)
            {
                Pawn p = e.Key as Pawn;
                ThingWithComps w = e.Value as ThingWithComps;

                if (w == null) continue;
                if (!ReferenceEquals(w, weapon)) continue;

                if (remove == null) remove = new List<Pawn>();
                remove.Add(p);
            }

            if (remove == null) return;

            for (int i = 0; i < remove.Count; i++)
                dict.Remove(remove[i]);
        }

        static MethodInfo FindTryGet(Type t)
        {
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var ms = t.GetMethods(flags);
            for (int i = 0; i < ms.Length; i++)
            {
                var m = ms[i];
                if (m.Name != "TryGet") continue;
                var p = m.GetParameters();
                if (p.Length != 2) continue;
                if (!typeof(Pawn).IsAssignableFrom(p[0].ParameterType)) continue;
                if (!p[1].ParameterType.IsByRef) continue;
                return m;
            }
            return null;
        }

        static MethodInfo FindSet(Type t)
        {
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var ms = t.GetMethods(flags);
            for (int i = 0; i < ms.Length; i++)
            {
                var m = ms[i];
                if (m.Name != "Set") continue;
                var p = m.GetParameters();
                if (p.Length != 2) continue;
                if (!typeof(Pawn).IsAssignableFrom(p[0].ParameterType)) continue;
                if (!typeof(ThingWithComps).IsAssignableFrom(p[1].ParameterType)) continue;
                return m;
            }
            return null;
        }

        static MethodInfo FindClear(Type t)
        {
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var ms = t.GetMethods(flags);
            for (int i = 0; i < ms.Length; i++)
            {
                var m = ms[i];
                if (m.Name != "Clear") continue;
                var p = m.GetParameters();
                if (p.Length != 1) continue;
                if (!typeof(Pawn).IsAssignableFrom(p[0].ParameterType)) continue;
                return m;
            }
            return null;
        }

        static IDictionary FindPawnWeaponDict(object comp)
        {
            Type t = comp.GetType();
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            foreach (var f in t.GetFields(flags))
            {
                if (f.FieldType.IsGenericType)
                {
                    var g = f.FieldType.GetGenericTypeDefinition();
                    if (g == typeof(Dictionary<,>))
                    {
                        Type[] args = f.FieldType.GetGenericArguments();
                        if (args.Length == 2 &&
                            typeof(Pawn).IsAssignableFrom(args[0]) &&
                            typeof(ThingWithComps).IsAssignableFrom(args[1]))
                        {
                            return f.GetValue(comp) as IDictionary;
                        }
                    }
                }

                if (typeof(IDictionary).IsAssignableFrom(f.FieldType))
                {
                    var v = f.GetValue(comp) as IDictionary;
                    if (v != null) return v;
                }
            }

            return null;
        }
    }
}