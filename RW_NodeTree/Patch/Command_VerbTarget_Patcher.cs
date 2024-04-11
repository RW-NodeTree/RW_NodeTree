using HarmonyLib;
using UnityEngine;
using Verse;

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
        private static bool PreVerb_DirectOwner(Command_VerbTarget __instance, ref Color __result)
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
    }
}
