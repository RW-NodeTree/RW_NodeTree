using System;
using System.Collections.Generic;
using Verse;

namespace RW_NodeTree.Tools
{
    public static class VerbHelper
    {

        /// <summary>
        /// Get all original verbs of verbTracker
        /// </summary>
        /// <param name="verbTracker">target VerbTracker</param>
        /// <returns>all verbs before parent convert</returns>
        public static List<Verb?> GetOriginalAllVerbs(this VerbTracker verbTracker)
        {
            if (verbTracker == null) throw new ArgumentNullException(nameof(verbTracker));
            CompChildNodeProccesser? proccess = ((verbTracker.directOwner as ThingComp)?.parent) ?? (verbTracker.directOwner as Thing);
            if (proccess != null) proccess.GetOriginalVerb = true;
            List<Verb?> result = verbTracker.AllVerbs;
            if (proccess != null) proccess.GetOriginalVerb = false;
            return result;
        }

        /// <summary>
        /// Get original primary verbs of verbTracker
        /// </summary>
        /// <param name="verbTracker">target VerbTracker</param>
        /// <returns>all verbs before parent convert</returns>
        public static Verb? GetOriginalPrimaryVerbs(this VerbTracker verbTracker)
        {
            if (verbTracker == null) throw new ArgumentNullException(nameof(verbTracker));
            CompChildNodeProccesser? proccess = ((verbTracker.directOwner as ThingComp)?.parent) ?? (verbTracker.directOwner as Thing);
            if (proccess != null) proccess.GetOriginalVerb = true;
            Verb? result = verbTracker.PrimaryVerb;
            if (proccess != null) proccess.GetOriginalVerb = false;
            return result;
        }
    }
}
