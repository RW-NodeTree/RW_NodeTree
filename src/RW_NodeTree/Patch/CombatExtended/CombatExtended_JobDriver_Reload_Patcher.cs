using HarmonyLib;
using RW_NodeTree.Tools;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using Verse.AI;

namespace RW_NodeTree.Patch.CombatExtended
{
    internal static class CombatExtended_JobDriver_Reload_Patcher
    {
        private static MethodInfo _PostJobDriver_Reload_weapon = typeof(CombatExtended_JobDriver_Reload_Patcher).GetMethod("PostJobDriver_Reload_weapon", BindingFlags.Static | BindingFlags.NonPublic);
        private static MethodInfo _PostJobDriver_Reload_compReloader = typeof(CombatExtended_JobDriver_Reload_Patcher).GetMethod("PostJobDriver_Reload_compReloader", BindingFlags.Static | BindingFlags.NonPublic);
        private static Type CombatExtended_JobDriver_Reload = GenTypes.GetTypeInAnyAssembly("CombatExtended.JobDriver_Reload");
        private static Type CombatExtended_CompAmmoUser = GenTypes.GetTypeInAnyAssembly("CombatExtended.CompAmmoUser");

        private static MethodInfo JobDriver_Reload_weapon = null;
        private static void PostJobDriver_Reload_weapon(ref ThingWithComps __result)
        {
            __result = __result.RootNode()?.parent ?? __result;
        }

        private static void PostJobDriver_Reload_compReloader(JobDriver __instance, ref ThingComp __result)
        {
            CompChildNodeProccesser Proccesser = ((CompChildNodeProccesser)__instance.job.targetB.Thing) ?? (__instance.job.targetB.Thing?.ParentHolder as CompChildNodeProccesser);
            if(Proccesser != null)
            {
                List<ThingComp> comps = (__instance.job.targetB.Thing as ThingWithComps)?.AllComps;
                if (comps != null)
                {
                    foreach (ThingComp comp in comps)
                    {
                        if (CombatExtended_CompAmmoUser.IsAssignableFrom(comp.GetType()))
                        {
                            __result = comp;
                            return;
                        }
                    }
                }
            }
        }


        public static void PatchJobDriver_Reload(Harmony patcher)
        {
            if (CombatExtended_JobDriver_Reload != null && CombatExtended_CompAmmoUser != null)
            {
                JobDriver_Reload_weapon = CombatExtended_JobDriver_Reload.GetMethod("get_weapon", BindingFlags.Instance | BindingFlags.NonPublic);
                MethodInfo target = JobDriver_Reload_weapon;
                patcher.Patch(
                    target,
                    postfix: new HarmonyMethod(_PostJobDriver_Reload_weapon)
                    );
                target = CombatExtended_JobDriver_Reload.GetMethod("get_compReloader", BindingFlags.Instance | BindingFlags.NonPublic);
                patcher.Patch(
                    target,
                    postfix: new HarmonyMethod(_PostJobDriver_Reload_compReloader)
                    );
            }
        }
    }
}
