using System;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace BoundWeapon
{
    [HarmonyPatch]
    public static class Patch_Cleanup_OnWeaponDestroyed
    {
        static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(Thing), "Destroy", new Type[] { typeof(DestroyMode) });
        }

        static void Postfix(Thing __instance)
        {
            if (__instance == null) return;
            if (!BoundWeaponUtil.IsValidWeapon(__instance)) return;

            ThingWithComps w = __instance as ThingWithComps;
            if (w == null) return;

            BoundWeaponApi.ClearByWeapon(w);
        }
    }
}