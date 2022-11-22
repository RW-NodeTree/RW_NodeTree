using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace RW_NodeTree.Patch
{
    [HarmonyPatch(typeof(Pawn_MeleeVerbs))]
    internal static partial class Pawn_MeleeVerbs_Patcher
    {

        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(Pawn_MeleeVerbs),
            "<GetUpdatedAvailableVerbsList>g__IsUsableMeleeVerb|18_0"
        )]
        public static bool PrePawn_MeleeVerbs_IsUsableMeleeVerb(Verb v, ref bool __result) => __result = v.IsMeleeAttack;

        //private static AccessTools.FieldRef<Verb, int> ticksToNextBurstShot = AccessTools.FieldRefAccess<int>(typeof(Verb), "ticksToNextBurstShot");
    }
}
