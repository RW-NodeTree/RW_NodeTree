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
using DataStructor;

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
                    camera.transform.position = new Vector3(0, CanvasHeight + 65, 0);
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

                LinkStack<List<RenderInfo>> list;
                if (!renderInfos.TryGetValue(current, out list))
                {
                    list = new LinkStack<List<RenderInfo>>();
                    renderInfos.Add(current, list);
                }
                if(list.Count > 0) return true;

                return false;
            }
            set
            {
                int current = Thread.CurrentThread.ManagedThreadId;

                LinkStack<List<RenderInfo>> list;
                if (!renderInfos.TryGetValue(current, out list))
                {
                    list = new LinkStack<List<RenderInfo>>();
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
                LinkStack<List<RenderInfo>> result;
                int current = Thread.CurrentThread.ManagedThreadId;
                if (!renderInfos.TryGetValue(current, out result))
                {
                    result = new LinkStack<List<RenderInfo>>();
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
        public static void RenderToTarget(List<RenderInfo> infos, ref RenderTexture cachedRenderTarget, ref Texture2D target, Vector2Int size = default(Vector2Int), int TextureSizeFactor = (int)DefaultTextureSizeFactor)
        {
            if (size.x <= 0 || size.y <= 0)
            {
                size = DrawSize(infos, TextureSizeFactor);
            }
            else
            {
                size.x = Mathf.Clamp(size.x, 1, MaxTexSize);
                size.y = Mathf.Clamp(size.y, 1, MaxTexSize);
            }

            Camera.Render();
            //if (Prefs.DevMode) Log.Message("RenderToTarget size:" + size);
            if (cachedRenderTarget == null ||
                cachedRenderTarget.width > MaxTexSize ||
                cachedRenderTarget.width > size.x + DefaultTextureSizeFactor ||
                target.width < size.x ||
                cachedRenderTarget.height > MaxTexSize ||
                cachedRenderTarget.height > size.y + DefaultTextureSizeFactor ||
                target.height < size.y
                )
            {
                if (cachedRenderTarget != null) GameObject.Destroy(cachedRenderTarget);
                cachedRenderTarget = new RenderTexture(size.x, size.y, 16, RenderTextureFormat.ARGB32);
            }
            size = new Vector2Int(cachedRenderTarget.width, cachedRenderTarget.height);
            Camera.targetTexture = cachedRenderTarget;
            Camera.orthographicSize = size.y / (float)(TextureSizeFactor << 1);
            for (int i = 0; i < infos.Count; i++)
            {
                RenderInfo info = infos[i];
                for (int j = 0; j < info.matrices.Length; ++j)
                {
                    info.matrices[j].m13 += CanvasHeight;
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
            //Camera.targetTexture = null;
            if (target == null ||
                target.width > MaxTexSize ||
                target.width != size.x ||
                target.height > MaxTexSize ||
                target.height != size.y
                )
            {
                if (target != null) GameObject.Destroy(target);
                target = new Texture2D(size.x, size.y, TextureFormat.ARGB32, false);
            }
            Graphics.CopyTexture(cachedRenderTarget, target);
            //RenderTexture cache = RenderTexture.active;
            //RenderTexture.active = render;

            //tex.ReadPixels(new Rect(0, 0, render.width, render.height), 0, 0);
            //tex.Apply();

            //RenderTexture.active = cache;
        }

        /// <summary>
        /// get the standard texture size of rendering infos
        /// </summary>
        /// <param name="infos">all arranged render infos</param>
        /// <returns>standard size of texture</returns>
        public static Vector2Int DrawSize(List<RenderInfo> infos, int TextureSizeFactor = (int)DefaultTextureSizeFactor)
        {
            Vector2Int result = default(Vector2Int);
            float TowTimesTextureSizeFactor = TextureSizeFactor << 1;
            foreach (RenderInfo info in infos)
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
                    result.x = (int)Math.Ceiling(Math.Max(result.x, Math.Abs(vert4d.x) * TowTimesTextureSizeFactor));
                    result.y = (int)Math.Ceiling(Math.Max(result.y, Math.Abs(vert4d.z) * TowTimesTextureSizeFactor));

                    //(-1, 1, 1)
                    extents.x *= -1;
                    vert3d = center + extents;
                    vert4d = matrix * new Vector4(vert3d.x, vert3d.y, vert3d.z, 1);
                    result.x = (int)Math.Ceiling(Math.Max(result.x, Math.Abs(vert4d.x) * TowTimesTextureSizeFactor));
                    result.y = (int)Math.Ceiling(Math.Max(result.y, Math.Abs(vert4d.z) * TowTimesTextureSizeFactor));

                    //(-1,-1, 1)
                    extents.y *= -1;
                    vert3d = center + extents;
                    vert4d = matrix * new Vector4(vert3d.x, vert3d.y, vert3d.z, 1);
                    result.x = (int)Math.Ceiling(Math.Max(result.x, Math.Abs(vert4d.x) * TowTimesTextureSizeFactor));
                    result.y = (int)Math.Ceiling(Math.Max(result.y, Math.Abs(vert4d.z) * TowTimesTextureSizeFactor));

                    //( 1,-1, 1)
                    extents.x *= -1;
                    vert3d = center + extents;
                    vert4d = matrix * new Vector4(vert3d.x, vert3d.y, vert3d.z, 1);
                    result.x = (int)Math.Ceiling(Math.Max(result.x, Math.Abs(vert4d.x) * TowTimesTextureSizeFactor));
                    result.y = (int)Math.Ceiling(Math.Max(result.y, Math.Abs(vert4d.z) * TowTimesTextureSizeFactor));

                    //( 1,-1,-1)
                    extents.z *= -1;
                    vert3d = center + extents;
                    vert4d = matrix * new Vector4(vert3d.x, vert3d.y, vert3d.z, 1);
                    result.x = (int)Math.Ceiling(Math.Max(result.x, Math.Abs(vert4d.x) * TowTimesTextureSizeFactor));
                    result.y = (int)Math.Ceiling(Math.Max(result.y, Math.Abs(vert4d.z) * TowTimesTextureSizeFactor));

                    //(-1,-1,-1)
                    extents.x *= -1;
                    vert3d = center + extents;
                    vert4d = matrix * new Vector4(vert3d.x, vert3d.y, vert3d.z, 1);
                    result.x = (int)Math.Ceiling(Math.Max(result.x, Math.Abs(vert4d.x) * TowTimesTextureSizeFactor));
                    result.y = (int)Math.Ceiling(Math.Max(result.y, Math.Abs(vert4d.z) * TowTimesTextureSizeFactor));

                    //(-1, 1,-1)
                    extents.y *= -1;
                    vert3d = center + extents;
                    vert4d = matrix * new Vector4(vert3d.x, vert3d.y, vert3d.z, 1);
                    result.x = (int)Math.Ceiling(Math.Max(result.x, Math.Abs(vert4d.x) * TowTimesTextureSizeFactor));
                    result.y = (int)Math.Ceiling(Math.Max(result.y, Math.Abs(vert4d.z) * TowTimesTextureSizeFactor));

                    //( 1, 1,-1)
                    extents.x *= -1;
                    vert3d = center + extents;
                    vert4d = matrix * new Vector4(vert3d.x, vert3d.y, vert3d.z, 1);
                    result.x = (int)Math.Ceiling(Math.Max(result.x, Math.Abs(vert4d.x) * TowTimesTextureSizeFactor));
                    result.y = (int)Math.Ceiling(Math.Max(result.y, Math.Abs(vert4d.z) * TowTimesTextureSizeFactor));


                    result.x = Mathf.Clamp(result.x, 1, MaxTexSize);
                    result.y = Mathf.Clamp(result.y, 1, MaxTexSize);
                }
            }
            return result;
        }

        private static Dictionary<int, LinkStack<List<RenderInfo>>> renderInfos = new Dictionary<int, LinkStack<List<RenderInfo>>>();
        private static Camera camera = null;
        internal static RenderTexture empty = new RenderTexture(1, 1, 0, RenderTextureFormat.ARGB32);
        public const int CanvasHeight = 4096;
        public const int MaxTexSize = 4096;
        public const int DefaultTextureSizeFactor = 128;
    }
}