using System.Collections.Generic;
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

        public WorldComp_BoundWeapon(World world) : base(world) { }

        public static WorldComp_BoundWeapon Instance => Find.World.GetComponent<WorldComp_BoundWeapon>();

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref pawnToWeapon, "pawnToWeapon", LookMode.Reference, LookMode.Reference, ref pawnKeys, ref weaponValues);
        }

        public void Set(Pawn pawn, ThingWithComps weapon)
        {
            if (pawn == null)
                return;

            if (weapon == null || weapon.Destroyed)
            {
                pawnToWeapon.Remove(pawn);
                return;
            }

            // 이미 다른 폰이 이 무기를 지정 중이면 차단
            foreach (var kv in pawnToWeapon)
            {
                Pawn otherPawn = kv.Key;
                ThingWithComps otherWeapon = kv.Value;

                if (otherPawn == null || otherWeapon == null)
                    continue;

                if (otherWeapon.Destroyed || otherPawn.Dead || otherPawn.DestroyedOrNull())
                    continue;

                if (otherPawn != pawn && ReferenceEquals(otherWeapon, weapon))
                {
                    Messages.Message(
                        "BW_WeaponAlreadyAssigned".Translate(weapon.LabelCap, otherPawn.LabelShortCap),
                        MessageTypeDefOf.RejectInput,
                        false
                    );
                    return;
                }
            }

            pawnToWeapon[pawn] = weapon;
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
    }
}