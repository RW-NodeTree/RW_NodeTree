using HarmonyLib;
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
        private static void PostGraphicData_GraphicColoredFor(/**GraphicData __instance, **/Thing t, ref Graphic __result)
        {
            ((CompChildNodeProccesser?)t)?.CreateGraphic_ChildNode(ref __result);
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
        /// <returns></returns>
        internal void CreateGraphic_ChildNode(ref Graphic OrgGraphic)
        {
            OrgGraphic = OrgGraphic.GetColoredVersion(OrgGraphic.Shader, parent.DrawColor, parent.DrawColorTwo);
            if (OrgGraphic is Graphic_Linked graphic_Linked)
            {
                OrgGraphic = ref Graphic_Linked_SubGraphic(graphic_Linked);
            }
            if (OrgGraphic is Graphic_RandomRotated graphic_RandomRotated)
            {
                OrgGraphic = ref Graphic_RandomRotated_SubGraphic(graphic_RandomRotated);
            }
            OrgGraphic = new Graphic_ChildNode(this, OrgGraphic);
        }

        private static AccessTools.FieldRef<Graphic_Linked, Graphic> Graphic_Linked_SubGraphic = AccessTools.FieldRefAccess<Graphic>(typeof(Graphic_Linked), "subGraphic");
        private static AccessTools.FieldRef<Graphic_RandomRotated, Graphic> Graphic_RandomRotated_SubGraphic = AccessTools.FieldRefAccess<Graphic>(typeof(Graphic_RandomRotated), "subGraphic");
    }
}