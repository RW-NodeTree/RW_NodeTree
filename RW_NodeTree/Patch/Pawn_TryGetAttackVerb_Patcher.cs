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
        public static void PrePawn_TryGetAttackVerb(Pawn __instance)
        {
            CompChildNodeProccesser weapon = __instance.equipment?.Primary;
            if(weapon != null)
            {
                Job job = __instance.CurJob;
                if(job != null && typeof(JobDriver_AttackStatic).IsAssignableFrom(job.def.driverClass) && job.verbToUse?.Caster == __instance)
                {
                    CompEquippable equippable = __instance.equipment.PrimaryEq;
                    List<Verb> verbList = CompChildNodeProccesser.GetOriginalAllVerbs(equippable.verbTracker);
                    if (verbList.Remove(__instance.CurJob.verbToUse))
                    {
                        verbList.Insert(0, __instance.CurJob.verbToUse);
                    }
                }
            }
        }

        //private static AccessTools.FieldRef<Verb, int> ticksToNextBurstShot = AccessTools.FieldRefAccess<int>(typeof(Verb), "ticksToNextBurstShot");
    }
}
