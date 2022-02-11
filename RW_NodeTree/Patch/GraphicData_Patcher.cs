using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RW_NodeTree.Patch
{
    [HarmonyPatch(typeof(GraphicData))]
    internal static class GraphicData_Patcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(GraphicData),
            "GraphicColoredFor",
            typeof(Thing)
        )]
        private static void PostGraphicData_GraphicColoredFor(GraphicData __instance, Thing t,ref Graphic __result)
        {

            Comp_ChildNodeProccesser comp_ChildNodeProccesser = t;
            __result = comp_ChildNodeProccesser?.CreateGraphic_ChildNode(__result, __instance) ?? __result;
        }

    }
}
