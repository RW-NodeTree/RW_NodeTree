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

    [HarmonyPatch(typeof(Graphics))]
    internal static partial class Graphics_Patcher
    {
        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(Graphics),
            "DrawMeshInstanced",
            typeof(Mesh),
            typeof(int),
            typeof(Material),
            typeof(Matrix4x4[]),
            typeof(int),
            typeof(MaterialPropertyBlock),
            typeof(ShadowCastingMode),
            typeof(bool),
            typeof(int),
            typeof(Camera),
            typeof(LightProbeUsage),
            typeof(LightProbeProxyVolume)
        )]
        private static bool DrawMeshInstanced_Catcher(Mesh mesh, int submeshIndex, Material material, Matrix4x4[] matrices, int count, MaterialPropertyBlock properties, ShadowCastingMode castShadows, bool receiveShadows, int layer, Camera camera, LightProbeUsage lightProbeUsage, LightProbeProxyVolume lightProbeProxyVolume)
        {
            if (camera == null && RenderingTools.StartOrEndDrawCatchingBlock)
            {
                //if (Prefs.DevMode) Log.Message(" Internal_DrawMesh: camera=" + camera + "; layer=" + layer + "\n");
                RenderingTools.RenderInfos.Add(new RenderInfo(mesh, submeshIndex, material, matrices, count, properties, castShadows, receiveShadows, layer, lightProbeUsage, lightProbeProxyVolume));
                return false;
            }
            return true;
        }
    }
}
