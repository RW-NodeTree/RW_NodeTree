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
        private static void PreGraphicData_GraphicColoredFor(GraphicData __instance, Thing t,ref Graphic __result)
        {

            Comp_ChildNodeProccesser comp_ChildNodeProccesser = t;
            if (comp_ChildNodeProccesser != null)
            {
                Graphic_ChildNode graphic = new Graphic_ChildNode();
                graphic._THIS = t;
                graphic._GRAPHIC = __result;
                __result = graphic;
            }
        }
    }
}
