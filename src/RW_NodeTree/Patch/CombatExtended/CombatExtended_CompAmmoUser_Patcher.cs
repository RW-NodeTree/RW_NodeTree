using HarmonyLib;
using RW_NodeTree.Tools;
using System;
using System.Reflection;
using Verse;

namespace RW_NodeTree.Patch.CombatExtended
{
    internal static class CombatExtended_CompAmmoUser_Patcher
    {
        private static MethodInfo _PostCompAmmoUser_CompEquippable = typeof(CombatExtended_CompAmmoUser_Patcher).GetMethod("PostCompAmmoUser_CompEquippable", BindingFlags.Static | BindingFlags.NonPublic);
        private static Type CombatExtended_CompAmmoUser = GenTypes.GetTypeInAnyAssembly("CombatExtended.CompAmmoUser");

        private static void PostCompAmmoUser_CompEquippable(ThingComp __instance, ref CompEquippable __result)
        {
            CompChildNodeProccesser comp = __instance.parent.RootNode();
            if (comp != null)
            {
                __result = comp.parent.TryGetComp<CompEquippable>();
                //Log.Message($"log {__instance}.PostCompAmmoUser_CompEquippable");
            }
        }


        public static void PatchCompEquippable(Harmony patcher)
        {
            if (CombatExtended_CompAmmoUser != null)
            {
                MethodInfo target = CombatExtended_CompAmmoUser.GetMethod("get_CompEquippable", BindingFlags.Instance | BindingFlags.Public);
                patcher.Patch(
                    target,
                    postfix: new HarmonyMethod(_PostCompAmmoUser_CompEquippable)
                    );
            }
        }
    }
}
