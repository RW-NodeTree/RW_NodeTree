using HarmonyLib;
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
using RimWorld;

namespace RW_NodeTree.Patch
{
    [HarmonyPatch(typeof(PawnRenderer))]
    internal static partial class PawnRenderer_Patcher
    {
        [HarmonyTranspiler]
        [HarmonyPatch(
            typeof(PawnRenderer),
            "DrawEquipmentAiming"
        )]
        public static IEnumerable<CodeInstruction> TranspilPawnRenderer_DrawEquipmentAiming(IEnumerable<CodeInstruction> instructions)
        {
            int count = 0;
            CodeInstruction[] codes = new CodeInstruction[8];
            foreach(CodeInstruction instruction in instructions)
            {
                if(count < 8)
                {
                    codes[count] = instruction;
                    count++;
                }
                else
                {
                    if (
                        codes[0].opcode == OpCodes.Ldloc_0      &&  codes[1].opcode == OpCodes.Ldarg_2  &&
                        codes[2].opcode == OpCodes.Ldloc_1      &&  codes[3].Calls(Vector3_get_up)      &&
                        codes[4].Calls(Quaternion_AngleAxis)    &&  codes[5].opcode == OpCodes.Ldarg_3  &&
                        codes[6].opcode == OpCodes.Ldc_I4_0     &&  codes[7].Calls(Graphics_DrawMeshOrg)
                    )
                    {
                        for (int j = 0; j < 5; j++) yield return codes[j];
                        yield return new CodeInstruction(OpCodes.Ldarg_1);
                        yield return new CodeInstruction(OpCodes.Callvirt, Thing_get_Graphic);
                        yield return new CodeInstruction(OpCodes.Call, GraphicHelper_GetGraphic_ChildNode);
                        yield return new CodeInstruction(OpCodes.Ldfld, Graphic_drawSize);
                        yield return new CodeInstruction(OpCodes.Call, PawnRenderer_internalConvert);
                        yield return new CodeInstruction(OpCodes.Call, Matrix4x4_TRS);
                        for (int j = 5; j < 7; j++) yield return codes[j];
                        yield return new CodeInstruction(OpCodes.Call, Graphics_DrawMeshTar);
                        codes[0] = instruction;
                        count = 1;
                    }
                    else
                    {
                        yield return codes[0];
                        for (int i = 0; i < 7; i++) codes[i] = codes[i + 1];
                        codes[7] = instruction;
                    }
                }
            }
            for (int i = 0; i < count; i++) yield return codes[i];
        }

        private static Vector3 internalConvert (Vector2 vector)
        {
            return new Vector3(vector.x, 0, vector.y);
        }

        private static MethodInfo Graphics_DrawMeshOrg = typeof(Graphics).GetMethod("DrawMesh", new Type[] { typeof(Mesh), typeof(Vector3), typeof(Quaternion), typeof(Material), typeof(int)});
        private static MethodInfo Graphics_DrawMeshTar = typeof(Graphics).GetMethod("DrawMesh", new Type[] { typeof(Mesh), typeof(Matrix4x4), typeof(Material), typeof(int) });
        private static MethodInfo Quaternion_AngleAxis = typeof(Quaternion).GetMethod("AngleAxis", AccessTools.all);
        private static MethodInfo Vector3_get_up = typeof(Vector3).GetMethod("get_up", AccessTools.all);
        private static MethodInfo Thing_get_Graphic = typeof(Thing).GetMethod("get_Graphic", AccessTools.all);
        private static MethodInfo GraphicHelper_GetGraphic_ChildNode = typeof(GraphicHelper).GetMethod("GetGraphic_ChildNode", AccessTools.all);
        private static MethodInfo PawnRenderer_internalConvert = typeof(PawnRenderer_Patcher).GetMethod("internalConvert", AccessTools.all);
        private static MethodInfo Matrix4x4_TRS = typeof(Matrix4x4).GetMethod("TRS", AccessTools.all);
        private static FieldInfo Graphic_drawSize = typeof(Graphic).GetField("drawSize", AccessTools.all);
    }
}
