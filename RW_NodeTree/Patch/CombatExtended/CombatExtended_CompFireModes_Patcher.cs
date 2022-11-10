using HarmonyLib;
using RimWorld;
using RW_NodeTree.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RW_NodeTree.Patch.CombatExtended
{
    internal static class CombatExtended_CompFireModes_Patcher
    {
        private static MethodInfo _PostCompFireModes_Verb = typeof(CombatExtended_CompFireModes_Patcher).GetMethod("PostCompFireModes_Verb", BindingFlags.Static | BindingFlags.NonPublic);
        private static Type CombatExtended_CompFireModes = GenTypes.GetTypeInAnyAssembly("CombatExtended.CompFireModes");

        private static AccessTools.FieldRef<object, Verb> CompFireModes_verbInt = null;

        private static void PostCompFireModes_Verb(ThingComp __instance)
        {
            CompChildNodeProccesser comp = ((CompChildNodeProccesser)__instance.parent) ?? (__instance.ParentHolder as CompChildNodeProccesser);
            if (comp != null)
            {
                CompFireModes_verbInt(__instance) = null;
            }
        }


        public static void PatchVerb(Harmony patcher)
        {
            if (CombatExtended_CompFireModes != null)
            {
                CompFireModes_verbInt = AccessTools.FieldRefAccess<Verb>(CombatExtended_CompFireModes, "verbInt");
                MethodInfo target = CombatExtended_CompFireModes.GetMethod("get_Verb", BindingFlags.Instance | BindingFlags.NonPublic);
                patcher.Patch(
                    target,
                    postfix: new HarmonyMethod(_PostCompFireModes_Verb)
                    );
            }
        }
    }
}
