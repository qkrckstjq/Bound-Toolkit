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

        public override Vector2 InitialSize
        {
            get
            {
                return BoundWeaponRuntimeProvider.SupportsOffHand
                    ? new Vector2(1360f, 660f)
                    : new Vector2(920f, 660f);
            }
        }

        public Window_BoundWeaponManager(Map map)
        {
            this.map = map;
            doCloseX = true;
            draggable = true;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = true;

            if (isInValidStorage1 == null)
                isInValidStorage1 = AccessTools.Method(typeof(StoreUtility), "IsInValidStorage", new Type[] { typeof(Thing) });

            if (isInValidStorage2 == null)
                isInValidStorage2 = AccessTools.Method(typeof(StoreUtility), "IsInValidStorage", new Type[] { typeof(Thing), typeof(Map) });
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, inRect.width, 34f), "BW_ManagerWindowTitle".Translate().ToString());
            Text.Font = GameFont.Small;

            if (map == null)
            {
                Widgets.Label(new Rect(0f, 44f, inRect.width, 24f), "BW_NoMap".Translate().ToString());
                return;
            }

            List<Pawn> pawns = map.mapPawns.FreeColonistsSpawned;

            Rect outRect = new Rect(0f, 44f, inRect.width, inRect.height - 44f);
            Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, pawns.Count * 40f + 10f);

            float y = 0f;
            Widgets.BeginScrollView(outRect, ref scroll, viewRect);

            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn p = pawns[i];
                Rect row = new Rect(0f, y, viewRect.width, 34f);
                DrawRow(row, p);
                y += 40f;
            }

            Widgets.EndScrollView();
        }

        void DrawRow(Rect row, Pawn pawn)
        {
            float x = 0f;
            bool supportsOffHand = BoundWeaponRuntimeProvider.SupportsOffHand;

            Widgets.Label(new Rect(x, row.y, 180f, row.height), pawn.LabelShortCap);
            x += 190f;

            DrawSlotSection(row, pawn, ref x, BoundWeaponSlot.Primary);

            if (supportsOffHand)
                DrawSlotSection(row, pawn, ref x, BoundWeaponSlot.OffHand);
        }

        void DrawSlotSection(Rect row, Pawn pawn, ref float x, BoundWeaponSlot slot)
        {
            ThingWithComps weapon;
            string current = BoundWeaponApi.TryGet(pawn, slot, out weapon) && weapon != null
                ? weapon.LabelCap
                : "BW_None".Translate().ToString();

            string title = BoundWeaponUtil.SlotLabel(slot);
            Widgets.Label(new Rect(x, row.y, 250f, row.height), title + ": " + current);
            x += 260f;

            if (Widgets.ButtonText(new Rect(x, row.y, 90f, row.height), "BW_Set".Translate().ToString()))
                OpenWeaponSelectMenu(pawn, slot);
            x += 100f;

            if (Widgets.ButtonText(new Rect(x, row.y, 90f, row.height), "BW_Clear".Translate().ToString()))
            {
                BoundWeaponApi.Clear(pawn, slot);
                Messages.Message(
                    "BW_AssignmentCleared".Translate(pawn.LabelShortCap, title),
                    MessageTypeDefOf.NeutralEvent,
                    false
                );
            }
            x += 110f;
        }

        void OpenWeaponSelectMenu(Pawn pawn, BoundWeaponSlot slot)
        {
            List<ThingWithComps> weapons = GetStoredWeapons(map);
            if (weapons.Count == 0)
            {
                Messages.Message("BW_NoStoredWeapons".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            weapons = weapons.OrderBy(t => t.LabelCap).ToList();

            List<FloatMenuOption> opts = new List<FloatMenuOption>();
            for (int i = 0; i < weapons.Count; i++)
            {
                ThingWithComps w = weapons[i];
                if (w == null || w.Destroyed)
                    continue;

                if (!BoundWeaponRuntimeProvider.Current.CanAssignToSlot(pawn, w, slot))
                    continue;

                string label = BoundWeaponUtil.SlotLabel(slot) + " - " +
                               "BW_WeaponListEntry".Translate(w.LabelCap, w.HitPoints, w.MaxHitPoints).ToString();

                opts.Add(new FloatMenuOption(label, delegate
                {
                    if (!BoundWeaponApi.TrySet(pawn, w, slot))
                        return;

                    w.SetForbidden(false, false);
                    Messages.Message(
                        "BW_AssignmentSet".Translate(pawn.LabelShortCap, BoundWeaponUtil.SlotLabel(slot), w.LabelCap),
                        MessageTypeDefOf.PositiveEvent,
                        false
                    );
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
                if (t == null)
                    continue;
                if (t.Destroyed)
                    continue;
                if (!t.Spawned)
                    continue;
                if (t.Map != map)
                    continue;
                if (!BoundWeaponUtil.IsValidWeapon(t))
                    continue;
                if (!IsInValidStorage(t, map))
                    continue;

                ThingWithComps twc = t as ThingWithComps;
                if (twc == null)
                    continue;

                result.Add(twc);
            }

            return result;
        }

        bool IsInValidStorage(Thing t, Map map)
        {
            if (isInValidStorage1 != null)
            {
                object r = isInValidStorage1.Invoke(null, new object[] { t });
                if (r is bool)
                    return (bool)r;
            }

            if (isInValidStorage2 != null)
            {
                object r = isInValidStorage2.Invoke(null, new object[] { t, map });
                if (r is bool)
                    return (bool)r;
            }

            return t.Position.GetSlotGroup(map) != null;
        }
    }
}