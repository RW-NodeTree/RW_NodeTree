using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
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

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(VerbTracker),
            "ExposeData"
        )]
        private static void PostVerbTracker_ExposeData(VerbTracker __instance, List<Verb> __state)
        {
            if(__state != null)
            {
                CompChildNodeProccesser.GetOriginalAllVerbs(__instance)?.SortBy(x => __state.IndexOf(x));
            }
        }
    }
}
