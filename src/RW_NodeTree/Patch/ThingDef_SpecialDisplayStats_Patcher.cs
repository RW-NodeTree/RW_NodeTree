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
            IThingDefStatsPatcher? processer = req.Thing as IThingDefStatsPatcher;
            __result = processer?.PostThingDef_SpecialDisplayStats(__instance, __result) ?? __result;
        }
    }
    
    public partial interface IThingDefStatsPatcher
    {
        IEnumerable<StatDrawEntry> PostThingDef_SpecialDisplayStats(ThingDef def, IEnumerable<StatDrawEntry> result);
    }
}