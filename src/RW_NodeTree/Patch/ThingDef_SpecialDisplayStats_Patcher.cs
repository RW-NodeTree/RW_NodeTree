﻿using HarmonyLib;
using RimWorld;
using RW_NodeTree.Tools;
using System;
using System.Collections.Generic;
using Verse;

namespace RW_NodeTree.Patch
{

    [HarmonyPatch(typeof(ThingDef))]
    internal static partial class ThingDef_Patcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(ThingDef),
            "SpecialDisplayStats",
            typeof(StatRequest)
            )]
        private static void PostThingDef_SpecialDisplayStats(ThingDef __instance, StatRequest req, ref IEnumerable<StatDrawEntry> __result)
        {
            CompChildNodeProccesser? proccesser = req.Thing;
            __result = proccesser?.PostThingDef_SpecialDisplayStats(__instance, req, __result) ?? __result;
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
        /// event proccesser after ThingDef.SpecialDisplayStats
        /// (WARRING!!!: Don't invoke any method if thet will invoke ThingDef.SpecialDisplayStats)
        /// </summary>
        /// <param name="def">ThingDef instance</param>
        /// <param name="req">parm 'req' of ThingDef.SpecialDisplayStats()</param>
        /// <param name="result">result of ThingDef.SpecialDisplayStats</param>
        internal IEnumerable<StatDrawEntry> PostThingDef_SpecialDisplayStats(ThingDef def, StatRequest req, IEnumerable<StatDrawEntry> result)
        {
            UpdateNode();
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                try
                {
                    result = comp.internal_PostThingDef_SpecialDisplayStats(def, req, result) ?? result;
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
            return result;
        }
    }
    public abstract partial class CompBasicNodeComp : ThingComp
    {
        protected virtual IEnumerable<StatDrawEntry> PostThingDef_SpecialDisplayStats(ThingDef def, StatRequest req, IEnumerable<StatDrawEntry> result)
        {
            return result;
        }
        internal IEnumerable<StatDrawEntry> internal_PostThingDef_SpecialDisplayStats(ThingDef def, StatRequest req, IEnumerable<StatDrawEntry> result)
            => PostThingDef_SpecialDisplayStats(def, req, result);
    }
}