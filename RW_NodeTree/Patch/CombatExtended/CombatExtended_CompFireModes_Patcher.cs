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
    internal static class CombatExtended_CompFireModes_Patcher
    {
        private static MethodInfo _PostCompFireModes_Verb = typeof(CombatExtended_CompFireModes_Patcher).GetMethod("PostCompFireModes_Verb", BindingFlags.Static | BindingFlags.NonPublic);
        private static MethodInfo _PostCompFireModes_Verb_Transpiler = typeof(CombatExtended_CompFireModes_Patcher).GetMethod("PostCompFireModes_Verb_Transpiler", BindingFlags.Static | BindingFlags.NonPublic);
        private static MethodInfo _CompFireModes_TryGetComp = typeof(CombatExtended_CompFireModes_Patcher).GetMethod("CompFireModes_TryGetComp", BindingFlags.Static | BindingFlags.NonPublic);
        private static MethodInfo _CompFireModes_PrimaryVerb = typeof(CombatExtended_CompFireModes_Patcher).GetMethod("CompFireModes_PrimaryVerb", BindingFlags.Static | BindingFlags.NonPublic);
        private static Type CombatExtended_CompFireModes = GenTypes.GetTypeInAnyAssembly("CombatExtended.CompFireModes");


        private static MethodInfo ThingCompUtility_TryGetComp = typeof(ThingCompUtility).GetMethod("TryGetComp", BindingFlags.Static | BindingFlags.Public).MakeGenericMethod(new Type[] { typeof(CompEquippable) });
        private static MethodInfo CompEquippable_PrimaryVerb = typeof(CompEquippable).GetMethod("get_PrimaryVerb", BindingFlags.Instance | BindingFlags.Public);

        private static AccessTools.FieldRef<object, Verb> CompFireModes_verbInt = null;

        private static void PostCompFireModes_Verb(ThingComp __instance, ref Verb __result)
        {
            CompChildNodeProccesser comp = ((CompChildNodeProccesser)__instance.parent) ?? (__instance.ParentHolder as CompChildNodeProccesser);
            if (comp != null)
            {
                CompFireModes_verbInt(__instance) = null;
            }
        }


        private static IEnumerable<CodeInstruction> PostCompFireModes_Verb_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.Calls(ThingCompUtility_TryGetComp)) yield return new CodeInstruction(OpCodes.Call, _CompFireModes_TryGetComp);
                else if(instruction.Calls(CompEquippable_PrimaryVerb))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, _CompFireModes_PrimaryVerb);
                }
                else yield return instruction;
            }
        }

        private static CompEquippable CompFireModes_TryGetComp(Thing thing)
        {
            Log.Message($"{thing}");
            return thing.TryGetComp<CompEquippable>()
                ?? (thing?.ParentHolder as CompChildNodeProccesser)?.parent.TryGetComp<CompEquippable>();
        }

        private static Verb CompFireModes_PrimaryVerb(CompEquippable compEq, ThingComp comp)
        {
            CompChildNodeProccesser childNodeProccesser = compEq.parent;
            if (childNodeProccesser != null)
            {
                foreach (Verb verb in CompChildNodeProccesser.GetAllOriginalVerbs(compEq.VerbTracker))
                {
                    if (comp.parent == childNodeProccesser.GetBeforeConvertVerbCorrespondingThing(typeof(CompEquippable), verb).Item1)
                    {
                        return verb;
                    }
                }
            }
            return compEq.PrimaryVerb;
        }

        public static void PatchVerb(Harmony patcher)
        {
            if (CombatExtended_CompFireModes != null)
            {
                CompFireModes_verbInt = AccessTools.FieldRefAccess<Verb>(CombatExtended_CompFireModes, "verbInt");
                MethodInfo target = CombatExtended_CompFireModes.GetMethod("get_Verb", BindingFlags.Instance | BindingFlags.NonPublic);
                patcher.Patch(
                    target,
                    postfix: new HarmonyMethod(_PostCompFireModes_Verb),
                    transpiler: new HarmonyMethod(_PostCompFireModes_Verb_Transpiler)
                    );
            }
        }
    }
}
