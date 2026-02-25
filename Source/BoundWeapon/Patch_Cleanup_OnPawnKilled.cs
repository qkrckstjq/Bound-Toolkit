using System;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace BoundWeapon
{
    [HarmonyPatch]
    public static class Patch_Cleanup_OnPawnKilled
    {
        static MethodBase TargetMethod()
        {
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var ms = typeof(Pawn).GetMethods(flags);
            for (int i = 0; i < ms.Length; i++)
            {
                var m = ms[i];
                if (m.Name != "Kill") continue;
                return m;
            }
            return null;
        }

        static void Postfix(Pawn __instance)
        {
            if (__instance == null) return;
            BoundWeaponApi.Clear(__instance);
        }
    }
}