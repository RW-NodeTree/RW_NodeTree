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
            "get_DirectOwner"
        )]
        private static void PostVerb_DirectOwner(Verb __instance, ref IVerbOwner __result)
        {
            Thing thing = (__result as Thing) ?? (__result as ThingComp)?.parent;
            CompChildNodeProccesser compChild = ((CompChildNodeProccesser)thing) ?? (thing?.ParentHolder as CompChildNodeProccesser);
            if (compChild != null && compChild.Props.VerbDirectOwnerRedictory)
            {
                thing = compChild.GetBeforeConvertVerbCorrespondingThing(__result.GetType(), __instance, true).Item1 ?? thing;
                __result = CompChildNodeProccesser.GetSameTypeVerbOwner(__result.GetType(), thing) ?? __result;
            }
        }



        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(Verb),
            "get_EquipmentSource"
        )]
        private static void PostVerb_EquipmentSource(Verb __instance, ref ThingWithComps __result)
        {
            IVerbOwner directOwner = __instance.verbTracker?.directOwner;
            Thing thing = (directOwner as Thing) ?? (directOwner as ThingComp)?.parent;
            CompChildNodeProccesser compChild = ((CompChildNodeProccesser)thing) ?? (thing?.ParentHolder as CompChildNodeProccesser);
            if (compChild != null && compChild.Props.VerbEquipmentSourceRedictory)
            {
                __result = (compChild.GetBeforeConvertVerbCorrespondingThing(__instance.verbTracker.directOwner.GetType(), __instance).Item1 as ThingWithComps) ?? __result;
            }
        }


        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(Verb),
            "get_UIIcon"
        )]
        private static void PostVerb_UIIcon(Verb __instance, ref Texture2D __result)
        {
            Thing EquipmentSource = __instance.EquipmentSource;
            CompChildNodeProccesser compChild = EquipmentSource;
            if (compChild != null && compChild.Props.VerbIconVerbInstanceSource)
            {
                IVerbOwner directOwner = __instance.verbTracker?.directOwner;
                EquipmentSource = (directOwner as Thing) ?? (directOwner as ThingComp)?.parent;
                compChild = ((CompChildNodeProccesser)EquipmentSource)?? (EquipmentSource?.ParentHolder as CompChildNodeProccesser);
                EquipmentSource = (compChild?.GetBeforeConvertVerbCorrespondingThing(__instance.verbTracker?.directOwner.GetType(), __instance, compChild.Props.VerbIconVerbInstanceSource).Item1 as ThingWithComps) ?? EquipmentSource;
            }
            __result = (EquipmentSource?.Graphic?.MatSingleFor(EquipmentSource)?.mainTexture as Texture2D) ?? __result;
        }
    }
}
