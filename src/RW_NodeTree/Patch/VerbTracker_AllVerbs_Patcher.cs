﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Threading;
using Verse;

namespace RW_NodeTree.Patch
{
    [HarmonyPatch(typeof(VerbTracker))]
    internal static partial class VerbTracker_Patcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(VerbTracker),
            "get_PrimaryVerb"
        )]
        private static void PostVerbTracker_PrimaryVerb(VerbTracker __instance, ref Verb __result)
        {
            foreach (Verb verb in __instance.AllVerbs)
            {
                if (verb.verbProps.isPrimary)
                {
                    __result = verb;
                    return;
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(VerbTracker),
            "get_AllVerbs"
        )]
        private static void PreVerbTracker_AllVerbs(VerbTracker __instance, ref CompChildNodeProccesser __state)
        {
            __state = ((__instance.directOwner as ThingComp)?.parent) ?? ((__instance.directOwner) as Thing);
            __state?.UpdateNode();
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(VerbTracker),
            "get_AllVerbs"
        )]
        private static void PostVerbTracker_AllVerbs(VerbTracker __instance, ref List<Verb> __result, ref CompChildNodeProccesser __state)
        {
            __result = __state?.PostVerbTracker_AllVerbs(__instance.directOwner?.GetType(), new List<Verb>(__result)) ?? __result;
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

        private bool GetOriginalVerb
        {
            get
            {
                bool result;
                int current = Thread.CurrentThread.ManagedThreadId;
                lock(notSetVerbDirectOwner)
                {
                    if (!notSetVerbDirectOwner.TryGetValue(current, out result))
                    {
                        notSetVerbDirectOwner.Add(current, false);
                    }
                }
                return result;
            }
            set
            {
                lock(notSetVerbDirectOwner)
                {
                    notSetVerbDirectOwner.SetOrAdd(Thread.CurrentThread.ManagedThreadId, value);
                }
            }
        }

        /// <summary>
        /// Get all original verbs of verbTracker
        /// </summary>
        /// <param name="verbTracker">target VerbTracker</param>
        /// <returns>all verbs before parent convert</returns>
        public static List<Verb> GetOriginalAllVerbs(VerbTracker verbTracker)
        {
            if(verbTracker != null)
            {
                CompChildNodeProccesser proccess = (((verbTracker.directOwner) as ThingComp)?.parent) ?? ((verbTracker.directOwner) as Thing);
                if (proccess != null) proccess.GetOriginalVerb = true;
                List<Verb> result = verbTracker.AllVerbs;
                if (proccess != null) proccess.GetOriginalVerb = false;
                return result;
            }
            return null;
        }

        /// <summary>
        /// Get original primary verbs of verbTracker
        /// </summary>
        /// <param name="verbTracker">target VerbTracker</param>
        /// <returns>all verbs before parent convert</returns>
        public static Verb GetOriginalPrimaryVerbs(VerbTracker verbTracker)
        {
            if (verbTracker != null)
            {
                CompChildNodeProccesser proccess = (((verbTracker.directOwner) as ThingComp)?.parent) ?? ((verbTracker.directOwner) as Thing);
                if (proccess != null) proccess.GetOriginalVerb = true;
                Verb result = verbTracker.PrimaryVerb;
                if (proccess != null) proccess.GetOriginalVerb = false;
                return result;
            }
            return null;
        }

        /// <summary>
        /// event proccesser after VerbTracker.AllVerbs
        /// (WARRING!!!: Don't invoke any method if thet will invoke VerbTracker.AllVerbs)
        /// </summary>
        /// <param name="ownerType">VerbTracker instance</param>
        /// <param name="result">result of VerbTracker.AllVerbs</param>
        internal List<Verb> PostVerbTracker_AllVerbs(Type ownerType, List<Verb> result)
        {
            //StackTrace stackTrace = new StackTrace();
            //StackFrame[] stackFrame = stackTrace.GetFrames();
            //if (Prefs.DevMode) Log.Message($"{stackFrame[0].GetMethod()}\n{stackFrame[1].GetMethod()}\n{stackFrame[2].GetMethod()}\n{stackFrame[3].GetMethod()}\n{stackFrame[4].GetMethod()}\n{stackFrame[5].GetMethod()}\n{stackFrame[6].GetMethod()}\n");
            if (Props.VerbTrackerAllVerbRedictory && ownerType != null && typeof(IVerbOwner).IsAssignableFrom(ownerType) && !GetOriginalVerb)
            {
                for (int i = 0; i < result.Count; i++)
                {
                    result[i] = GetAfterConvertVerbCorrespondingThing(ownerType, result[i]).Item2 ?? result[i];
                }
                result.RemoveAll(x => x == null || x.verbProps == null);
                return result;
            }
            return null;
        }

        private readonly Dictionary<int, bool> notSetVerbDirectOwner = new Dictionary<int, bool>();
    }
}
