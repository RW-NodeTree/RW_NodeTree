using System;
using System.Collections.Generic;
using System.Threading;
using Verse;
using UnityEngine;
using UnityEngine.Rendering;
using RW_NodeTree.DataStructure;

namespace RW_NodeTree.Rendering
{
    /// <summary>
    /// Offscreen rendering and Unity Graphics Draw Blocker in hear
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
                    GameObject gameObject = new GameObject("RW_NodeTree_Camera");
                    camera = gameObject.AddComponent<Camera>();
                    UnityEngine.Object.DontDestroyOnLoad(gameObject);
                    camera.orthographic = true;
                    camera.orthographicSize = 1;
                    camera.nearClipPlane = 0;
                    camera.farClipPlane = 71.5f;
                    camera.transform.position = new Vector3(0, 65 + CanvasHeight, 0);
                    camera.transform.rotation = Quaternion.Euler(90, 0, 0);
                    camera.backgroundColor = Color.clear;
                    camera.clearFlags = CameraClearFlags.SolidColor;
                    camera.targetTexture = empty;
                    camera.enabled = false;
                }
                return camera;
            }
        }


        internal static CommandBuffer CommandBuffer
        {
            get
            {
                if (commandBuffer == null)
                {
                    commandBuffer = new CommandBuffer();
                    commandBuffer.name = "RW_NodeTree_Renderer";
                }
                return commandBuffer;
            }
        }


        public static Color BackgroundColor
        {
            get => Camera.backgroundColor;
            set => Camera.backgroundColor = value;
        }


        public static bool AutoClear
        {
            get => Camera.clearFlags < CameraClearFlags.Nothing;
            set => Camera.clearFlags = value ? CameraClearFlags.SolidColor : CameraClearFlags.Nothing;
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
                lock(renderInfos)
                {
                    if (!renderInfos.TryGetValue(current, out list))
                    {
                        list = new LinkStack<List<RenderInfo>>();
                        renderInfos.Add(current, list);
                    }
                }
                if(list.Count > 0) return true;

                return false;
            }
            set
            {
                int current = Thread.CurrentThread.ManagedThreadId;

                LinkStack<List<RenderInfo>> list;
                lock (renderInfos)
                {
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
                lock (renderInfos)
                {
                    if (!renderInfos.TryGetValue(current, out result))
                    {
                        result = new LinkStack<List<RenderInfo>>();
                        renderInfos.Add(current, result);
                    }
                    return result.Peek();
                }
            }
        }



        /// <summary>
        /// Render a perview texture by infos
        /// </summary>
        /// <param name="infos">all arranged render infos</param>
        /// <param name="cachedRenderTarget"></param>
        /// <param name="target"></param>
        /// <param name="size">force render texture size</param>
        /// <param name="TextureSizeFactor"></param>
        public static void RenderToTarget(List<RenderInfo> infos, ref RenderTexture cachedRenderTarget, ref Texture2D target, Vector2Int size = default(Vector2Int), uint TextureSizeFactor = DefaultTextureSizeFactor, float ExceedanceFactor = 1f, float ExceedanceOffset = 1f, Action<RenderTexture> PostFX = null)
        {
            RenderToTarget(infos, ref cachedRenderTarget, size, TextureSizeFactor, ExceedanceFactor, ExceedanceOffset);
            PostFX?.Invoke(cachedRenderTarget);
            if (target == null || target.width != cachedRenderTarget.width || target.height != cachedRenderTarget.height)
            {
                if (target != null) GameObject.Destroy(target);
                target = new Texture2D(cachedRenderTarget.width, cachedRenderTarget.height, TextureFormat.ARGB32, false);
            }
            //Camera.targetTexture = null;
            Graphics.CopyTexture(cachedRenderTarget, target);
            //target.Apply();
            //GameObject.Destroy(cachedRenderTarget);
            //RenderTexture cache = RenderTexture.active;
            //RenderTexture.active = render;

            //tex.ReadPixels(new Rect(0, 0, render.width, render.height), 0, 0);
            //tex.Apply();

            //RenderTexture.active = cache;
        }


        /// <summary>
        /// Render a perview texture by infos
        /// </summary>
        /// <param name="infos">all arranged render infos</param>
        /// <param name="cachedRenderTarget"></param>
        /// <param name="size">force render texture size</param>
        /// <param name="TextureSizeFactor"></param>
        public static void RenderToTarget(List<RenderInfo> infos, ref RenderTexture cachedRenderTarget, Vector2Int size = default(Vector2Int), uint TextureSizeFactor = DefaultTextureSizeFactor, float ExceedanceFactor = 1f, float ExceedanceOffset = 1f)
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

            size = CheckAndResizeRenderTexture(ref cachedRenderTarget, size, TextureSizeFactor, ExceedanceFactor, ExceedanceOffset);

            //if (Prefs.DevMode) Log.Message("RenderToTarget size:" + size);
            //Debug.Log("RenderToTarget size:" + size);

            if(CanUseFastDraw(infos))
            {
                CommandBuffer.Clear();

                float TowTimesTextureSizeFactor = TextureSizeFactor << 1;
                float hw = size.x / TowTimesTextureSizeFactor, hh = size.y / TowTimesTextureSizeFactor;

                Matrix4x4 view = Matrix4x4.TRS(new Vector3(0, 65, 0), Quaternion.Euler(90, 0, 0), Vector3.one);
                Matrix4x4.Inverse3DAffine(view, ref view);

                Matrix4x4 perspective = Matrix4x4.Ortho(-hw, hw, -hh, hh, 0, 71.5f);
                //perspective.m32 *= -1;
                perspective.m22 *= -1;

                CommandBuffer.SetViewProjectionMatrices(view, perspective);
                CommandBuffer.SetRenderTarget(cachedRenderTarget);

                if (AutoClear) CommandBuffer.ClearRenderTarget(true, true, BackgroundColor);
                for (int i = 0; i < infos.Count; i++)
                {
                    infos[i].DrawInfoFast(CommandBuffer);
                }
                Graphics.ExecuteCommandBuffer(CommandBuffer);
            }
            else
            {
                Camera.targetTexture = cachedRenderTarget;
                Camera.orthographicSize = size.y / (float)(TextureSizeFactor << 1);
                for (int i = 0; i < infos.Count; i++)
                {
                    RenderInfo info = infos[i];
                    Matrix4x4[] matrices = new Matrix4x4[info.matrices.Length];
                    Array.Copy(info.matrices, matrices, matrices.Length);
                    info.matrices = matrices;
                    for (int j = 0; j < info.matrices.Length; ++j)
                    {
                        info.matrices[j].m13 += RenderingTools.CanvasHeight;
                    }
                    info.DrawInfo(Camera);
                }
                //Camera.Render();
                Camera.Render();
                Camera.targetTexture = empty;
            }
        }


        public static Vector2Int CheckAndResizeRenderTexture(ref RenderTexture renderTexture, Vector2Int size, uint TextureSizeFactor = DefaultTextureSizeFactor, float ExceedanceFactor = 1f, float ExceedanceOffset = 1f)
        {
            if (renderTexture != null)
            {
                ExceedanceFactor = Math.Max(ExceedanceFactor, 1f);
                if (renderTexture.width <= MaxTexSize &&
                    renderTexture.width <= size.x * ExceedanceFactor + TextureSizeFactor * ExceedanceOffset &&
                    renderTexture.width >= size.x
                    )
                {
                    size.x = renderTexture.width;
                }
                if (renderTexture.height <= MaxTexSize &&
                    renderTexture.height <= size.y * ExceedanceFactor + TextureSizeFactor * ExceedanceOffset &&
                    renderTexture.height >= size.y
                    )
                {
                    size.y = renderTexture.height;
                }
            }
            if (renderTexture == null || renderTexture.width != size.x || renderTexture.height != size.y)
            {
                if (renderTexture != null) GameObject.Destroy(renderTexture);
                renderTexture = new RenderTexture(size.x, size.y, 16, RenderTextureFormat.ARGB32);
            }
            return size;
        }

        /// <summary>
        /// get the standard texture size of rendering infos
        /// </summary>
        /// <param name="infos">all arranged render infos</param>
        /// <param name="TextureSizeFactor"></param>
        /// <returns>standard size of texture</returns>
        public static Vector2Int DrawSize(List<RenderInfo> infos, uint TextureSizeFactor = DefaultTextureSizeFactor)
        {
            Matrix4x4 camearMatrix = Matrix4x4.TRS(new Vector3(0, 65, 0), Quaternion.Euler(90, 0, 0), Vector3.one);
            Matrix4x4.Inverse3DAffine(camearMatrix, ref camearMatrix);
            Vector2Int result = default(Vector2Int);
            float TowTimesTextureSizeFactor = TextureSizeFactor << 1;
            //camearMatrix.m13 += CanvasHeight;
            foreach (RenderInfo info in infos)
            {
                Bounds bounds = info.mesh.bounds;

                Vector3 center = bounds.center;


                for (int i = 0; i < info.matrices.Length && i < info.count; ++i)
                {
                    Matrix4x4 matrix = camearMatrix * info.matrices[i];
                    //calculate 8 vertex of the border cube
                    //( 1, 1, 1)
                    Vector3 extents = bounds.extents;
                    Vector3 vert3d = center + extents;
                    Vector4 vert4d = matrix * new Vector4(vert3d.x, vert3d.y, vert3d.z, 1);
                    result.x = (int)Math.Ceiling(Math.Max(result.x, Math.Abs(vert4d.x) * TowTimesTextureSizeFactor));
                    result.y = (int)Math.Ceiling(Math.Max(result.y, Math.Abs(vert4d.y) * TowTimesTextureSizeFactor));

                    //(-1, 1, 1)
                    extents.x *= -1;
                    vert3d = center + extents;
                    vert4d = matrix * new Vector4(vert3d.x, vert3d.y, vert3d.z, 1);
                    result.x = (int)Math.Ceiling(Math.Max(result.x, Math.Abs(vert4d.x) * TowTimesTextureSizeFactor));
                    result.y = (int)Math.Ceiling(Math.Max(result.y, Math.Abs(vert4d.y) * TowTimesTextureSizeFactor));

                    //(-1,-1, 1)
                    extents.y *= -1;
                    vert3d = center + extents;
                    vert4d = matrix * new Vector4(vert3d.x, vert3d.y, vert3d.z, 1);
                    result.x = (int)Math.Ceiling(Math.Max(result.x, Math.Abs(vert4d.x) * TowTimesTextureSizeFactor));
                    result.y = (int)Math.Ceiling(Math.Max(result.y, Math.Abs(vert4d.y) * TowTimesTextureSizeFactor));

                    //( 1,-1, 1)
                    extents.x *= -1;
                    vert3d = center + extents;
                    vert4d = matrix * new Vector4(vert3d.x, vert3d.y, vert3d.z, 1);
                    result.x = (int)Math.Ceiling(Math.Max(result.x, Math.Abs(vert4d.x) * TowTimesTextureSizeFactor));
                    result.y = (int)Math.Ceiling(Math.Max(result.y, Math.Abs(vert4d.y) * TowTimesTextureSizeFactor));

                    //( 1,-1,-1)
                    extents.z *= -1;
                    vert3d = center + extents;
                    vert4d = matrix * new Vector4(vert3d.x, vert3d.y, vert3d.z, 1);
                    result.x = (int)Math.Ceiling(Math.Max(result.x, Math.Abs(vert4d.x) * TowTimesTextureSizeFactor));
                    result.y = (int)Math.Ceiling(Math.Max(result.y, Math.Abs(vert4d.y) * TowTimesTextureSizeFactor));

                    //(-1,-1,-1)
                    extents.x *= -1;
                    vert3d = center + extents;
                    vert4d = matrix * new Vector4(vert3d.x, vert3d.y, vert3d.z, 1);
                    result.x = (int)Math.Ceiling(Math.Max(result.x, Math.Abs(vert4d.x) * TowTimesTextureSizeFactor));
                    result.y = (int)Math.Ceiling(Math.Max(result.y, Math.Abs(vert4d.y) * TowTimesTextureSizeFactor));

                    //(-1, 1,-1)
                    extents.y *= -1;
                    vert3d = center + extents;
                    vert4d = matrix * new Vector4(vert3d.x, vert3d.y, vert3d.z, 1);
                    result.x = (int)Math.Ceiling(Math.Max(result.x, Math.Abs(vert4d.x) * TowTimesTextureSizeFactor));
                    result.y = (int)Math.Ceiling(Math.Max(result.y, Math.Abs(vert4d.y) * TowTimesTextureSizeFactor));

                    //( 1, 1,-1)
                    extents.x *= -1;
                    vert3d = center + extents;
                    vert4d = matrix * new Vector4(vert3d.x, vert3d.y, vert3d.z, 1);
                    result.x = (int)Math.Ceiling(Math.Max(result.x, Math.Abs(vert4d.x) * TowTimesTextureSizeFactor));
                    result.y = (int)Math.Ceiling(Math.Max(result.y, Math.Abs(vert4d.y) * TowTimesTextureSizeFactor));


                    result.x = Mathf.Clamp(result.x, 1, MaxTexSize);
                    result.y = Mathf.Clamp(result.y, 1, MaxTexSize);
                }
            }
            return result;
        }


        public static bool CanUseFastDraw(List<RenderInfo> infos)
        {
            foreach (var info in infos)
            {
                //Log.Message(info.ToString());
                if (!info.CanUseFastDrawingMode) return false;
            }
            return true;
        }

        private static Camera camera = null;
        private static CommandBuffer commandBuffer = null;
        internal static RenderTexture empty = new RenderTexture(1, 1, 0, RenderTextureFormat.ARGB32);
        private static readonly Dictionary<int, LinkStack<List<RenderInfo>>> renderInfos = new Dictionary<int, LinkStack<List<RenderInfo>>>();
        public const float CanvasHeight = 4096;
        public const int MaxTexSize = 4096;
        public const uint DefaultTextureSizeFactor = 128;
    }
}