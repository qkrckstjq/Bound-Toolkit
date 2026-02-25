using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace BoundWeapon
{
    public class BoundWeaponMod : Mod
    {
        public BoundWeaponMod(ModContentPack content) : base(content)
        {
            Log.Message("[BW-DIAG] Mod ctor start");

            DumpType(typeof(FloatMenuMakerMap), "FloatMenuMakerMap");

            Type ctx = AccessTools.TypeByName("RimWorld.FloatMenuContext");
            Type ctxByRef = AccessTools.TypeByName("RimWorld.FloatMenuContextByRef");

            DumpType(ctx, "RimWorld.FloatMenuContext");
            DumpType(ctxByRef, "RimWorld.FloatMenuContextByRef");

            DumpType(typeof(WindowStack), "Verse.WindowStack");
            DumpType(typeof(FloatMenu), "Verse.FloatMenu");

            var h = new Harmony("BoundWeapon.DIAG");
            h.PatchAll(Assembly.GetExecutingAssembly());

            Log.Message("[BW-DIAG] PatchAll done");
        }

        static void DumpType(Type t, string name)
        {
            if (t == null)
            {
                Log.Message("[BW-DIAG] Type " + name + " = null");
                return;
            }

            Log.Message("[BW-DIAG] Type " + name + " = " + t.FullName + " asm=" + t.Assembly.GetName().Name);

            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

            var fields = t.GetFields(flags);
            Log.Message("[BW-DIAG] Fields(" + name + ")=" + fields.Length);
            for (int i = 0; i < fields.Length; i++)
            {
                var f = fields[i];
                Log.Message("[BW-DIAG]  F " + f.FieldType.FullName + " " + f.Name);
            }

            var props = t.GetProperties(flags);
            Log.Message("[BW-DIAG] Props(" + name + ")=" + props.Length);
            for (int i = 0; i < props.Length; i++)
            {
                var p = props[i];
                Log.Message("[BW-DIAG]  P " + p.PropertyType.FullName + " " + p.Name);
            }

            var methods = t.GetMethods(flags);
            Log.Message("[BW-DIAG] Methods(" + name + ")=" + methods.Length);
            for (int i = 0; i < methods.Length; i++)
            {
                var m = methods[i];
                if (m.IsSpecialName) continue;
                Log.Message("[BW-DIAG]  M " + m);
            }
        }
    }
}