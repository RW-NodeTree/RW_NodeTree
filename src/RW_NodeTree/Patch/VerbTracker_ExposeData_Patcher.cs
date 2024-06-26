﻿using HarmonyLib;
using System.Collections.Generic;
using Verse;

namespace RW_NodeTree.Patch
{
    [HarmonyPatch(typeof(VerbTracker))]
    internal static partial class VerbTracker_Patcher
    {
        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(VerbTracker),
            "ExposeData"
        )]
        private static void PreVerbTracker_ExposeData(VerbTracker __instance, ref List<Verb> __state)
        {
            if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
            {
                __state = CompChildNodeProccesser.GetOriginalAllVerbs(__instance);
            }
        }

        [HarmonyFinalizer]
        [HarmonyPatch(
            typeof(VerbTracker),
            "ExposeData"
        )]
        private static void FinalVerbTracker_ExposeData(VerbTracker __instance, List<Verb> __state)
        {
            if(__state != null)
            {
                CompChildNodeProccesser.GetOriginalAllVerbs(__instance)?.SortBy(x => __state.IndexOf(x));
            }
        }
    }
}
