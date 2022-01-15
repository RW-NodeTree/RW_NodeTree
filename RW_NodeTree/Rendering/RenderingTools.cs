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
            camera.name = "Preview Rander Camera";
        }


        /// <summary>
        /// getter: if there has any block,it will return true;setter:for setting block start or end
        /// </summary>
        public static bool StartOrEndDrawCatchingBlock
        {
            get
            {
                Thread current = Thread.CurrentThread;
                Stack<List<RenderInfo>> list;
                if (!renderInfos.TryGetValue(current, out list))
                {
                    list = new Stack<List<RenderInfo>>();
                    renderInfos.Add(current, list);
                }
                else if(list.Count > 0) return true;
                return false;
            }
            set
            {
                Thread current = Thread.CurrentThread;
                Stack<List<RenderInfo>> list;
                if (!renderInfos.TryGetValue(current, out list))
                {
                    list = new Stack<List<RenderInfo>>();
                    if (value)
                    {
                        list.Push(new List<RenderInfo>());
                    }
                    renderInfos.Add(current, list);
                }
                else
                {
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
        }

        public static List<RenderInfo> RenderInfos
        {
            get
            {
                Stack<List<RenderInfo>> result;
                Thread current = Thread.CurrentThread;
                if (!renderInfos.TryGetValue(current, out result))
                {
                    result = new Stack<List<RenderInfo>>();
                    renderInfos.Add(current, result);
                }
                return result.Peek();
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
                    for (int j = 0; j < info.matrices.Length && j < info.count; ++j)
                        Graphics.DrawMesh(info.mesh, info.matrices[j], info.material, 31, camera, info.submeshIndex, info.properties, info.castShadows, info.receiveShadows, info.probeAnchor, info.lightProbeUsage, info.lightProbeProxyVolume);
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

        private static Dictionary<Thread, Stack<List<RenderInfo>>> renderInfos = new Dictionary<Thread, Stack<List<RenderInfo>>>();
        private static Camera camera = new Camera();
    }
}