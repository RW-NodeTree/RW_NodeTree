using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;

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
            CompChildNodeProccesser proccess = ((__instance.verb?.verbTracker?.directOwner as ThingComp)?.parent) ?? ((__instance.verb?.verbTracker?.directOwner) as Thing);
            if(proccess != null)
            {
                Thing part = proccess.GetBeforeConvertVerbCorrespondingThing(__instance.verb.verbTracker.directOwner.GetType(), __instance.verb, proccess.Props.VerbIconVerbInstanceSource).Item1;
                if(part != null)
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
            if(ev.type == EventType.MouseUp && ev.keyCode == KeyCode.Mouse1)
            {
                
                SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                VerbTracker verbTracker = __instance.verb?.verbTracker;
                List<Verb> verbList = CompChildNodeProccesser.GetOriginalAllVerbs(verbTracker);
                if (verbList.Remove(__instance.verb))
                {
                    verbList.Insert(0, __instance.verb);
                }
                return false;
            }
            return true;
        }
    }
}
