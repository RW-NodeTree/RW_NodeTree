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
    /// <summary>
    /// Preview Rander Controler and Unity Graphics Draw Blocker in hear
    /// </summary>
    [StaticConstructorOnStartup]
    public static class RenderingTools
    {

        /// <summary>
        /// Preview Rander Camera
        /// </summary>
        internal static Camera Camera
        {
            get
            {
                if (camera == null)
                {
                    GameObject gameObject = new GameObject("Preview_Rander_Camera");
                    camera = gameObject.AddComponent<Camera>();
                    camera.orthographic = true;
                    camera.orthographicSize = 1;
                    camera.nearClipPlane = 0;
                    camera.farClipPlane = 71.5f;
                    camera.transform.position = new Vector3(0, FocusHeight + 65, 0);
                    camera.transform.rotation = Quaternion.Euler(90, 0, 0);
                    camera.backgroundColor = Color.clear;
                    camera.clearFlags = CameraClearFlags.SolidColor;
                    camera.targetTexture = empty;
                    camera.enabled = false;
                }
                return camera;
            }
        }


        /// <summary>
        /// getter: if there has any block,it will return true;setter:for setting block start or end
        /// </summary>
        public static bool StartOrEndDrawCatchingBlock
        {
            get
            {
                int current = Thread.CurrentThread.ManagedThreadId;

                Stack<List<RenderInfo>> list;
                if (!renderInfos.TryGetValue(current, out list))
                {
                    list = new Stack<List<RenderInfo>>();
                    renderInfos.Add(current, list);
                }
                if(list.Count > 0) return true;

                return false;
            }
            set
            {
                int current = Thread.CurrentThread.ManagedThreadId;

                Stack<List<RenderInfo>> list;
                if (!renderInfos.TryGetValue(current, out list))
                {
                    list = new Stack<List<RenderInfo>>();
                    renderInfos.Add(current, list);
                }

                if (value)
                {
                    list.Push(new List<RenderInfo>());
                }
                else
                {
                    list.Pop();
                }
            }
        }

        /// <summary>
        /// Catched rendering infos
        /// </summary>
        public static List<RenderInfo> RenderInfos
        {
            get
            {
                Stack<List<RenderInfo>> result;
                int current = Thread.CurrentThread.ManagedThreadId;
                if (!renderInfos.TryGetValue(current, out result))
                {
                    result = new Stack<List<RenderInfo>>();
                    renderInfos.Add(current, result);
                }
                return result.Peek();
            }
        }

        /// <summary>
        /// Render a perview texture by infos
        /// </summary>
        /// <param name="infos">all arranged render infos</param>
        /// <param name="size">force render texture size</param>
        /// <returns></returns>
        public static RenderTexture RenderToTarget(List<RenderInfo> infos, int size = 0)
        {
            if (size <= 0)
            {
                size = pixelSize(infos);
            }
            else if(size > MaxTexSize)
            {
                size = (int)MaxTexSize;
            }

            Camera.Render();
            //if (Prefs.DevMode) Log.Message("RenderToTarget size:" + size);
            RenderTexture target = new RenderTexture(size, size, 16, RenderTextureFormat.ARGB32);
            Camera.targetTexture = target;
            Camera.orthographicSize = size / TexSizeFactor;
            for (int i = 0; i < infos.Count; i++)
            {
                RenderInfo info = infos[i];
                for (int j = 0; j < info.matrices.Length; ++j)
                {
                    info.matrices[j].m13 += FocusHeight;
                }
                if(info.probeAnchor != null || !info.DrawMeshInstanced)
                {
                    for (int j = 0; j < info.matrices.Length && j < info.count; ++j)
                        Graphics.DrawMesh(info.mesh, info.matrices[j], info.material, info.layer, Camera, info.submeshIndex, info.properties, info.castShadows, info.receiveShadows, info.probeAnchor, info.lightProbeUsage, info.lightProbeProxyVolume);
                }
                else
                {
                    Graphics.DrawMeshInstanced(info.mesh, info.submeshIndex, info.material, info.matrices, info.count, info.properties, info.castShadows, info.receiveShadows, info.layer, Camera, info.lightProbeUsage, info.lightProbeProxyVolume);
                }
            }
            Camera.Render();
            Camera.targetTexture = empty;
            return target;
        }

        /// <summary>
        /// get the standard texture size of rendering infos
        /// </summary>
        /// <param name="infos">all arranged render infos</param>
        /// <returns>standard size of texture</returns>
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
                    result = (int)Math.Max(result, Math.Abs(vert4d.x) * TexSizeFactor);
                    result = (int)Math.Max(result, Math.Abs(vert4d.z) * TexSizeFactor);

                    //(-1, 1, 1)
                    extents.x *= -1;
                    vert3d = center + extents;
                    vert4d = matrix * new Vector4(vert3d.x, vert3d.y, vert3d.z, 1);
                    result = (int)Math.Max(result, Math.Abs(vert4d.x) * TexSizeFactor);
                    result = (int)Math.Max(result, Math.Abs(vert4d.z) * TexSizeFactor);

                    //(-1,-1, 1)
                    extents.y *= -1;
                    vert3d = center + extents;
                    vert4d = matrix * new Vector4(vert3d.x, vert3d.y, vert3d.z, 1);
                    result = (int)Math.Max(result, Math.Abs(vert4d.x) * TexSizeFactor);
                    result = (int)Math.Max(result, Math.Abs(vert4d.z) * TexSizeFactor);

                    //( 1,-1, 1)
                    extents.x *= -1;
                    vert3d = center + extents;
                    vert4d = matrix * new Vector4(vert3d.x, vert3d.y, vert3d.z, 1);
                    result = (int)Math.Max(result, Math.Abs(vert4d.x) * TexSizeFactor);
                    result = (int)Math.Max(result, Math.Abs(vert4d.z) * TexSizeFactor);

                    //( 1,-1,-1)
                    extents.z *= -1;
                    vert3d = center + extents;
                    vert4d = matrix * new Vector4(vert3d.x, vert3d.y, vert3d.z, 1);
                    result = (int)Math.Max(result, Math.Abs(vert4d.x) * TexSizeFactor);
                    result = (int)Math.Max(result, Math.Abs(vert4d.z) * TexSizeFactor);

                    //(-1,-1,-1)
                    extents.x *= -1;
                    vert3d = center + extents;
                    vert4d = matrix * new Vector4(vert3d.x, vert3d.y, vert3d.z, 1);
                    result = (int)Math.Max(result, Math.Abs(vert4d.x) * TexSizeFactor);
                    result = (int)Math.Max(result, Math.Abs(vert4d.z) * TexSizeFactor);

                    //(-1, 1,-1)
                    extents.y *= -1;
                    vert3d = center + extents;
                    vert4d = matrix * new Vector4(vert3d.x, vert3d.y, vert3d.z, 1);
                    result = (int)Math.Max(result, Math.Abs(vert4d.x) * TexSizeFactor);
                    result = (int)Math.Max(result, Math.Abs(vert4d.z) * TexSizeFactor);

                    //( 1, 1,-1)
                    extents.x *= -1;
                    vert3d = center + extents;
                    vert4d = matrix * new Vector4(vert3d.x, vert3d.y, vert3d.z, 1);
                    result = (int)Math.Max(result, Math.Abs(vert4d.x) * TexSizeFactor);
                    result = (int)Math.Max(result, Math.Abs(vert4d.z) * TexSizeFactor);

                    if (result >= MaxTexSize)
                    {
                        return result;
                    }
                }
            }
            return result;
        }

        private static Dictionary<int, Stack<List<RenderInfo>>> renderInfos = new Dictionary<int, Stack<List<RenderInfo>>>();
        private static Camera camera = null;
        private static RenderTexture empty = new RenderTexture(1, 1, 0, RenderTextureFormat.ARGB32);
        public const float FocusHeight = 4096;
        public const float MaxTexSize = 4096;
        public const float TexSizeFactor = 256;
    }
}