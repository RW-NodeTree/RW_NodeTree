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
            __result = ((CompChildNodeProccesser)t)?.CreateGraphic_ChildNode(__result, __instance) ?? __result;
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
        /// create Graphic_ChildNode and insert into the Graphic nestification;
        /// </summary>
        /// <param name="OrgGraphic"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public Graphic CreateGraphic_ChildNode(Graphic OrgGraphic, GraphicData data)
        {
            Graphic_Linked graphic_Linked = OrgGraphic as Graphic_Linked;
            if (graphic_Linked != null)
            {
                OrgGraphic = Graphic_Linked_SubGraphic(graphic_Linked);
            }
            Graphic_RandomRotated graphic_RandomRotated = OrgGraphic as Graphic_RandomRotated;
            if (graphic_RandomRotated != null)
            {
                OrgGraphic = Graphic_RandomRotated_SubGraphic(graphic_RandomRotated);
            }
            OrgGraphic = new Graphic_ChildNode(this, OrgGraphic);
            if (graphic_RandomRotated != null) OrgGraphic = new Graphic_RandomRotated(OrgGraphic, Graphic_RandomRotated_MaxAngle(graphic_RandomRotated));
            if (data.Linked) OrgGraphic = GraphicUtility.WrapLinked(OrgGraphic, data.linkType);
            return OrgGraphic;
        }

        private static AccessTools.FieldRef<Graphic_Linked, Graphic> Graphic_Linked_SubGraphic = AccessTools.FieldRefAccess<Graphic>(typeof(Graphic_Linked), "subGraphic");
        private static AccessTools.FieldRef<Graphic_RandomRotated, Graphic> Graphic_RandomRotated_SubGraphic = AccessTools.FieldRefAccess<Graphic>(typeof(Graphic_RandomRotated), "subGraphic");
        private static AccessTools.FieldRef<Graphic_RandomRotated, float> Graphic_RandomRotated_MaxAngle = AccessTools.FieldRefAccess<float>(typeof(Graphic_RandomRotated), "maxAngle");
    }
}