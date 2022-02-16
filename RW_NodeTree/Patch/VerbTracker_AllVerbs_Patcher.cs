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
            "get_PrimaryVerb"
        )]
        private static bool PreVerbTracker_PrimaryVerb(VerbTracker __instance, ref Verb __result)
        {
            foreach(Verb verb in __instance.AllVerbs)
            {
                if (verb.verbProps.isPrimary)
                {
                    __result = verb;
                    return false;
                }
            }
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(VerbTracker),
            "get_AllVerbs"
        )]
        private static void PostVerbTracker_AllVerbs(VerbTracker __instance, ref List<Verb> __result)
        {
            __result = new List<Verb>(__result);
            CompChildNodeProccesser proccess = (((__instance.directOwner) as ThingComp)?.parent) ?? ((__instance.directOwner) as Thing);
            __result = proccess?.PostVerbTracker_AllVerbs(__instance, __result) ?? __result;
        }
    }
}

namespace RW_NodeTree
{
    /// <summary>
    /// Node function proccesser
    /// </summary>
    public partial class CompChildNodeProccesser : ThingComp, IThingHolder
    {

        /// <summary>
        /// event proccesser after VerbTracker.AllVerbs
        /// (WARRING!!!: Don't invoke any method if thet will invoke VerbTracker.AllVerbs)
        /// </summary>
        /// <param name="verbTracker">VerbTracker instance</param>
        /// <param name="result">result of VerbTracker.AllVerbs</param>
        public List<Verb> PostVerbTracker_AllVerbs(VerbTracker verbTracker, List<Verb> result)
        {
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                result = comp.PostVerbTracker_AllVerbs(verbTracker, result) ?? result;
            }
            return result;
        }
    }
    public abstract partial class CompBasicNodeComp : ThingComp
    {

        public virtual List<Verb> PostVerbTracker_AllVerbs(VerbTracker verbTracker, List<Verb> result)
        {
            return result;
        }

    }
}
