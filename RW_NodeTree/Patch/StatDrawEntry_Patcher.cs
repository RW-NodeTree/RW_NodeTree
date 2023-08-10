using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace RW_NodeTree.Patch
{
	[HarmonyPatch(typeof(StatDrawEntry))]
    public static class StatDrawEntry_Patcher
	{
		[HarmonyPostfix]
		[HarmonyPatch(typeof(StatDrawEntry), "get_ValueString")]
        private static void PostStatDrawEntry_ValueString(StatDrawEntry __instance, string __result)
		{
			if(__instance.hasOptionalReq)
            {
                ref string valueStringInt = ref StatDrawEntry_valueStringInt(__instance);
                if (valueStringInt.NullOrEmpty())
                {
                    valueStringInt = __result;
                }
            }
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(StatDrawEntry), "GetExplanationText")]
        private static bool PreStatDrawEntry_GetExplanationText(StatDrawEntry __instance, ref string __result)
		{
			ref string explanationText = ref StatDrawEntry_explanationText(__instance);
			if (!explanationText.NullOrEmpty())
			{
				__result = explanationText;
				return false;
			}
			return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(StatDrawEntry), "GetExplanationText")]
        private static void PostStatDrawEntry_GetExplanationText(StatDrawEntry __instance, string __result)
        {
            StatDrawEntry_explanationText(__instance) = __result;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(StatDrawEntry), "GetHyperlinks")]
        private static void PostStatDrawEntry_GetHyperlinks(StatDrawEntry __instance, IEnumerable<Dialog_InfoCard.Hyperlink> __result)
        {
            StatDrawEntry_hyperlinks(__instance) = __result;
        }

        public static readonly AccessTools.FieldRef<StatDrawEntry, string> StatDrawEntry_valueStringInt = AccessTools.FieldRefAccess<string>(typeof(StatDrawEntry), "valueStringInt");
        public static readonly AccessTools.FieldRef<StatDrawEntry, string> StatDrawEntry_explanationText = AccessTools.FieldRefAccess<string>(typeof(StatDrawEntry), "explanationText");
        public static readonly AccessTools.FieldRef<StatDrawEntry, IEnumerable<Dialog_InfoCard.Hyperlink>> StatDrawEntry_hyperlinks = AccessTools.FieldRefAccess<IEnumerable<Dialog_InfoCard.Hyperlink>>(typeof(StatDrawEntry), "hyperlinks");
    }
}
