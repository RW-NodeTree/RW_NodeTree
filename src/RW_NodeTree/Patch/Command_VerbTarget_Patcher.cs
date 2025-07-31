using HarmonyLib;
using RimWorld;
using RW_NodeTree.Tools;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RW_NodeTree.Patch
{
    [HarmonyPatch(typeof(Command_VerbTarget))]
    internal static class Command_VerbTarget_Patcher
    {


        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(Command_VerbTarget),
            "get_IconDrawColor"
        )]
        private static bool PreIconDrawColor(Command_VerbTarget __instance, ref Color __result)
        {
            CompChildNodeProccesser? proccess = ((__instance.verb?.verbTracker?.directOwner as ThingComp)?.parent) ?? ((__instance.verb?.verbTracker?.directOwner) as Thing);
            if (proccess != null)
            {
                Thing part = proccess.GetBeforeConvertThingWithVerb(__instance.verb!.verbTracker.directOwner.GetType(), __instance.verb, proccess.Props.VerbIconVerbInstanceSource).Item1;
                if (part != null)
                {
                    __result = part.DrawColor;
                    return false;
                }
            }
            return true;
        }


        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(Command_VerbTarget),
            "ProcessInput"
        )]
        private static bool PreProcessInput(Command_VerbTarget __instance, Event ev)
        {
            if (ev.keyCode == KeyCode.Mouse1)
            {

                SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                VerbTracker? verbTracker = __instance.verb?.verbTracker;
                if (verbTracker != null)
                {
                    List<Verb?> verbList = verbTracker.GetOriginalAllVerbs();
                    if (verbList.Remove(__instance.verb))
                    {
                        verbList.Insert(0, __instance.verb);
                    }
                    return false;
                }
            }
            return true;
        }
    }
}
