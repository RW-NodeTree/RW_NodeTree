using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace RW_NodeTree.Patch
{
    [HarmonyPatch(typeof(Pawn))]
    internal static partial class Pawn_Patcher
    {

        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(Pawn),
            "TryGetAttackVerb"
        )]
        public static bool PrePawn_TryGetAttackVerb(Pawn __instance, ref Verb __result)
        {
            if(__instance.CurJob != null && __instance.CurJob.GetCachedDriverDirect != null && typeof(JobDriver_AttackStatic).IsAssignableFrom(__instance.CurJob.GetCachedDriverDirect.GetType()) && __instance.CurJob.verbToUse != null && __instance.CurJob.verbToUse.Caster == __instance && __instance.CurJob.verbToUse.Available())
            {
                __result = __instance.CurJob.verbToUse;
                //if (Prefs.DevMode) Log.Message("ticksToNextBurstShot : " + ticksToNextBurstShot(__result) + " state : " + __result.state + " caster : " + __result.caster);
                return false;
            }
            return true;
        }

        //private static AccessTools.FieldRef<Verb, int> ticksToNextBurstShot = AccessTools.FieldRefAccess<int>(typeof(Verb), "ticksToNextBurstShot");
    }
}
