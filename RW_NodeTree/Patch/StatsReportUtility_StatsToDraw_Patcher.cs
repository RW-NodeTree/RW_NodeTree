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
        public IEnumerable<StatDrawEntry> PostStatsReportUtility_StatsToDraw(Thing thing, IEnumerable<StatDrawEntry> result)
        {
            foreach (ThingComp_BasicNodeComp comp in AllNodeComp)
            {
                result = comp.PostThingDef_SpecialDisplayStats(thing, result) ?? result;
            }
            return result;
        }
    }
    public abstract partial class ThingComp_BasicNodeComp : ThingComp
    {
        public virtual IEnumerable<StatDrawEntry> PostThingDef_SpecialDisplayStats(Thing thing, IEnumerable<StatDrawEntry> result)
        {
            return result;
        }
    }
}