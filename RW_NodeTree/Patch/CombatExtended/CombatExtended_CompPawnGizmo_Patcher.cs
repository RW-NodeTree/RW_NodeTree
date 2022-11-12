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
    internal static class CombatExtended_CompPawnGizmo_Patcher
    {
        private static MethodInfo _PostCompPawnGizmo_CompGetGizmosExtra = typeof(CombatExtended_CompPawnGizmo_Patcher).GetMethod("PostCompPawnGizmo_CompGetGizmosExtra", BindingFlags.Static | BindingFlags.NonPublic);
        private static Type CombatExtended_CompPawnGizmo = GenTypes.GetTypeInAnyAssembly("CombatExtended.CompPawnGizmo");

        private static void PostCompPawnGizmo_CompGetGizmosExtra(ThingComp __instance, ref IEnumerable<Gizmo> __result)
        {
            CompChildNodeProccesser comp = __instance.parent;
            if(comp != null) __result = inner_PostCompPawnGizmo_CompGetGizmosExtra(comp, __result);
        }

        private static IEnumerable<Gizmo> inner_PostCompPawnGizmo_CompGetGizmosExtra(CompChildNodeProccesser comp, IEnumerable<Gizmo> gizmos)
        {
            foreach(Gizmo gizmo in gizmos)
            {
                yield return gizmo;
            }
            foreach(CompBasicNodeComp compBasicNode in comp.AllNodeComp)
            {
                foreach (Gizmo gizmo in compBasicNode.CompGetGizmosExtra())
                {
                    yield return gizmo;
                }
            }
        }


        public static void PatchCompPawnGizmo(Harmony patcher)
        {
            if (CombatExtended_CompPawnGizmo != null)
            {
                MethodInfo target = CombatExtended_CompPawnGizmo.GetMethod("CompGetGizmosExtra", BindingFlags.Instance | BindingFlags.Public);
                patcher.Patch(
                    target,
                    postfix: new HarmonyMethod(_PostCompPawnGizmo_CompGetGizmosExtra)
                    );
            }
        }
    }
}
