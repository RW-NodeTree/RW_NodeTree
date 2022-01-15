using HarmonyLib;
using RW_NodeTree.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace RW_NodeTree.Patch
{

    [HarmonyPatch(typeof(Graphics), "Internal_DrawMesh")]
    internal static class Graphics_Internal_DrawMesh_Patcher
    {
        [HarmonyPrefix]
        public static bool Internal_DrawMesh_Catcher(Mesh mesh, int submeshIndex, Matrix4x4 matrix, Material material, MaterialPropertyBlock properties, ShadowCastingMode castShadows, bool receiveShadows, Transform probeAnchor, LightProbeUsage lightProbeUsage, LightProbeProxyVolume lightProbeProxyVolume)
        {
            if (RenderingTools.StartOrEndDrawCatchingBlock)
            {
                RenderingTools.RenderInfos.Add(new RenderInfo(mesh, submeshIndex, matrix, material, properties, castShadows, receiveShadows, probeAnchor, lightProbeUsage, lightProbeProxyVolume));
                return false;
            }
            return true;
        }
    }
}
