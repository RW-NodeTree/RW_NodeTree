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
    internal static class CombatExtended_PawnRenderer_Patcher
    {
        private static MethodInfo _PerHarmony_PawnRenderer_Harmony_PawnRenderer_DrawEquipmentAiming_DrawMesh = typeof(CombatExtended_PawnRenderer_Patcher).GetMethod("PerHarmony_PawnRenderer_Harmony_PawnRenderer_DrawEquipmentAiming_DrawMesh", BindingFlags.Static | BindingFlags.NonPublic);
        private static MethodInfo _PostHarmony_PawnRenderer_Harmony_PawnRenderer_DrawEquipmentAiming_DrawMesh = typeof(CombatExtended_PawnRenderer_Patcher).GetMethod("PostHarmony_PawnRenderer_Harmony_PawnRenderer_DrawEquipmentAiming_DrawMesh", BindingFlags.Static | BindingFlags.NonPublic);
        private static Type Harmony_PawnRenderer_DrawEquipmentAiming = GenTypes.GetTypeInAnyAssembly("CombatExtended.HarmonyCE.Harmony_PawnRenderer")?.GetNestedType("Harmony_PawnRenderer_DrawEquipmentAiming", BindingFlags.Static | BindingFlags.NonPublic);
        private static Type GunDrawExtension = GenTypes.GetTypeInAnyAssembly("CombatExtended.GunDrawExtension");
        private static AccessTools.FieldRef<object, Vector2> GunDrawExtension_DrawSize = null;

        private static void PerHarmony_PawnRenderer_Harmony_PawnRenderer_DrawEquipmentAiming_DrawMesh(Matrix4x4 matrix, Thing eq, ref (DefModExtension, Vector2) __state)
        {
            CompChildNodeProccesser comp = eq;
            if (comp != null)
            {
                if (eq.def.modExtensions.NullOrEmpty())
                {
                    eq.def.modExtensions = eq.def.modExtensions ?? new List<DefModExtension>();
                    eq.def.modExtensions.Add((DefModExtension)Activator.CreateInstance(GunDrawExtension));
                }
                foreach (DefModExtension extension in eq.def.modExtensions)
                {
                    if (GunDrawExtension.IsAssignableFrom(extension.GetType()))
                    {
                        ref Vector2 DrawSize = ref GunDrawExtension_DrawSize(extension);
                        __state = (extension, DrawSize);
                        Vector3 scale = matrix.lossyScale;
                        DrawSize = new Vector2(scale.x, scale.z);
                        return;
                    }
                }
            }
        }

        private static void PostHarmony_PawnRenderer_Harmony_PawnRenderer_DrawEquipmentAiming_DrawMesh((DefModExtension, Vector2) __state)
        {
            (DefModExtension extension, Vector2 drawSize) = __state;
            if (extension != null)
            {
                GunDrawExtension_DrawSize(extension) = drawSize;
            }
        }


        public static void PatchDrawMesh(Harmony patcher)
        {
            //Log.Message($"Try Patch CE; Harmony_PawnRenderer_DrawEquipmentAiming={Harmony_PawnRenderer_DrawEquipmentAiming}; GunDrawExtension={GunDrawExtension}");
            if (Harmony_PawnRenderer_DrawEquipmentAiming != null && GunDrawExtension != null)
            {
                //Log.Message("Patching CE");
                GunDrawExtension_DrawSize = AccessTools.FieldRefAccess<Vector2>(GunDrawExtension, "DrawSize");
                MethodInfo target = Harmony_PawnRenderer_DrawEquipmentAiming.GetMethod("DrawMesh", BindingFlags.Static | BindingFlags.NonPublic);
                patcher.Patch(
                    target,
                    new HarmonyMethod(_PerHarmony_PawnRenderer_Harmony_PawnRenderer_DrawEquipmentAiming_DrawMesh),
                    new HarmonyMethod(_PostHarmony_PawnRenderer_Harmony_PawnRenderer_DrawEquipmentAiming_DrawMesh)
                    );
            }
        }
    }
}
