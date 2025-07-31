using HarmonyLib;
using RimWorld;
using RW_NodeTree.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RW_NodeTree.Patch
{

    [HarmonyPatch(typeof(StatsReportUtility))]
    internal static partial class StatsReportUtility_Patcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(StatsReportUtility),
            "StatsToDraw",
            typeof(Thing)
            )]
        public static void PostStatsReportUtility_StatsToDraw(Thing thing, ref IEnumerable<StatDrawEntry> __result)
        {
            try
            {
                __result = thing.RootNode()?.PostStatsReportUtility_StatsToDraw(thing, __result)?.ToList() ?? __result;
            }
            catch (Exception ex)
            {
                Log.Message(ex.ToString());
            }
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
        /// <param name="thing">parm 'thing' of StatsReportUtility.StatsToDraw()</param>
        /// <param name="result">result of ThingDef.SpecialDisplayStats</param>
        internal IEnumerable<StatDrawEntry> PostStatsReportUtility_StatsToDraw(Thing thing, IEnumerable<StatDrawEntry> result)
        {
            UpdateNode();
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                try
                {
                    result = comp.internal_PostStatsReportUtility_StatsToDraw(thing, result) ?? result;
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
        protected virtual IEnumerable<StatDrawEntry> PostStatsReportUtility_StatsToDraw(Thing thing, IEnumerable<StatDrawEntry> result)
        {
            return result;
        }
        internal IEnumerable<StatDrawEntry> internal_PostStatsReportUtility_StatsToDraw(Thing thing, IEnumerable<StatDrawEntry> result)
            => PostStatsReportUtility_StatsToDraw(thing, result);
    }
}