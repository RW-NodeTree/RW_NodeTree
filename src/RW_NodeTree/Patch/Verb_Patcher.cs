﻿using HarmonyLib;
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
            "get_DirectOwner"
        )]
        private static void PostVerb_DirectOwner(Verb __instance, ref IVerbOwner __result)
        {
            Thing? thing = (__result as Thing) ?? (__result as ThingComp)?.parent;
            CompChildNodeProccesser? compChild = ((CompChildNodeProccesser?)thing) ?? (thing?.ParentHolder as CompChildNodeProccesser);
            if (compChild != null && compChild.Props.VerbDirectOwnerRedictory && CompChildNodeProccesser.GetSameTypeVerbOwner(__result.GetType(), thing!) == __instance)
            {
                __result = compChild.GetBeforeConvertThingWithVerb(__result.GetType(), __instance, true).Item2?.DirectOwner ?? __result;
            }
        }



        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(Verb),
            "get_EquipmentSource"
        )]
        private static void PostVerb_EquipmentSource(Verb __instance, ref ThingWithComps __result)
        {
            IVerbOwner? directOwner = __instance.verbTracker?.directOwner;
            Thing? thing = (directOwner as Thing) ?? (directOwner as ThingComp)?.parent;
            CompChildNodeProccesser? compChild = ((CompChildNodeProccesser?)thing) ?? (thing?.ParentHolder as CompChildNodeProccesser);
            if (compChild != null && directOwner != null && compChild.Props.VerbEquipmentSourceRedictory && CompChildNodeProccesser.GetSameTypeVerbOwner(directOwner.GetType(), thing!) == directOwner)
            {
                __result = (compChild.GetBeforeConvertThingWithVerb(directOwner.GetType(), __instance).Item1 as ThingWithComps) ?? __result;
            }
        }


        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(Verb),
            "get_UIIcon"
        )]
        private static void PostVerb_UIIcon(Verb __instance, ref Texture2D __result)
        {
            IVerbOwner? directOwner = __instance.verbTracker?.directOwner;
            Thing? thing = (directOwner as Thing) ?? (directOwner as ThingComp)?.parent;
            Thing? EquipmentSource = __instance.EquipmentSource;
            CompChildNodeProccesser? compChild = EquipmentSource;
            if (compChild != null && directOwner != null && compChild.Props.VerbIconVerbInstanceSource && CompChildNodeProccesser.GetSameTypeVerbOwner(directOwner.GetType(), thing!) == directOwner)
            {
                EquipmentSource = thing;
                compChild = ((CompChildNodeProccesser?)EquipmentSource) ?? (EquipmentSource?.ParentHolder as CompChildNodeProccesser);
                EquipmentSource = (compChild?.GetBeforeConvertThingWithVerb(directOwner.GetType(), __instance).Item1 as ThingWithComps) ?? EquipmentSource;
            }
            __result = (EquipmentSource?.Graphic?.MatSingleFor(EquipmentSource)?.mainTexture as Texture2D) ?? __result;
        }
    }
}
