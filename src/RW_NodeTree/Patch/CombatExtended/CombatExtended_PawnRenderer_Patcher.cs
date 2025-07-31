using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;

namespace RW_NodeTree.Patch.CombatExtended
{
    // When version <= 1.4
    internal static class CombatExtended_PawnRenderer_Patcher
    {
        private static MethodInfo _PerHarmony_PawnRenderer_DrawEquipmentAiming_DrawMesh = typeof(CombatExtended_PawnRenderer_Patcher).GetMethod("PerHarmony_PawnRenderer_DrawEquipmentAiming_DrawMesh", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        private static MethodInfo _FinalHarmony_PawnRenderer_DrawEquipmentAiming_DrawMesh = typeof(CombatExtended_PawnRenderer_Patcher).GetMethod("FinalHarmony_PawnRenderer_DrawEquipmentAiming_DrawMesh", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        private static Type Harmony_PawnRenderer_DrawEquipmentAiming = GenTypes.GetTypeInAnyAssembly("CombatExtended.HarmonyCE.Harmony_PawnRenderer")?.GetNestedType("Harmony_PawnRenderer_DrawEquipmentAiming", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public) ?? GenTypes.GetTypeInAnyAssembly("CombatExtended.HarmonyCE.Harmony_PawnRenderer_DrawEquipmentAiming");
        private static Type GunDrawExtension = GenTypes.GetTypeInAnyAssembly("CombatExtended.GunDrawExtension");
        private static AccessTools.FieldRef<object, Vector2>? GunDrawExtension_DrawSize = null;

        private static void PerHarmony_PawnRenderer_DrawEquipmentAiming_DrawMesh(
            Thing? eq,
            ref (DefModExtension?, Vector2) __state)
        {
            if (GunDrawExtension_DrawSize == null) throw new ArgumentNullException(nameof(GunDrawExtension_DrawSize));
            CompChildNodeProccesser? comp = eq;
            if (comp != null && eq != null)
            {
                if (eq.def.modExtensions.NullOrEmpty())
                {
                    eq.def.modExtensions = eq.def.modExtensions ?? new List<DefModExtension>();
                    eq.def.modExtensions.Add((DefModExtension)Activator.CreateInstance(GunDrawExtension));
                }
                DefModExtension? targetExtension = null;
                foreach (DefModExtension extension in eq.def.modExtensions)
                {
                    if (GunDrawExtension.IsAssignableFrom(extension.GetType()))
                    {
                        targetExtension = extension;
                        break;
                    }
                }
                if (targetExtension == null)
                {
                    targetExtension = (DefModExtension)Activator.CreateInstance(GunDrawExtension);
                    GunDrawExtension_DrawSize(targetExtension) = eq.def.graphicData.drawSize;
                    eq.def.modExtensions.Add(targetExtension);
                }
                ref Vector2 DrawSize = ref GunDrawExtension_DrawSize(targetExtension);
                __state = (targetExtension, DrawSize);
                DrawSize = eq.Graphic.drawSize;
            }
        }

        private static void FinalHarmony_PawnRenderer_DrawEquipmentAiming_DrawMesh((DefModExtension?, Vector2) __state)
        {
            if (GunDrawExtension_DrawSize == null) throw new ArgumentNullException(nameof(GunDrawExtension_DrawSize));
            (DefModExtension? extension, Vector2 drawSize) = __state;
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
                    new HarmonyMethod(_PerHarmony_PawnRenderer_DrawEquipmentAiming_DrawMesh),
                    null,
                    null,
                    new HarmonyMethod(_FinalHarmony_PawnRenderer_DrawEquipmentAiming_DrawMesh)
                    );
            }
        }
    }
}
