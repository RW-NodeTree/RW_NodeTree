using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
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
            CompChildNodeProccesser proccess = ((__instance.directOwner as ThingComp)?.parent) ?? ((__instance.directOwner) as Thing);
            foreach (Verb verb in __instance.AllVerbs)
            {
                if (verb.verbProps.isPrimary)
                {
                    __result = verb;
                    return;
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(VerbTracker),
            "get_AllVerbs"
        )]
        private static void PostVerbTracker_AllVerbs(VerbTracker __instance, ref List<Verb> __result)
        {
            CompChildNodeProccesser proccess = ((__instance.directOwner as ThingComp)?.parent) ?? ((__instance.directOwner) as Thing);
            __result = proccess?.PostVerbTracker_AllVerbs(__instance.directOwner?.GetType(), new List<Verb>(__result)) ?? __result;
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
        public static List<Verb> GetAllOriginalVerbs(VerbTracker verbTracker)
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
            if (ownerType != null && typeof(IVerbOwner).IsAssignableFrom(ownerType) && !GetOriginalVerb)
            {
                for (int i = 0; i < result.Count; i++)
                {
                    Verb verb = result[i];
                    IVerbOwner verbOwner = verb.DirectOwner;
                    if (verbOwner != null && (verbOwner == parent || (verbOwner as ThingComp)?.parent == parent))
                    {
                        verb = GetAfterConvertVerbCorrespondingThing(ownerType, verb).Item2;
                        Thing thing = this.GetBeforeConvertVerbCorrespondingThing(ownerType, result[i]).Item1;
                        verbOwner = GetSameTypeVerbOwner(ownerType, thing);
                        if (verbOwner != null)
                        {
                            verb.verbTracker = verbOwner.VerbTracker;
                            //if (Prefs.DevMode) Log.Message(verb + " : " + thing);
                        }
                    }
                    result[i] = verb;
                }
                return result;
            }
            return null;
        }

        private readonly Dictionary<int, bool> notSetVerbDirectOwner = new Dictionary<int, bool>();
    }
}
