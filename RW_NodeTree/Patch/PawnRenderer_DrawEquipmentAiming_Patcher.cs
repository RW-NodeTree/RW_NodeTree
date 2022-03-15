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
            foreach(CodeInstruction instruction in instructions)
            {
                if(instruction.Calls(MathHelper_DrawMeshOrg))
                {
                    yield return new CodeInstruction(OpCodes.Call, MathHelper_DrawMeshTar);
                    continue;
                }
                yield return instruction;
                if(instruction.Calls(Quaternion_AngleAxis))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Callvirt, Thing_get_Graphic);
                    yield return new CodeInstruction(OpCodes.Call, GraphicHelper_GetGraphic_ChildNode);
                    yield return new CodeInstruction(OpCodes.Ldfld, Graphic_drawSize);
                    yield return new CodeInstruction(OpCodes.Call, MathHelper_toVector3OnMap);
                    yield return new CodeInstruction(OpCodes.Call, Matrix4x4_TRS);
                }
            }
        }

        private static Vector3 internalConvert (Vector2 vector)
        {
            return new Vector3(vector.x, 0, vector.y);
        }

        private static MethodInfo MathHelper_DrawMeshOrg = typeof(Graphics).GetMethod("DrawMesh", new Type[] { typeof(Mesh), typeof(Vector3), typeof(Quaternion), typeof(Material), typeof(int)});
        private static MethodInfo MathHelper_DrawMeshTar = typeof(Graphics).GetMethod("DrawMesh", new Type[] { typeof(Mesh), typeof(Matrix4x4), typeof(Material), typeof(int) });
        private static MethodInfo Quaternion_AngleAxis = typeof(Quaternion).GetMethod("AngleAxis", AccessTools.all);
        private static MethodInfo Thing_get_Graphic = typeof(Thing).GetMethod("get_Graphic", AccessTools.all);
        private static MethodInfo GraphicHelper_GetGraphic_ChildNode = typeof(GraphicHelper).GetMethod("GetGraphic_ChildNode", AccessTools.all);
        private static MethodInfo MathHelper_toVector3OnMap = typeof(PawnRenderer_Patcher).GetMethod("internalConvert", AccessTools.all);
        private static MethodInfo Matrix4x4_TRS = typeof(Matrix4x4).GetMethod("TRS", AccessTools.all);
        private static FieldInfo Graphic_drawSize = typeof(Graphic).GetField("drawSize", AccessTools.all);
    }
}
