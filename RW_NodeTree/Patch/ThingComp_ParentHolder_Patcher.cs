using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RW_NodeTree.Patch
{
    [HarmonyPatch(typeof(ThingComp))]
    internal static class ThingComp_ParentHolder_Patcher
    {


        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(ThingComp),
            "get_ParentHolder"
        )]
        private static void PostThingComp_ParentHolder(ThingComp __instance, ref IThingHolder __result)
        {
            CompChildNodeProccesser comp = __instance as CompChildNodeProccesser;
            if (comp != null && CompChildNodeProccesser.RenderingKey != null)
            {
                __result = ((CompChildNodeProccesser)comp.ChildNodes.InternalParent);
            }
        }
    }
}
