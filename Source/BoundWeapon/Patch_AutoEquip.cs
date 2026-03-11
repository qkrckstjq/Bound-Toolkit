using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace BoundWeapon
{
    [HarmonyPatch(typeof(Pawn_JobTracker), "TryFindAndStartJob")]
    public static class Patch_AutoEquip
    {
        private static readonly AccessTools.FieldRef<Pawn_JobTracker, Pawn> pawnRef =
            AccessTools.FieldRefAccess<Pawn_JobTracker, Pawn>("pawn");

        public static bool Prefix(Pawn_JobTracker __instance)
        {
            Pawn pawn = pawnRef(__instance);
            if (pawn == null || pawn.DestroyedOrNull() || pawn.Dead)
                return true;

            if (pawn.Faction != Faction.OfPlayer)
                return true;

            if (pawn.Downed || pawn.InMentalState)
                return true;

            if (pawn.jobs != null && pawn.jobs.curJob != null && pawn.jobs.curJob.playerForced)
                return true;

            if (pawn.Drafted)
                return true;

            if (pawn.equipment == null)
                return true;

            if (!TryHandleSlot(__instance, pawn, BoundWeaponSlot.Primary))
                return false;

            if (BoundWeaponRuntimeProvider.SupportsOffHand)
            {
                if (!TryHandleSlot(__instance, pawn, BoundWeaponSlot.OffHand))
                    return false;
            }

            return true;
        }

        static bool TryHandleSlot(Pawn_JobTracker tracker, Pawn pawn, BoundWeaponSlot slot)
        {
            ThingWithComps weapon;
            if (!BoundWeaponApi.TryGet(pawn, slot, out weapon))
                return true;

            if (!BoundWeaponUtil.NeedsEquip(pawn, weapon, slot))
                return true;

            if (BoundWeaponUtil.TryEquipAssignedWeaponFromInventory(pawn, weapon, slot))
                return true;

            Job job = BoundWeaponUtil.TryMakeAssignedWeaponEquipJob(pawn, weapon, slot);
            if (job == null)
                return true;

            ThinkTreeDef thinkTree = pawn.thinker != null ? pawn.thinker.MainThinkTree : null;
            tracker.StartJob(job, JobCondition.None, null, false, true, thinkTree);
            return false;
        }
    }
}