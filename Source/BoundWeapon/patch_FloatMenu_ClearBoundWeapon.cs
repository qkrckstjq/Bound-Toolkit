//using System;
//using System.Collections.Generic;
//using System.Reflection;
//using HarmonyLib;
//using RimWorld;
//using Verse;

//namespace BoundWeapon
//{
//    [HarmonyPatch]
//    [HarmonyPriority(Priority.Last)]
//    public static class Patch_FloatMenu_ClearBoundWeapon
//    {
//        static MethodBase TargetMethod()
//        {
//            return AccessTools.Method(typeof(FloatMenuMakerMap), "GetProviderOptions",
//                new Type[] { typeof(FloatMenuContext), typeof(List<FloatMenuOption>) });
//        }

//        static void Postfix(FloatMenuContext __0, List<FloatMenuOption> __1)
//        {
//            if (__0 == null || __1 == null) return;

//            Pawn pawn = __0.FirstSelectedPawn;
//            if (pawn == null) return;
//            if (pawn.Faction != Faction.OfPlayer) return;

//            ThingWithComps bound;
//            if (!BoundWeaponApi.TryGet(pawn, out bound)) return;
//            if (bound == null) return;

//            string label = "지정 무기 해제 (현재: " + bound.LabelCap + ")";
//            if (HasLabel(__1, label)) return;

//            __1.Add(new FloatMenuOption(label, () =>
//            {
//                BoundWeaponApi.Clear(pawn);
//                Messages.Message(pawn.LabelShortCap + " 지정 무기 해제", MessageTypeDefOf.NeutralEvent, false);
//            }));
//        }

//        static bool HasLabel(List<FloatMenuOption> opts, string label)
//        {
//            for (int i = 0; i < opts.Count; i++)
//            {
//                FloatMenuOption o = opts[i];
//                if (o == null) continue;

//                string l = GetLabel(o);
//                if (l == label) return true;
//            }
//            return false;
//        }

//        static string GetLabel(FloatMenuOption opt)
//        {
//            FieldInfo f = AccessTools.Field(opt.GetType(), "label");
//            if (f != null) return f.GetValue(opt) as string;

//            PropertyInfo p = AccessTools.Property(opt.GetType(), "Label");
//            if (p != null) return p.GetValue(opt, null) as string;

//            return null;
//        }
//    }
//}