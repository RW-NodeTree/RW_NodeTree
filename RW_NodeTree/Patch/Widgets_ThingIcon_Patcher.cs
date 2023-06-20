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
            typeof(Rot4?)
#if !V13
            ,typeof(bool)
#endif
        )]
        private static void PreWidgets_ThingIcon(Thing thing, ref (Vector2, float)? __state)
        {
            ThingStyleDef styleDef = thing.StyleDef;
            if (thing?.def?.graphicData != null && (styleDef == null || styleDef.UIIcon == null) && thing.def.uiIconPath.NullOrEmpty() && !(thing is Pawn || thing is Corpse))
            {
                CompChildNodeProccesser proccesser = thing;
                if(proccesser != null)
                {
                    Rot4 defaultPlacingRot = thing.def.defaultPlacingRot;
                    ref Vector2 drawSize = ref thing.def.graphicData.drawSize;
                    ref float scale = ref thing.def.uiIconScale;
                    __state = (drawSize, scale);
                    drawSize = proccesser.parent.Graphic.drawSize;
                    if (defaultPlacingRot.IsHorizontal)
                    {
                        drawSize = drawSize.Rotated();
                        Vector2 scaleCalc = drawSize / thing.def.size.ToVector2().Rotated();
                        scale *= Math.Max(scaleCalc.x, scaleCalc.y);
                    }
                    else
                    {
                        Vector2 scaleCalc = drawSize / thing.def.size.ToVector2();
                        scale *= Math.Max(scaleCalc.x, scaleCalc.y);
                    }
                    //Log.Message($"thing.def.graphicData.drawSize : {drawSize}; cache : {__state}");
                }
            }
        }


        [HarmonyFinalizer]
        [HarmonyPatch(
            typeof(Widgets),
            "ThingIcon",
            typeof(Rect),
            typeof(Thing),
            typeof(float),
            typeof(Rot4?)
#if !V13
            ,typeof(bool)
#endif
        )]
        private static void FinalWidgets_ThingIcon(Thing thing, (Vector2, float)? __state)
        {
            if (thing?.def?.graphicData != null && __state.HasValue)
            {
                thing.def.graphicData.drawSize = __state.Value.Item1;
                thing.def.uiIconScale = __state.Value.Item2;
            }
        }
    }
}
