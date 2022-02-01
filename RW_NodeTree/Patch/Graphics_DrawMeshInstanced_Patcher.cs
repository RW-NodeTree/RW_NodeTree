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
        public static bool DrawMeshInstanced_Catcher(Mesh mesh, int submeshIndex, Material material, Matrix4x4[] matrices, int count, MaterialPropertyBlock properties, ShadowCastingMode castShadows, bool receiveShadows, LightProbeUsage lightProbeUsage, LightProbeProxyVolume lightProbeProxyVolume)
        {
            if (RenderingTools.StartOrEndDrawCatchingBlock)
            {
                RenderingTools.RenderInfos.Add(new RenderInfo(mesh, submeshIndex, material, matrices, count, properties, castShadows, receiveShadows, lightProbeUsage, lightProbeProxyVolume));
                return false;
            }
            return true;
        }

        [HarmonyTranspiler]
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
        public static IEnumerable<CodeInstruction> DrawMeshInstanced_DebugCheck(IEnumerable<CodeInstruction> instructions, MethodBase __originalMethod)
        {
            if (Prefs.DevMode) Log.Message(typeof(Graphics) + "::" + __originalMethod + " PatchSuccess\n");
            return instructions;
        }
    }
}
