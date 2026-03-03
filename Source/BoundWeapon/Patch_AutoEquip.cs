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

            if (pawn.jobs?.curJob != null && pawn.jobs.curJob.playerForced) return true;

            if (pawn.Drafted) return true;

            if (pawn.equipment == null)
                return true;

            if (!WorldComp_BoundWeapon.Instance.TryGet(pawn, out var weapon))
                return true;

            if (!BoundWeaponUtil.NeedsEquip(pawn, weapon))
                return true;

            if (BoundWeaponUtil.TryEquipFromInventory(pawn, weapon))
                return true;

            if (!pawn.Spawned)
                return true;

            if (!weapon.Spawned || weapon.Map != pawn.Map)
                return true;

            if (!pawn.CanReserveAndReach(weapon, PathEndMode.Touch, Danger.Deadly))
                return true;

            var job = JobMaker.MakeJob(JobDefOf.Equip, weapon);
            job.ignoreForbidden = true;

            var thinkTree = pawn.thinker?.MainThinkTree;
            __instance.StartJob(job, JobCondition.None, null, false, true, thinkTree);

            return false;
        }
    }
}