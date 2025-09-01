﻿using HarmonyLib;
using System;
using System.Reflection;
using Verse;

namespace RW_NodeTree.Patch
{
    [StaticConstructorOnStartup]
    internal static class HarmonyInjector
    {
        static HarmonyInjector()
        {
            foreach (Type t in GenTypes.AllTypes)
            {
                StatWorker_Patcher.PatchValue(t, patcher);
                StatWorker_Patcher.PatchExplanation(t, patcher);
                StatWorker_Patcher.PatchGetInfoCardHyperlinks(t, patcher);
                StatWorker_Patcher.PatchShouldShowForAndIsDisabledFor(t, patcher);
                StatWorker_Patcher.PatchStatDrawEntry(t, patcher);
            }
            patcher.PatchAll();
        }

        public static Harmony patcher = new Harmony("RW_NodeTree.Patch");
        public static Assembly coreAssembly = typeof(Thing).Assembly;
    }
}
