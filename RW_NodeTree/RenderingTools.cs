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
                List<RenderInfo> result = null;
                Thread current = Thread.CurrentThread;
                if (!renderInfos.TryGetValue(current, out result))
                {
                    result = new List<RenderInfo>();
                    renderInfos.Add(current, result);
                }
                return result;
            }
        }

        public static void RenderToTarget(List<RenderInfo> infos, RenderTexture target, float size = 1)
        {
            RenderTexture cache = camera.targetTexture;
            camera.targetTexture = target;
            camera.orthographicSize = size;
            for (int i = 0; i < infos.Count; i++)
            {
                RenderInfo info = infos[i];
                info.matrix.m13 += 4096;
                Graphics.DrawMesh(info.mesh, info.matrix, info.material, 1 << 31, camera, info.submeshIndex);
            }
            camera.Render();
            camera.targetTexture = cache;
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
        public Matrix4x4 matrix;
        public Material material;

        public RenderInfo(Mesh mesh, int submeshIndex, Matrix4x4 matrix, Material material)
        {
            this.mesh = mesh;
            this.submeshIndex = submeshIndex;
            this.matrix = matrix;
            this.material = material;
        }
    }

    [HarmonyPatch(typeof(Graphics),"Internal_DrawMesh")]
    public static class RenderingPatchManager
    {
        public static bool Internal_DrawMesh_Catcher(Mesh mesh, int submeshIndex, Matrix4x4 matrix, Material material, int layer)
        {
            if(RenderingTools.BlockingState)
            {
                RenderingTools.RenderInfos.Add(new RenderInfo(mesh, submeshIndex, matrix, material));
                return false;
            }
            return true;
        }
    }
}