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

namespace RW_NodeTree.Patch
{
    [StaticConstructorOnStartup]
    public static class RenderingPatch
    {
        static RenderingPatch()
        {
            patcher = new Harmony("RW_NodeTree.Patch.RenderingPatch");
        }

        public static bool BlockingState
        {
            get
            {
                bool result = false;
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
                List<RenderInfo> list = null;
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

        private static Harmony patcher = null;
        private static Dictionary<Thread,bool> blockingState = new Dictionary<Thread,bool>();
        private static Dictionary<Thread, List<RenderInfo>> renderInfos = new Dictionary<Thread, List<RenderInfo>>();
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
            if(RenderingPatch.BlockingState)
            {
                RenderingPatch.RenderInfos.Add(new RenderInfo(mesh, submeshIndex, matrix, material));
                return false;
            }
            return true;
        }
    }
}