using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace BoundWeapon
{
    public class WorldComp_BoundWeapon : WorldComponent
    {
        private Dictionary<Pawn, ThingWithComps> pawnToWeapon = new Dictionary<Pawn, ThingWithComps>();
        private List<Pawn> pawnKeys;
        private List<ThingWithComps> weaponValues;

        public WorldComp_BoundWeapon(World world) : base(world)
        {
        }

        public static WorldComp_BoundWeapon Instance => Find.World.GetComponent<WorldComp_BoundWeapon>();

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref pawnToWeapon, "pawnToWeapon", LookMode.Reference, LookMode.Reference, ref pawnKeys, ref weaponValues);
        }

        public bool Set(Pawn pawn, ThingWithComps weapon)
        {
            if (pawn == null)
                return false;

            if (weapon == null || weapon.Destroyed)
            {
                pawnToWeapon.Remove(pawn);
                return true;
            }

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

            foreach (var kv in pawnToWeapon)
            {
                Pawn otherPawn = kv.Key;
                ThingWithComps otherWeapon = kv.Value;

                if (otherPawn == null || otherWeapon == null)
                    continue;

                if (otherPawn.Dead || otherPawn.DestroyedOrNull())
                    continue;

                if (otherWeapon.Destroyed)
                    continue;

                if (!ReferenceEquals(otherPawn, pawn) && ReferenceEquals(otherWeapon, weapon))
                {
                    Messages.Message(
                        "BW_WeaponAlreadyAssigned".Translate(weapon.LabelCap, otherPawn.LabelShortCap),
                        MessageTypeDefOf.RejectInput,
                        false
                    );
                    return false;
                }
            }

            pawnToWeapon[pawn] = weapon;
            return true;
        }

        public void Clear(Pawn pawn)
        {
            if (pawn == null)
                return;

            pawnToWeapon.Remove(pawn);
        }

        public bool TryGet(Pawn pawn, out ThingWithComps weapon)
        {
            weapon = null;

            if (pawn == null)
                return false;

            if (!pawnToWeapon.TryGetValue(pawn, out weapon))
                return false;

            if (weapon == null || weapon.Destroyed)
            {
                pawnToWeapon.Remove(pawn);
                weapon = null;
                return false;
            }

            return true;
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