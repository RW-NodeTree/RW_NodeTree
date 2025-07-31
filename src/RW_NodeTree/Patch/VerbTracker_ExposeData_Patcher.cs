using HarmonyLib;
using RW_NodeTree.Tools;
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
        private static void PreVerbTracker_ExposeData(VerbTracker __instance, ref List<Verb?>? __state)
        {
            if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
            {
                __state = __instance.GetOriginalAllVerbs();
            }
        }

        [HarmonyFinalizer]
        [HarmonyPatch(
            typeof(VerbTracker),
            "ExposeData"
        )]
        private static void FinalVerbTracker_ExposeData(VerbTracker __instance, List<Verb?>? __state)
        {
            if (__state != null)
            {
                __instance.GetOriginalAllVerbs()?.SortBy(x => __state.IndexOf(x));
            }
        }
    }
}
