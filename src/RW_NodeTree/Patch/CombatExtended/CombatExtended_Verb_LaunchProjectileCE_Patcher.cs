using HarmonyLib;
using System;
using System.Reflection;
using Verse;

namespace RW_NodeTree.Patch.CombatExtended
{
    internal static class CombatExtended_Verb_LaunchProjectileCE_Patcher
    {
        private static MethodInfo _PostPostVerb_LaunchProjectileCE_ShotsPerBurst = typeof(CombatExtended_Verb_LaunchProjectileCE_Patcher).GetMethod("PostVerb_LaunchProjectileCE_ShotsPerBurst", BindingFlags.Static | BindingFlags.NonPublic);
        private static Type CombatExtended_Verb_LaunchProjectileCE = GenTypes.GetTypeInAnyAssembly("CombatExtended.Verb_LaunchProjectileCE");

        private static void PostVerb_LaunchProjectileCE_ShotsPerBurst(Verb __instance, ref int __result)
        {
            CompChildNodeProccesser comp = (__instance.verbTracker?.directOwner as ThingComp)?.parent;
            if (comp != null)
            {
                __result = __instance.verbProps.burstShotCount;
                //Log.Message($"log {__instance}.ShotsPerBurst = {__result}");
            }
        }


        public static void PatchVerb_LaunchProjectileCE(Harmony patcher)
        {
            if (CombatExtended_Verb_LaunchProjectileCE != null)
            {
                MethodInfo target = CombatExtended_Verb_LaunchProjectileCE.GetMethod("get_ShotsPerBurst", BindingFlags.Instance | BindingFlags.Public);
                patcher.Patch(
                    target,
                    postfix: new HarmonyMethod(_PostPostVerb_LaunchProjectileCE_ShotsPerBurst)
                    );
            }
        }
    }
}
