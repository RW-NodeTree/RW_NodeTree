using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            __result = ((CompChildNodeProccesser)thing)?.PostStatsReportUtility_StatsToDraw(thing, __result) ?? __result;
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
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                result = comp.internal_PostThingDef_SpecialDisplayStats(thing, result) ?? result;
            }
            return result;
        }
    }
    public abstract partial class CompBasicNodeComp : ThingComp
    {
        protected virtual IEnumerable<StatDrawEntry> PostThingDef_SpecialDisplayStats(Thing thing, IEnumerable<StatDrawEntry> result)
        {
            return result;
        }
        internal IEnumerable<StatDrawEntry> internal_PostThingDef_SpecialDisplayStats(Thing thing, IEnumerable<StatDrawEntry> result)
            => PostThingDef_SpecialDisplayStats(thing, result);
    }
}