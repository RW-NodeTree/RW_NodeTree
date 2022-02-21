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
        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(Verb),
            "get_EquipmentSource"
        )]
        private static void PostVerb_EquipmentSource(Verb __instance,ref Thing __result)
        {
            Verb _ = null;
            __result = ((CompChildNodeProccesser)__result)?.GetVerbCorrespondingThing(__instance.DirectOwner, ref _, ref __instance) ?? __result;
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(Verb),
            "get_UIIcon"
        )]
        private static void PostVerb_UIIcon(Verb __instance,ref Texture2D __result)
        {
            Thing eq = __instance.EquipmentSource;
            __result = (eq?.Graphic?.MatSingleFor(eq)?.mainTexture as Texture2D) ?? __result;
        }
    }
}
