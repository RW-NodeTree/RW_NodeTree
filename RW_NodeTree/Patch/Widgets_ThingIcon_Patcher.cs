using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RW_NodeTree.Patch
{
    [HarmonyPatch(typeof(Widgets))]
    internal static class Widgets_ThingIcon_Patcher
    {

        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(Widgets),
            "ThingIcon",
            typeof(Rect),
            typeof(Thing),
            typeof(float),
            typeof(Rot4?),
            typeof(bool)
        )]
        private static void PreWidgets_ThingIcon(Thing thing, ref Vector2? __state)
        {
            ThingStyleDef styleDef = thing.StyleDef;
            if (thing?.def?.graphicData != null && (styleDef == null || styleDef.UIIcon == null) && thing.def.uiIconPath.NullOrEmpty() && !(thing is Pawn || thing is Corpse))
            {
                CompChildNodeProccesser proccesser = thing;
                if(proccesser != null)
                {
                    Rot4 defaultPlacingRot = thing.def.defaultPlacingRot;
                    ref Vector2 drawSize = ref thing.def.graphicData.drawSize;
                    __state = drawSize;
                    drawSize = proccesser.GetAndUpdateDrawSize(defaultPlacingRot);
                    if (defaultPlacingRot.IsHorizontal) drawSize = drawSize.Rotated();
                    //Log.Message($"thing.def.graphicData.drawSize : {drawSize}; cache : {__state}");
                }
            }
        }


        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(Widgets),
            "ThingIcon",
            typeof(Rect),
            typeof(Thing),
            typeof(float),
            typeof(Rot4?),
            typeof(bool)
        )]
        private static void PostWidgets_ThingIcon(Thing thing, ref Vector2? __state)
        {
            if (thing?.def?.graphicData != null && __state.HasValue)
            {
                thing.def.graphicData.drawSize = __state.Value;
            }
        }
    }
}
