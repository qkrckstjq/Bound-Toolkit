using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace BoundWeapon
{
    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    public static class Patch_Gizmos_Pawn
    {
        public static void Postfix(Pawn __instance, ref IEnumerable<Gizmo> __result)
        {
            if (__instance == null) return;                 
            if (!__instance.RaceProps.Humanlike) return;    //µ¿¹°,¸ÞÄ«Á¦¿Ü
            if (!__instance.IsColonist) return;             //Á¤Âø¹Î¸¸
            //if (__instance == null || __instance.Faction != Faction.OfPlayer)
            //    return;

            var list = __result.ToList();

            //if (__instance.equipment?.Primary != null && BoundWeaponUtil.IsValidWeapon(__instance.equipment.Primary))
            //{
            //    list.Add(new Command_Action
            //    {
            //        defaultLabel = "BW_DesignateCurrent".Translate(),
            //        icon = BW_Icons.Bind,
            //        action = () =>
            //        {
            //            var w = __instance.equipment.Primary;
            //            WorldComp_BoundWeapon.Instance.Set(__instance, w);
            //            Messages.Message("BW_BindWeaponDesc".Translate(__instance.LabelShortCap, w.LabelCap), MessageTypeDefOf.PositiveEvent, false);
            //        }
            //    });
            //}

            if (WorldComp_BoundWeapon.Instance.TryGet(__instance, out var bound) && bound != null)
            {
                list.Add(new Command_Action
                {
                    defaultLabel = "BW_ClearWeapon".Translate(),
                    icon = BW_Icons.Clear,
                    action = () =>
                    {
                        WorldComp_BoundWeapon.Instance.Clear(__instance);
                        Messages.Message("BW_ClearWeaponDesc".Translate(__instance.LabelShortCap), MessageTypeDefOf.PositiveEvent, false);
                    }
                });
            }

            __result = list;
        }
    }
}