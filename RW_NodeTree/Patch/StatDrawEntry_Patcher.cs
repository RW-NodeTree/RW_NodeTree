using System;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace AssembleWeapon
{
	[HarmonyPatch(typeof(StatDrawEntry))]
	public static class StatDrawEntry_Patcher
	{
		[HarmonyPostfix]
		[HarmonyPatch(typeof(StatDrawEntry), "get_ValueString")]
		private static void PostStatDrawEntry_ValueString(StatDrawEntry __instance, string __result)
		{
			ref string valueStringInt = ref StatDrawEntry_valueStringInt(__instance);
			if (valueStringInt.NullOrEmpty())
			{
				valueStringInt = __result;
			}
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(StatDrawEntry), "GetExplanationText")]
		public static bool GetExplanationTextPostfix(StatDrawEntry __instance, ref string __result)
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
		public static void GetExplanationTextPostfix(StatDrawEntry __instance, string __result)
		{
			StatDrawEntry_explanationText(__instance) = __result;
		}

		private static AccessTools.FieldRef<StatDrawEntry, string> StatDrawEntry_valueStringInt = AccessTools.FieldRefAccess<string>(typeof(StatDrawEntry), "valueStringInt");
		private static AccessTools.FieldRef<StatDrawEntry, string> StatDrawEntry_explanationText = AccessTools.FieldRefAccess<string>(typeof(StatDrawEntry), "explanationText");
	}
}
