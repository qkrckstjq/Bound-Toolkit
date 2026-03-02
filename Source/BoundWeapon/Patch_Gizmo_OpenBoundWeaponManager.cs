using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace BoundWeapon
{
    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    [HarmonyPriority(Priority.Last)]
    public static class Patch_Gizmo_OpenBoundWeaponManager
    {
        static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn __instance)
        {
            foreach (var g in __result) yield return g;

            if (__instance == null) yield break;
            if (__instance.Map == null) yield break;
            if (!__instance.RaceProps.Humanlike) yield break;    //µ¿¹°,¸ÞÄ«Á¦¿Ü
            if (!__instance.IsColonist) yield break;             //Á¤Âø¹Î¸¸

            yield return new Command_Action
            {
                defaultLabel = "BW_WeaponManagement".Translate(),
                icon = BW_Icons.Manager,
                action = () => Find.WindowStack.Add(new Window_BoundWeaponManager(__instance.Map))
            };
        }
    }
}