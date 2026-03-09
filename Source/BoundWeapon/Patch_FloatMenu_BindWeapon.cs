using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;

namespace BoundWeapon
{
    [HarmonyPatch]
    public static class Patch_FloatMenu_BindWeapon
    {
        static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(FloatMenuMakerMap), "GetProviderOptions",
                new Type[] { typeof(FloatMenuContext), typeof(List<FloatMenuOption>) });
        }

        static void Postfix(FloatMenuContext __0, List<FloatMenuOption> __1)
        {
            if (__0 == null || __1 == null) return;

            Pawn pawn = __0.FirstSelectedPawn;
            if (pawn == null || pawn.Faction != Faction.OfPlayer || pawn.Map == null) return;

            List<Thing> clickedThings = __0.ClickedThings;
            if (clickedThings == null || clickedThings.Count == 0) return;

            for (int i = 0; i < clickedThings.Count; i++)
            {
                Thing t = clickedThings[i];
                if (t == null) continue;

                if (!BoundWeaponUtil.IsValidWeapon(t)) continue;

                ThingWithComps weapon = t as ThingWithComps;
                if (weapon == null) continue;
                if (!weapon.Spawned) continue;
                if (weapon.Map != pawn.Map) continue;

                string label = "BW_BindWeapon".Translate(weapon.LabelCap);

                if (HasLabel(__1, label)) continue;

                __1.Add(new FloatMenuOption(label, () =>
                {
                    bool ok = WorldComp_BoundWeapon.Instance.Set(pawn, weapon);
                    if (!ok)
                        return;

                    weapon.SetForbidden(false, false);
                    Messages.Message("BW_DesignatedWeaponSetMsg".Translate(pawn.LabelShortCap, weapon.LabelCap),
                        MessageTypeDefOf.PositiveEvent, false);
                }));
            }
        }

        static bool HasLabel(List<FloatMenuOption> opts, string label)
        {
            for (int i = 0; i < opts.Count; i++)
            {
                FloatMenuOption o = opts[i];
                if (o == null) continue;

                string l = GetLabel(o);
                if (l == label) return true;
            }
            return false;
        }

        static string GetLabel(FloatMenuOption opt)
        {
            FieldInfo f = AccessTools.Field(opt.GetType(), "label");
            if (f != null) return f.GetValue(opt) as string;

            PropertyInfo p = AccessTools.Property(opt.GetType(), "Label");
            if (p != null) return p.GetValue(opt, null) as string;

            return null;
        }
    }
}