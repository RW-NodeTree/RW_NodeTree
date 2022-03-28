using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RW_NodeTree.Patch
{
    [StaticConstructorOnStartup]
    internal static class HarmonyInjector
    {
        static HarmonyInjector()
        {
            foreach(Type t in GenTypes.AllTypes)
            {
                StatWorker_Patcher.PatchValue(t, patcher);
                StatWorker_Patcher.PatchExplanation(t, patcher);
                StatWorker_Patcher.PatchGetInfoCardHyperlinks(t, patcher);
                IVerbOwner_Patcher.PatchIVerbOwner(t, patcher);
            }
            patcher.PatchAll();
        }

        public static Harmony patcher = new Harmony("RW_NodeTree.Patch");
    }
}
