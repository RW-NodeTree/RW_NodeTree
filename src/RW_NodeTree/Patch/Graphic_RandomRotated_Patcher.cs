using HarmonyLib;
using RW_NodeTree.Tools;
using UnityEngine;
using Verse;

namespace RW_NodeTree.Patch
{
    [HarmonyPatch(typeof(Graphic))]
    internal static class Graphic_Patcher
    {

        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(Graphic),
            "MeshAt"
        )]
        private static void PreGraphic_MeshAt(Graphic __instance, Rot4 rot)
        {
            __instance.GetGraphic_ChildNode()?.MatAt(rot);
        }
    }
}
