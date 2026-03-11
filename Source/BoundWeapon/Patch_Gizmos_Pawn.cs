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
            if (__instance == null)
                return;
            if (!__instance.RaceProps.Humanlike)
                return;
            if (!__instance.IsColonist)
                return;

            List<Gizmo> list = __result.ToList();

            ThingWithComps bound;
            if (BoundWeaponApi.TryGetAny(__instance, out bound) && bound != null)
            {
                Command_ActionWithOverlay cmd = new Command_ActionWithOverlay
                {
                    defaultLabel = "BW_ClearWeapon".Translate(),
                    icon = bound.def.uiIcon,
                    overlayTex = BW_Icons.Clear,
                    overlayColor = Color.white,
                    overlayScale = 0.45f,
                    action = delegate
                    {
                        BoundWeaponApi.ClearAll(__instance);
                        Messages.Message("BW_ClearWeaponDesc".Translate(__instance.LabelShortCap), MessageTypeDefOf.PositiveEvent, false);
                    }
                };

                list.Add(cmd);
            }

            __result = list;
        }
    }
}