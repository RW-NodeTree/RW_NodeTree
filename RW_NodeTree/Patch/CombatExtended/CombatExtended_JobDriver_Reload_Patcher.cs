using HarmonyLib;
using RimWorld;
using RW_NodeTree.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RW_NodeTree.Patch.CombatExtended
{
    internal static class CombatExtended_JobDriver_Reload_Patcher
    {
        private static MethodInfo _PostJobDriver_Reload_weapon = typeof(CombatExtended_JobDriver_Reload_Patcher).GetMethod("PostJobDriver_Reload_weapon", BindingFlags.Static | BindingFlags.NonPublic);
        private static MethodInfo _JobDriver_Reload_compReloader_Transpiler = typeof(CombatExtended_JobDriver_Reload_Patcher).GetMethod("JobDriver_Reload_compReloader_Transpiler", BindingFlags.Static | BindingFlags.NonPublic);
        private static Type CombatExtended_JobDriver_Reload = GenTypes.GetTypeInAnyAssembly("CombatExtended.JobDriver_Reload");

        private static MethodInfo JobDriver_Reload_weapon = null;
        private static MethodInfo JobDriver_TargetThingB = typeof(JobDriver).GetMethod("get_TargetThingB", BindingFlags.Instance | BindingFlags.NonPublic);
        private static void PostJobDriver_Reload_weapon(ref ThingWithComps __result)
        {
            __result = __result.RootNode()?.parent ?? __result;
        }

        private static IEnumerable<CodeInstruction> JobDriver_Reload_compReloader_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach(CodeInstruction instruction in instructions)
            {
                if(instruction.Calls(JobDriver_Reload_weapon)) yield return new CodeInstruction(OpCodes.Call, JobDriver_TargetThingB);
                else yield return instruction;
            }
        }


        public static void PatchJobDriver_Reload(Harmony patcher)
        {
            if (CombatExtended_JobDriver_Reload != null)
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
                    transpiler: new HarmonyMethod(_JobDriver_Reload_compReloader_Transpiler)
                    );
            }
        }
    }
}
