using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace BoundWeapon
{
    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    public static class Patch_Gizmos_Pawn
    {
        public static void Postfix(Pawn __instance, ref IEnumerable<Gizmo> __result)
        {
            if (__instance == null) return;
            if (!__instance.RaceProps.Humanlike) return;
            if (!__instance.IsColonist) return;

            var list = __result.ToList();

            if (WorldComp_BoundWeapon.Instance.TryGet(__instance, out var bound) && bound != null)
            {
                var cmd = new Command_ActionWithOverlay
                {
                    defaultLabel = "BW_ClearWeapon".Translate(),
                    icon = bound.def.uiIcon,
                    overlayTex = BW_Icons.Clear,
                    overlayColor = Color.white,
                    overlayScale = 0.45f,
                    action = () =>
                    {
                        WorldComp_BoundWeapon.Instance.Clear(__instance);
                        Messages.Message("BW_ClearWeaponDesc".Translate(__instance.LabelShortCap), MessageTypeDefOf.PositiveEvent, false);
                    }
                };

                list.Add(cmd);
            }

            __result = list;
        }
    }
}