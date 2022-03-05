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
    [HarmonyPatch(typeof(Verb))]
    internal static class Verb_Patcher
    {

        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(Verb),
            "get_UIIcon"
        )]
        private static void PreVerb_UIIcon(Verb __instance, ref Texture2D __state)
        {

            Thing eq = __instance.EquipmentSource;
            __state = eq.def.uiIcon;
            eq.def.uiIcon = (eq?.Graphic?.MatSingleFor(eq)?.mainTexture as Texture2D) ?? __state;
        }


        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(Verb),
            "get_UIIcon"
        )]
        private static void PostVerb_UIIcon(Verb __instance, ref Texture2D __state)
        {
            __instance.EquipmentSource.def.uiIcon = __state;
        }
    }
}
