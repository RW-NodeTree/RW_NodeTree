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
            __result = proccess?.PostVerbTracker_AllVerbs(__instance.directOwner?.GetType(), __result) ?? __result;
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

        private bool NotSetVerbDirectOwner
        {
            get
            {
                int current = Thread.CurrentThread.ManagedThreadId;

                bool result;
                if (!notSetVerbDirectOwner.TryGetValue(current, out result))
                {
                    notSetVerbDirectOwner.Add(current, false);
                }
                return result;
            }
            set
            {
                notSetVerbDirectOwner.SetOrAdd(Thread.CurrentThread.ManagedThreadId, value);
            }
        }

        /// <summary>
        /// event proccesser after VerbTracker.AllVerbs
        /// (WARRING!!!: Don't invoke any method if thet will invoke VerbTracker.AllVerbs)
        /// </summary>
        /// <param name="verbTracker">VerbTracker instance</param>
        /// <param name="result">result of VerbTracker.AllVerbs</param>
        public List<Verb> PostVerbTracker_AllVerbs(Type ownerType, List<Verb> result)
        {
            //StackTrace stackTrace = new StackTrace();
            //StackFrame[] stackFrame = stackTrace.GetFrames();
            //if (Prefs.DevMode) Log.Message($"{stackFrame[0].GetMethod()}\n{stackFrame[1].GetMethod()}\n{stackFrame[2].GetMethod()}\n{stackFrame[3].GetMethod()}\n{stackFrame[4].GetMethod()}\n{stackFrame[5].GetMethod()}\n{stackFrame[6].GetMethod()}\n");
            if (ownerType != null && typeof(IVerbOwner).IsAssignableFrom(ownerType))
            {
                foreach (CompBasicNodeComp comp in AllNodeComp)
                {
                    result = comp.PostVerbTracker_AllVerbs(ownerType, result) ?? result;
                }
                if(!NotSetVerbDirectOwner)
                {
                    for (int i = 0; i < result.Count; i++)
                    {
                        Verb verb = result[i];
                        IVerbOwner verbOwner = verb.DirectOwner;
                        if (verbOwner != null && (verbOwner == parent || (verbOwner as ThingComp).parent == parent))
                        {
                            Verb _ = null;
                            NotSetVerbDirectOwner = true;
                            Thing thing = this.GetVerbCorrespondingThing(ownerType, ref _, ref verb);
                            NotSetVerbDirectOwner = false;
                            verbOwner = GetSameTypeVerbOwner(ownerType, thing);
                            if (verbOwner != null)
                            {
                                result[i].verbTracker = verbOwner.VerbTracker;
                            }
                        }
                    }
                }
            }
            return result;
        }

        private Dictionary<int, bool> notSetVerbDirectOwner = new Dictionary<int, bool>();
    }
    public abstract partial class CompBasicNodeComp : ThingComp
    {

        public virtual List<Verb> PostVerbTracker_AllVerbs(Type ownerType, List<Verb> result)
        {
            return result;
        }

    }
}
