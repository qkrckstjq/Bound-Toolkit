using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace BoundWeapon
{
    public class Window_BoundWeaponManager : Window
    {
        readonly Map map;
        Vector2 scroll;

        static MethodInfo isInValidStorage1;
        static MethodInfo isInValidStorage2;

        public override Vector2 InitialSize => new Vector2(920f, 660f);

        public Window_BoundWeaponManager(Map map)
        {
            this.map = map;
            doCloseX = true;
            draggable = true;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = false;

            if (isInValidStorage1 == null)
                isInValidStorage1 = AccessTools.Method(typeof(StoreUtility), "IsInValidStorage", new Type[] { typeof(Thing) });
            if (isInValidStorage2 == null)
                isInValidStorage2 = AccessTools.Method(typeof(StoreUtility), "IsInValidStorage", new Type[] { typeof(Thing), typeof(Map) });
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, inRect.width, 34f), "BoundWeapon 무기 지정 관리");
            Text.Font = GameFont.Small;

            if (map == null)
            {
                Widgets.Label(new Rect(0f, 44f, inRect.width, 24f), "맵이 없습니다.");
                return;
            }

            List<Pawn> pawns = map.mapPawns.FreeColonistsSpawned;

            Rect outRect = new Rect(0f, 44f, inRect.width, inRect.height - 44f);
            Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, pawns.Count * 36f + 10f);

            float y = 0f;
            Widgets.BeginScrollView(outRect, ref scroll, viewRect);

            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn p = pawns[i];
                Rect row = new Rect(0f, y, viewRect.width, 32f);
                DrawRow(row, p);
                y += 36f;
            }

            Widgets.EndScrollView();
        }

        void DrawRow(Rect row, Pawn p)
        {
            float x = 0f;

            Widgets.Label(new Rect(x, row.y, 220f, row.height), p.LabelShortCap);
            x += 230f;

            ThingWithComps w;
            string bound = "없음";
            if (BoundWeaponApi.TryGet(p, out w) && w != null)
                bound = w.LabelCap;

            Widgets.Label(new Rect(x, row.y, 360f, row.height), "지정: " + bound);
            x += 370f;

            if (Widgets.ButtonText(new Rect(x, row.y, 130f, row.height), "무기 지정"))
                OpenWeaponSelectMenu(p);
            x += 140f;

            if (Widgets.ButtonText(new Rect(x, row.y, 130f, row.height), "지정 해제"))
            {
                BoundWeaponApi.Clear(p);
                Messages.Message(p.LabelShortCap + " 지정 무기 해제", MessageTypeDefOf.NeutralEvent, false);
            }
        }

        void OpenWeaponSelectMenu(Pawn pawn)
        {
            List<ThingWithComps> weapons = GetStoredWeapons(map);
            if (weapons.Count == 0)
            {
                Messages.Message("저장구역에서 무기를 찾지 못했습니다.", MessageTypeDefOf.RejectInput, false);
                return;
            }

            weapons = weapons.OrderBy(t => t.LabelCap).ToList();

            List<FloatMenuOption> opts = new List<FloatMenuOption>();
            for (int i = 0; i < weapons.Count; i++)
            {
                ThingWithComps w = weapons[i];
                if (w == null || w.Destroyed) continue;

                string label = w.LabelCap + " (" + w.HitPoints + "/" + w.MaxHitPoints + ")";
                opts.Add(new FloatMenuOption(label, () =>
                {
                    BoundWeaponApi.Set(pawn, w);
                    w.SetForbidden(false, false);
                    Messages.Message(pawn.LabelShortCap + " 지정 무기: " + w.LabelCap, MessageTypeDefOf.PositiveEvent, false);
                }));
            }

            Find.WindowStack.Add(new FloatMenu(opts));
        }

        List<ThingWithComps> GetStoredWeapons(Map map)
        {
            List<ThingWithComps> result = new List<ThingWithComps>();
            List<Thing> all = map.listerThings.ThingsInGroup(ThingRequestGroup.Weapon);

            for (int i = 0; i < all.Count; i++)
            {
                Thing t = all[i];
                if (t == null) continue;
                if (t.Destroyed) continue;
                if (!t.Spawned) continue;
                if (t.Map != map) continue;

                if (!BoundWeaponUtil.IsValidWeapon(t)) continue;
                if (!IsInValidStorage(t, map)) continue;

                ThingWithComps twc = t as ThingWithComps;
                if (twc == null) continue;

                result.Add(twc);
            }

            return result;
        }

        bool IsInValidStorage(Thing t, Map map)
        {
            if (isInValidStorage1 != null)
            {
                object r = isInValidStorage1.Invoke(null, new object[] { t });
                if (r is bool) return (bool)r;
            }

            if (isInValidStorage2 != null)
            {
                object r = isInValidStorage2.Invoke(null, new object[] { t, map });
                if (r is bool) return (bool)r;
            }

            return t.Position.GetSlotGroup(map) != null;
        }
    }
}