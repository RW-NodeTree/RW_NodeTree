using HarmonyLib;
using Mono.Unix.Native;
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
    internal static class CombatExtended_CompFireModes_Patcher
    {
        private static MethodInfo _PreCompFireModes_Verb = typeof(CombatExtended_CompFireModes_Patcher).GetMethod("PreCompFireModes_Verb", BindingFlags.Static | BindingFlags.NonPublic);
        //private static MethodInfo _CompFireModes_Verb_Transpiler = typeof(CombatExtended_CompFireModes_Patcher).GetMethod("CompFireModes_Verb_Transpiler", BindingFlags.Static | BindingFlags.NonPublic);
        //private static MethodInfo _CompFireModes_TryGetComp = typeof(CombatExtended_CompFireModes_Patcher).GetMethod("CompFireModes_TryGetComp", BindingFlags.Static | BindingFlags.NonPublic);
        private static Type CombatExtended_CompFireModes = GenTypes.GetTypeInAnyAssembly("CombatExtended.CompFireModes");


        //private static MethodInfo ThingCompUtility_TryGetComp = typeof(ThingCompUtility).GetMethod("TryGetComp", BindingFlags.Static | BindingFlags.Public).MakeGenericMethod(new Type[] { typeof(CompEquippable) });

        //private static AccessTools.FieldRef<object, Verb> CompFireModes_verbInt = null;

        private static bool PreCompFireModes_Verb(ThingComp __instance, ref Verb __result)
        {
            CompChildNodeProccesser comp = ((CompChildNodeProccesser)__instance.parent) ?? (__instance.ParentHolder as CompChildNodeProccesser);
            if (comp != null)
            {
                while (comp != null)
                {
                    List<Verb> verbs = comp.parent.TryGetComp<CompEquippable>()?.AllVerbs;
                    if (verbs != null)
                    {
                        foreach (Verb verb in verbs)
                        {
                            if (verb.EquipmentSource == __instance.parent && verb.verbProps.isPrimary)
                            {
                                __result = verb;
                                return false;
                            }
                        }
                    }
                    comp = comp.ParentProccesser;
                }
            }
            return true;
        }


        //private static IEnumerable<CodeInstruction> CompFireModes_Verb_Transpiler(IEnumerable<CodeInstruction> instructions)
        //{
        //    foreach (CodeInstruction instruction in instructions)
        //    {
        //        if (instruction.Calls(ThingCompUtility_TryGetComp)) yield return new CodeInstruction(OpCodes.Call, _CompFireModes_TryGetComp);
        //        else yield return instruction;
        //    }
        //}

        //private static CompEquippable CompFireModes_TryGetComp(Thing thing)
        //{
        //    return thing.TryGetComp<CompEquippable>()
        //        ?? ((CompChildNodeProccesser)thing)?.parent.TryGetComp<CompEquippable>()
        //        ?? (thing?.ParentHolder as CompChildNodeProccesser)?.parent.TryGetComp<CompEquippable>();
        //}

        public static void PatchVerb(Harmony patcher)
        {
            if (CombatExtended_CompFireModes != null)
            {
                //CompFireModes_verbInt = AccessTools.FieldRefAccess<Verb>(CombatExtended_CompFireModes, "verbInt");
                MethodInfo target = CombatExtended_CompFireModes.GetMethod("get_Verb", BindingFlags.Instance | BindingFlags.NonPublic);
                patcher.Patch(
                    target,
                    prefix: new HarmonyMethod(_PreCompFireModes_Verb)
                    //,
                    //transpiler: new HarmonyMethod(_CompFireModes_Verb_Transpiler)
                    );
            }
        }
    }
}
