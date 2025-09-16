using HarmonyLib;
using Verse;

namespace RW_NodeTree.Patch
{
    [HarmonyPatch(typeof(GraphicData))]
    internal static class GraphicData_Patcher
    {        private static AccessTools.FieldRef<Graphic_Linked, Graphic> Graphic_Linked_SubGraphic = AccessTools.FieldRefAccess<Graphic>(typeof(Graphic_Linked), "subGraphic");
        private static AccessTools.FieldRef<Graphic_RandomRotated, Graphic> Graphic_RandomRotated_SubGraphic = AccessTools.FieldRefAccess<Graphic>(typeof(Graphic_RandomRotated), "subGraphic");

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(GraphicData),
            "GraphicColoredFor",
            typeof(Thing)
        )]
        private static void PostGraphicData_GraphicColoredFor(/**GraphicData __instance, **/Thing t, ref Graphic __result)
        {
            INodeProccesser? proccesser = t as INodeProccesser;
            if (proccesser != null && __result != null)
            {
                __result = __result.GetColoredVersion(__result.Shader, t.DrawColor, t.DrawColorTwo);
                if (__result is Graphic_Linked graphic_Linked)
                {
                    __result = ref Graphic_Linked_SubGraphic(graphic_Linked);
                }
                if (__result is Graphic_RandomRotated graphic_RandomRotated)
                {
                    __result = ref Graphic_RandomRotated_SubGraphic(graphic_RandomRotated);
                }
                __result = new Graphic_ChildNode(t, __result);
            }
        }

    }
}