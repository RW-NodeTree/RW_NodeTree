using HarmonyLib;
using RW_NodeTree.Tools;
using System;
using System.Collections.Generic;
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
        private static void PrePawn_TryGetAttackVerb(Pawn __instance)
        {
            try
            {
                Job job = __instance.CurJob;
                if (job != null && typeof(JobDriver_AttackStatic).IsAssignableFrom(job.def.driverClass) && job.verbToUse?.Caster == __instance)
                {
                    List<Verb?>? verbList = __instance.CurJob.verbToUse.verbTracker.GetOriginalAllVerbs();
                    if (verbList.Remove(__instance.CurJob.verbToUse))
                    {
                        verbList.Insert(0, __instance.CurJob.verbToUse);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        //private static AccessTools.FieldRef<Verb, int> ticksToNextBurstShot = AccessTools.FieldRefAccess<int>(typeof(Verb), "ticksToNextBurstShot");
    }
}
