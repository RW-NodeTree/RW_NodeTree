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
                IStatsToDrawPatcher? processer = thing as IStatsToDrawPatcher;
                __result = processer?.PostStatsReportUtility_StatsToDraw(__result)?.ToList() ?? __result;
            }
            catch (Exception ex)
            {
                Log.Message(ex.ToString());
            }
        }
    }
    
    public partial interface IStatsToDrawPatcher
    {
        IEnumerable<StatDrawEntry> PostStatsReportUtility_StatsToDraw(IEnumerable<StatDrawEntry> result);
    }
}