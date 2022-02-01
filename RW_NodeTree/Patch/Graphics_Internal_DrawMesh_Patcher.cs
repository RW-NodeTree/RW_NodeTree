using HarmonyLib;
using RW_NodeTree.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using Verse;

namespace RW_NodeTree.Patch
{

    internal static partial class Graphics_Patcher
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Graphics), "Internal_DrawMesh")]
        public static bool Internal_DrawMesh_Catcher(Mesh mesh, int submeshIndex, Matrix4x4 matrix, Material material, MaterialPropertyBlock properties, ShadowCastingMode castShadows, bool receiveShadows, Transform probeAnchor, LightProbeUsage lightProbeUsage, LightProbeProxyVolume lightProbeProxyVolume)
        {
            if (RenderingTools.StartOrEndDrawCatchingBlock)
            {
                RenderingTools.RenderInfos.Add(new RenderInfo(mesh, submeshIndex, matrix, material, properties, castShadows, receiveShadows, probeAnchor, lightProbeUsage, lightProbeProxyVolume));
                return false;
            }
            return true;
        }
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(Graphics), "Internal_DrawMesh")]
        public static IEnumerable<CodeInstruction> Internal_DrawMesh_DebugCheck(IEnumerable<CodeInstruction> instructions, MethodBase __originalMethod)
        {
            if (Prefs.DevMode) Log.Message(typeof(Graphics) + "::" + __originalMethod + " PatchSuccess\n" + instructions);
            return instructions;
        }
    }
}
