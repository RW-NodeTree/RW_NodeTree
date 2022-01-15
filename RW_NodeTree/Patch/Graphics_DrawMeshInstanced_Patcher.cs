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

    [HarmonyPatch(typeof(Graphics), "DrawMeshInstanced")]
    internal static class Graphics_DrawMeshInstanced_Patcher
    {
        [HarmonyPrefix]
        public static bool DrawMeshInstanced_Catcher(Mesh mesh, int submeshIndex, Material material, Matrix4x4[] matrices, int count, MaterialPropertyBlock properties, ShadowCastingMode castShadows, bool receiveShadows, LightProbeUsage lightProbeUsage, LightProbeProxyVolume lightProbeProxyVolume)
        {
            if (RenderingTools.StartOrEndDrawCatchingBlock)
            {
                RenderingTools.RenderInfos.Add(new RenderInfo(mesh, submeshIndex, material, matrices, count, properties, castShadows, receiveShadows, lightProbeUsage, lightProbeProxyVolume));
                return false;
            }
            return true;
        }
    }
}
