using HarmonyLib;
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
            IThingDefStatsPatcher? proccesser = req.Thing as IThingDefStatsPatcher;
            __result = proccesser?.PostThingDef_SpecialDisplayStats(__instance, req, __result) ?? __result;
        }
    }
}

namespace RW_NodeTree
{
    public partial interface IThingDefStatsPatcher
    {
        IEnumerable<StatDrawEntry> PostThingDef_SpecialDisplayStats(ThingDef def, StatRequest req, IEnumerable<StatDrawEntry> result);
    }
}