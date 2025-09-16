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
                IStatsToDrawPatcher? proccesser = thing as IStatsToDrawPatcher;
                __result = proccesser?.PostStatsReportUtility_StatsToDraw(thing, __result)?.ToList() ?? __result;
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
    public partial interface IStatsToDrawPatcher
    {
        IEnumerable<StatDrawEntry> PostStatsReportUtility_StatsToDraw(Thing thing, IEnumerable<StatDrawEntry> result);
    }
}