﻿using HarmonyLib;
using RW_NodeTree.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace RW_NodeTree.Patch
{

    internal static partial class Graphics_Patcher
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Graphics), "Internal_DrawMesh")]
        private static bool Internal_DrawMesh_Catcher(Mesh mesh, int submeshIndex, Matrix4x4 matrix, Material material, int layer, Camera? camera, MaterialPropertyBlock? properties, ShadowCastingMode castShadows, bool receiveShadows, Transform? probeAnchor, LightProbeUsage lightProbeUsage, LightProbeProxyVolume? lightProbeProxyVolume)
        {
            if (camera == null && RenderingTools.StartOrEndDrawCatchingBlock)
            {
                //if (Prefs.DevMode) Log.Message(" Internal_DrawMesh: camera=" + camera + "; layer=" + layer + "\n");
                RenderingTools.RenderInfos!.Add(new RenderInfo(mesh, submeshIndex, matrix, material, layer, properties, castShadows, receiveShadows, probeAnchor, lightProbeUsage, lightProbeProxyVolume));
                return false;
            }
            return true;
        }
    }
}
