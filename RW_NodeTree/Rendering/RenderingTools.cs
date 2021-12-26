using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using HarmonyLib;
using RimWorld;
using Verse;
using UnityEngine;
using System.Runtime.CompilerServices;
using UnityEngine.Rendering;

namespace RW_NodeTree.Rendering
{
    [StaticConstructorOnStartup]
    public static class RenderingTools
    {
        static RenderingTools()
        {
            camera.orthographic = true;
            camera.orthographicSize = 1;
            camera.nearClipPlane = 0;
            camera.farClipPlane = 2048;
            camera.transform.position = new Vector3(0, 5120, 0);
            camera.transform.rotation = Quaternion.Euler(90, 0, 0);
            camera.enabled = false;
            camera.backgroundColor = Color.clear;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.cullingMask = 31;
            patcher.PatchAll();
        }

        public static bool BlockingState
        {
            get
            {
                bool result;
                Thread current = Thread.CurrentThread;
                if (!blockingState.TryGetValue(current, out result))
                {
                    blockingState.Add(current, false);
                }
                return result;
            }
            set
            {
                Thread current = Thread.CurrentThread;
                blockingState.SetOrAdd(current, value);
                List<RenderInfo> list;
                if (!renderInfos.TryGetValue(current, out list))
                {
                    list = new List<RenderInfo>();
                    renderInfos.Add(current, list);
                }
                else
                {
                    list.Clear();
                }
            }
        }

        public static List<RenderInfo> RenderInfos
        {
            get
            {
                List<RenderInfo> result;
                Thread current = Thread.CurrentThread;
                if (!renderInfos.TryGetValue(current, out result))
                {
                    result = new List<RenderInfo>();
                    renderInfos.Add(current, result);
                }
                return result;
            }
        }

        public static RenderTexture RenderToTarget(List<RenderInfo> infos, RenderTexture target, int size = 0)
        {
            RenderTexture cache = camera.targetTexture;
            camera.targetTexture = target;
            if (size <= 0)
            {
                size = pixelSize(infos);
                if (target == null || target.texelSize.x > size || target.texelSize.y > size)
                {
                    if (target != null) GameObject.Destroy(target);
                    target = new RenderTexture(size, size, 16);
                }
            }
            else if (target == null)
            {
                target = new RenderTexture(size, size, 16);
            }
            camera.orthographicSize = size;
            for (int i = 0; i < infos.Count; i++)
            {
                RenderInfo info = infos[i];
                for (int j = 0; j < info.matrices.Length; ++j)
                {
                    info.matrices[j].m13 += 4096;
                }
                if(info.probeAnchor != null)
                {
                    for (int j = 0; j < info.matrices.Length && j < info.count; ++j) Graphics.DrawMesh(info.mesh, info.matrices[j], info.material, 31, camera, info.submeshIndex, info.properties, info.castShadows, info.receiveShadows, info.probeAnchor, info.lightProbeUsage, info.lightProbeProxyVolume);
                }
                else
                {
                    Graphics.DrawMeshInstanced(info.mesh, info.submeshIndex, info.material, info.matrices, info.count, info.properties, info.castShadows, info.receiveShadows, 31, camera, info.lightProbeUsage, info.lightProbeProxyVolume);
                }
            }
            camera.Render();
            camera.targetTexture = cache;
            return target;
        }

        public static int pixelSize(List<RenderInfo> infos)
        {
            int result = 1;
            foreach(RenderInfo info in infos)
            {
                Bounds bounds = info.mesh.bounds;

                Vector3 center = bounds.center;


                for (int i = 0; i < info.matrices.Length && i < info.count; ++i)
                {
                    Matrix4x4 matrix = info.matrices[i];
                    //calculate 8 vertex of the border cube
                    //( 1, 1, 1)
                    Vector3 extents = bounds.extents;
                    Vector3 vert3d = center + extents;
                    Vector4 vert4d = matrix * new Vector4(vert3d.x, vert3d.y, vert3d.z, 1);
                    result = (int)Math.Max(result, Math.Abs(vert4d.x) * 256);
                    result = (int)Math.Max(result, Math.Abs(vert4d.z) * 256);

                    //(-1, 1, 1)
                    extents.x *= -1;
                    vert3d = center + extents;
                    vert4d = matrix * new Vector4(vert3d.x, vert3d.y, vert3d.z, 1);
                    result = (int)Math.Max(result, Math.Abs(vert4d.x) * 256);
                    result = (int)Math.Max(result, Math.Abs(vert4d.z) * 256);

                    //(-1,-1, 1)
                    extents.y *= -1;
                    vert3d = center + extents;
                    vert4d = matrix * new Vector4(vert3d.x, vert3d.y, vert3d.z, 1);
                    result = (int)Math.Max(result, Math.Abs(vert4d.x) * 256);
                    result = (int)Math.Max(result, Math.Abs(vert4d.z) * 256);

                    //( 1,-1, 1)
                    extents.x *= -1;
                    vert3d = center + extents;
                    vert4d = matrix * new Vector4(vert3d.x, vert3d.y, vert3d.z, 1);
                    result = (int)Math.Max(result, Math.Abs(vert4d.x) * 256);
                    result = (int)Math.Max(result, Math.Abs(vert4d.z) * 256);

                    //( 1,-1,-1)
                    extents.z *= -1;
                    vert3d = center + extents;
                    vert4d = matrix * new Vector4(vert3d.x, vert3d.y, vert3d.z, 1);
                    result = (int)Math.Max(result, Math.Abs(vert4d.x) * 256);
                    result = (int)Math.Max(result, Math.Abs(vert4d.z) * 256);

                    //(-1,-1,-1)
                    extents.x *= -1;
                    vert3d = center + extents;
                    vert4d = matrix * new Vector4(vert3d.x, vert3d.y, vert3d.z, 1);
                    result = (int)Math.Max(result, Math.Abs(vert4d.x) * 256);
                    result = (int)Math.Max(result, Math.Abs(vert4d.z) * 256);

                    //(-1, 1,-1)
                    extents.y *= -1;
                    vert3d = center + extents;
                    vert4d = matrix * new Vector4(vert3d.x, vert3d.y, vert3d.z, 1);
                    result = (int)Math.Max(result, Math.Abs(vert4d.x) * 256);
                    result = (int)Math.Max(result, Math.Abs(vert4d.z) * 256);

                    //( 1, 1,-1)
                    extents.x *= -1;
                    vert3d = center + extents;
                    vert4d = matrix * new Vector4(vert3d.x, vert3d.y, vert3d.z, 1);
                    result = (int)Math.Max(result, Math.Abs(vert4d.x) * 256);
                    result = (int)Math.Max(result, Math.Abs(vert4d.z) * 256);
                }

            }
            return result;
        }

        private static Harmony patcher = new Harmony("RW_NodeTree.Patch.RenderingPatch");
        private static Dictionary<Thread, bool> blockingState = new Dictionary<Thread,bool>();
        private static Dictionary<Thread, List<RenderInfo>> renderInfos = new Dictionary<Thread, List<RenderInfo>>();
        private static Camera camera = new Camera();
    }

    public struct RenderInfo
    {

        public Mesh mesh;
        public int submeshIndex;
        public Matrix4x4[] matrices;
        public Material material;
        public MaterialPropertyBlock properties;
        public ShadowCastingMode castShadows;
        public bool receiveShadows;
        public Transform probeAnchor;
        public LightProbeUsage lightProbeUsage;
        public LightProbeProxyVolume lightProbeProxyVolume;
        public int count;

        public RenderInfo(Mesh mesh, int submeshIndex, Matrix4x4 matrix, Material material)
        {
            this.mesh = mesh;
            this.submeshIndex = submeshIndex;
            this.matrices = new Matrix4x4[] { matrix };
            this.material = material;
            this.properties = null;
            this.castShadows = ShadowCastingMode.On;
            this.receiveShadows = true;
            this.probeAnchor = null;
            this.lightProbeUsage = LightProbeUsage.BlendProbes;
            this.lightProbeProxyVolume = null;
            this.count = 1;
        }

        public RenderInfo(Mesh mesh, int submeshIndex, Matrix4x4 matrix, Material material, MaterialPropertyBlock properties, ShadowCastingMode castShadows, bool receiveShadows, Transform probeAnchor, LightProbeUsage lightProbeUsage, LightProbeProxyVolume lightProbeProxyVolume)
        {
            this.mesh = mesh;
            this.submeshIndex = submeshIndex;
            this.matrices = new Matrix4x4[] { matrix };
            this.material = material;
            this.properties = properties;
            this.castShadows = castShadows;
            this.receiveShadows = receiveShadows;
            this.probeAnchor = probeAnchor;
            this.lightProbeUsage = lightProbeUsage;
            this.lightProbeProxyVolume = lightProbeProxyVolume;
            this.count = 1;
        }

        public RenderInfo(Mesh mesh, int submeshIndex, Material material, Matrix4x4[] matrices, int count, MaterialPropertyBlock properties, ShadowCastingMode castShadows, bool receiveShadows, LightProbeUsage lightProbeUsage, LightProbeProxyVolume lightProbeProxyVolume)
        {
            this.mesh = mesh;
            this.submeshIndex = submeshIndex;
            this.matrices = matrices;
            this.material = material;
            this.properties = properties;
            this.castShadows = castShadows;
            this.receiveShadows = receiveShadows;
            this.probeAnchor = null;
            this.lightProbeUsage = lightProbeUsage;
            this.lightProbeProxyVolume = lightProbeProxyVolume;
            this.count = count;
        }
    }

    [HarmonyPatch(typeof(Graphics),"Internal_DrawMesh")]
    public static class Internal_DrawMesh_Patcher
    {
        [HarmonyPrefix]
        public static bool Internal_DrawMesh_Catcher(Mesh mesh, int submeshIndex, Matrix4x4 matrix, Material material, MaterialPropertyBlock properties, ShadowCastingMode castShadows, bool receiveShadows, Transform probeAnchor, LightProbeUsage lightProbeUsage, LightProbeProxyVolume lightProbeProxyVolume)
        {
            if(RenderingTools.BlockingState)
            {
                RenderingTools.RenderInfos.Add(new RenderInfo(mesh, submeshIndex, matrix, material,properties,castShadows,receiveShadows,probeAnchor,lightProbeUsage,lightProbeProxyVolume));
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Graphics), "DrawMeshInstanced")]
    public static class DrawMeshInstanced_Patcher
    {
        [HarmonyPrefix]
        public static bool DrawMeshInstanced_Catcher(Mesh mesh, int submeshIndex, Material material, Matrix4x4[] matrices, int count, MaterialPropertyBlock properties, ShadowCastingMode castShadows, bool receiveShadows, LightProbeUsage lightProbeUsage, LightProbeProxyVolume lightProbeProxyVolume)
        {
            if (RenderingTools.BlockingState)
            {
                RenderingTools.RenderInfos.Add(new RenderInfo(mesh, submeshIndex, material, matrices, count, properties, castShadows, receiveShadows, lightProbeUsage, lightProbeProxyVolume));
                return false;
            }
            return true;
        }
    }
}