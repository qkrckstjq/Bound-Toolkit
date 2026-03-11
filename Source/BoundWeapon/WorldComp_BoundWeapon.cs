using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace BoundWeapon
{
    public class WorldComp_BoundWeapon : WorldComponent
    {
        private Dictionary<Pawn, BoundWeaponLoadout> pawnToLoadout = new Dictionary<Pawn, BoundWeaponLoadout>();
        private List<Pawn> pawnKeys;
        private List<BoundWeaponLoadout> loadoutValues;

        private Dictionary<Pawn, ThingWithComps> legacyPawnToWeapon;
        private List<Pawn> legacyPawnKeys;
        private List<ThingWithComps> legacyWeaponValues;

        public WorldComp_BoundWeapon(World world) : base(world)
        {
        }

        public static WorldComp_BoundWeapon Instance
        {
            get { return Find.World.GetComponent<WorldComp_BoundWeapon>(); }
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref pawnToLoadout, "pawnToLoadout", LookMode.Reference, LookMode.Deep, ref pawnKeys, ref loadoutValues);
            Scribe_Collections.Look(ref legacyPawnToWeapon, "pawnToWeapon", LookMode.Reference, LookMode.Reference, ref legacyPawnKeys, ref legacyWeaponValues);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                MigrateLegacyData();
                CleanupInvalidEntries();
            }
        }

        public bool TrySet(Pawn pawn, ThingWithComps weapon, BoundWeaponSlot slot)
        {
            if (pawn == null)
                return false;

            if (!BoundWeaponRuntimeProvider.Current.CanAssignToSlot(pawn, weapon, slot))
            {
                Messages.Message(
                    "BW_InvalidSlotAssignment".Translate(BoundWeaponUtil.SlotLabel(slot), weapon.LabelCap),
                    MessageTypeDefOf.RejectInput,
                    false
                );
                return false;
            }

            if (slot == BoundWeaponSlot.OffHand && !BoundWeaponRuntimeProvider.SupportsOffHand)
            {
                Messages.Message("Off-hand assignment requires Dual Wield.", MessageTypeDefOf.RejectInput, false);
                return false;
            }

            if (weapon == null || weapon.Destroyed)
            {
                Clear(pawn, slot);
                return true;
            }

            CleanupInvalidEntries();

            bool hasLinkSignal;
            Pawn linkedPawn;
            GetLinkInfo(weapon, out hasLinkSignal, out linkedPawn);

            if (hasLinkSignal && linkedPawn == null)
            {
                Messages.Message(
                    "BW_WeaponLinkedUnknownOwner".Translate(weapon.LabelCap),
                    MessageTypeDefOf.RejectInput,
                    false
                );
                return false;
            }

            if (hasLinkSignal && linkedPawn != null && !ReferenceEquals(linkedPawn, pawn))
            {
                Messages.Message(
                    "BW_WeaponLinkedToOtherPawn".Translate(weapon.LabelCap, linkedPawn.LabelShortCap),
                    MessageTypeDefOf.RejectInput,
                    false
                );
                return false;
            }

            Pawn ownerPawn;
            BoundWeaponSlot ownerSlot;
            if (TryFindOwner(weapon, out ownerPawn, out ownerSlot))
            {
                if (!ReferenceEquals(ownerPawn, pawn))
                {
                    Messages.Message(
                        "BW_WeaponAlreadyAssigned".Translate(weapon.LabelCap, ownerPawn.LabelShortCap),
                        MessageTypeDefOf.RejectInput,
                        false
                    );
                    return false;
                }

                if (ownerSlot == slot)
                    return true;
            }

            BoundWeaponLoadout loadout = GetOrCreateLoadout(pawn);

            BoundWeaponSlot otherSlot = slot == BoundWeaponSlot.Primary
                ? BoundWeaponSlot.OffHand
                : BoundWeaponSlot.Primary;

            ThingWithComps other = loadout.Get(otherSlot);
            if (other != null && ReferenceEquals(other, weapon))
                loadout.Clear(otherSlot);

            loadout.Set(slot, weapon);
            CleanupPawnEntryIfEmpty(pawn, loadout);
            return true;
        }

        public void Clear(Pawn pawn, BoundWeaponSlot slot)
        {
            if (pawn == null)
                return;

            BoundWeaponLoadout loadout;
            if (!pawnToLoadout.TryGetValue(pawn, out loadout) || loadout == null)
                return;

            loadout.Clear(slot);
            CleanupPawnEntryIfEmpty(pawn, loadout);
        }

        public void ClearAll(Pawn pawn)
        {
            if (pawn == null)
                return;

            pawnToLoadout.Remove(pawn);
        }

        public bool TryGet(Pawn pawn, BoundWeaponSlot slot, out ThingWithComps weapon)
        {
            weapon = null;

            if (pawn == null)
                return false;

            BoundWeaponLoadout loadout;
            if (!pawnToLoadout.TryGetValue(pawn, out loadout) || loadout == null)
                return false;

            loadout.CleanupDestroyed();
            CleanupPawnEntryIfEmpty(pawn, loadout);

            weapon = loadout.Get(slot);
            return weapon != null && !weapon.Destroyed;
        }

        public bool TryGetAny(Pawn pawn, out ThingWithComps weapon)
        {
            weapon = null;

            ThingWithComps primary;
            if (TryGet(pawn, BoundWeaponSlot.Primary, out primary))
            {
                weapon = primary;
                return true;
            }

            ThingWithComps offHand;
            if (TryGet(pawn, BoundWeaponSlot.OffHand, out offHand))
            {
                weapon = offHand;
                return true;
            }

            return false;
        }

        public bool AnyAssigned(Pawn pawn)
        {
            ThingWithComps weapon;
            return TryGetAny(pawn, out weapon);
        }

        public void ClearByWeapon(ThingWithComps weapon)
        {
            if (weapon == null)
                return;

            List<Pawn> removePawns = null;

            foreach (KeyValuePair<Pawn, BoundWeaponLoadout> kv in pawnToLoadout)
            {
                Pawn pawn = kv.Key;
                BoundWeaponLoadout loadout = kv.Value;
                if (pawn == null || loadout == null)
                    continue;

                bool changed = false;

                if (loadout.primary != null && ReferenceEquals(loadout.primary, weapon))
                {
                    loadout.primary = null;
                    changed = true;
                }

                if (loadout.offHand != null && ReferenceEquals(loadout.offHand, weapon))
                {
                    loadout.offHand = null;
                    changed = true;
                }

                if (changed && !loadout.HasAny())
                {
                    if (removePawns == null)
                        removePawns = new List<Pawn>();

                    removePawns.Add(pawn);
                }
            }

            if (removePawns == null)
                return;

            for (int i = 0; i < removePawns.Count; i++)
                pawnToLoadout.Remove(removePawns[i]);
        }

        BoundWeaponLoadout GetOrCreateLoadout(Pawn pawn)
        {
            BoundWeaponLoadout loadout;
            if (!pawnToLoadout.TryGetValue(pawn, out loadout) || loadout == null)
            {
                loadout = new BoundWeaponLoadout();
                pawnToLoadout[pawn] = loadout;
            }

            return loadout;
        }

        void CleanupPawnEntryIfEmpty(Pawn pawn, BoundWeaponLoadout loadout)
        {
            if (pawn == null || loadout == null)
                return;

            loadout.CleanupDestroyed();

            if (!loadout.HasAny())
                pawnToLoadout.Remove(pawn);
        }

        void CleanupInvalidEntries()
        {
            List<Pawn> removePawns = null;

            foreach (KeyValuePair<Pawn, BoundWeaponLoadout> kv in pawnToLoadout)
            {
                Pawn pawn = kv.Key;
                BoundWeaponLoadout loadout = kv.Value;

                if (pawn == null || pawn.Dead || pawn.DestroyedOrNull() || loadout == null)
                {
                    if (removePawns == null)
                        removePawns = new List<Pawn>();

                    removePawns.Add(pawn);
                    continue;
                }

                loadout.CleanupDestroyed();

                if (!loadout.HasAny())
                {
                    if (removePawns == null)
                        removePawns = new List<Pawn>();

                    removePawns.Add(pawn);
                }
            }

            if (removePawns == null)
                return;

            for (int i = 0; i < removePawns.Count; i++)
                pawnToLoadout.Remove(removePawns[i]);
        }

        void MigrateLegacyData()
        {
            if (legacyPawnToWeapon == null || legacyPawnToWeapon.Count == 0)
                return;

            foreach (KeyValuePair<Pawn, ThingWithComps> kv in legacyPawnToWeapon)
            {
                Pawn pawn = kv.Key;
                ThingWithComps weapon = kv.Value;

                if (pawn == null || pawn.Dead || pawn.DestroyedOrNull())
                    continue;

                if (weapon == null || weapon.Destroyed)
                    continue;

                BoundWeaponLoadout loadout;
                if (!pawnToLoadout.TryGetValue(pawn, out loadout) || loadout == null)
                {
                    loadout = new BoundWeaponLoadout();
                    pawnToLoadout[pawn] = loadout;
                }

                if (loadout.primary == null)
                    loadout.primary = weapon;
            }

            legacyPawnToWeapon = null;
            legacyPawnKeys = null;
            legacyWeaponValues = null;
        }

        bool TryFindOwner(ThingWithComps weapon, out Pawn ownerPawn, out BoundWeaponSlot ownerSlot)
        {
            ownerPawn = null;
            ownerSlot = BoundWeaponSlot.Primary;

            foreach (KeyValuePair<Pawn, BoundWeaponLoadout> kv in pawnToLoadout)
            {
                Pawn pawn = kv.Key;
                BoundWeaponLoadout loadout = kv.Value;

                if (pawn == null || pawn.Dead || pawn.DestroyedOrNull() || loadout == null)
                    continue;

                if (loadout.primary != null && !loadout.primary.Destroyed && ReferenceEquals(loadout.primary, weapon))
                {
                    ownerPawn = pawn;
                    ownerSlot = BoundWeaponSlot.Primary;
                    return true;
                }

                if (loadout.offHand != null && !loadout.offHand.Destroyed && ReferenceEquals(loadout.offHand, weapon))
                {
                    ownerPawn = pawn;
                    ownerSlot = BoundWeaponSlot.OffHand;
                    return true;
                }
            }

            return false;
        }

        private static void GetLinkInfo(ThingWithComps weapon, out bool hasLinkSignal, out Pawn linkedPawn)
        {
            hasLinkSignal = false;
            linkedPawn = null;

            if (weapon == null)
                return;

            var comps = weapon.AllComps;
            if (comps == null)
                return;

            for (int i = 0; i < comps.Count; i++)
            {
                ThingComp c = comps[i];
                if (c == null)
                    continue;

                string name = c.GetType().Name;

                if (name == "CompBladelinkWeapon")
                {
                    Pawn bonded = GetPawn(c, "BondedPawn", "oldBondedPawn", "bondedPawn");
                    if (bonded != null)
                    {
                        hasLinkSignal = true;
                        linkedPawn = bonded;
                        return;
                    }

                    bool bondedFlag = GetBool(c, "Bonded", "bonded", "HasBond", "hasBond", "IsBonded", "isBonded");
                    if (bondedFlag)
                    {
                        hasLinkSignal = true;
                        linkedPawn = null;
                        return;
                    }

                    bool biocoded = GetBool(c, "Biocoded", "biocoded", "Biocodable");
                    if (biocoded)
                    {
                        hasLinkSignal = true;
                        Pawn coded = GetPawn(c, "CodedPawn", "codedPawn", "BiocodedTo", "biocodedTo");
                        linkedPawn = coded;
                        return;
                    }

                    continue;
                }

                if (name == "CompBiocodable")
                {
                    bool biocoded = GetBool(c, "Biocoded", "biocoded", "Biocodable");
                    if (!biocoded)
                        continue;

                    hasLinkSignal = true;
                    Pawn coded = GetPawn(c, "CodedPawn", "codedPawn", "BiocodedTo", "biocodedTo");
                    linkedPawn = coded;
                    return;
                }
            }
        }

        private static bool GetBool(object obj, params string[] names)
        {
            var t = obj.GetType();
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            for (int i = 0; i < names.Length; i++)
            {
                string n = names[i];

                var p = t.GetProperty(n, flags);
                if (p != null && p.PropertyType == typeof(bool))
                    return (bool)p.GetValue(obj, null);

                var f = t.GetField(n, flags);
                if (f != null && f.FieldType == typeof(bool))
                    return (bool)f.GetValue(obj);
            }

            return false;
        }

        private static Pawn GetPawn(object obj, params string[] names)
        {
            var t = obj.GetType();
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            for (int i = 0; i < names.Length; i++)
            {
                string n = names[i];

                var p = t.GetProperty(n, flags);
                if (p != null && typeof(Pawn).IsAssignableFrom(p.PropertyType))
                    return p.GetValue(obj, null) as Pawn;

                var f = t.GetField(n, flags);
                if (f != null && typeof(Pawn).IsAssignableFrom(f.FieldType))
                    return f.GetValue(obj) as Pawn;
            }

            return null;
        }
    }
}