using RW_NodeTree.Rendering;
using RW_NodeTree.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RW_NodeTree
{
    /// <summary>
    /// Node function proccesser
    /// </summary>
    public partial class CompChildNodeProccesser : ThingComp, IThingHolder
    {

        public CompProperties_ChildNodeProccesser Props => (CompProperties_ChildNodeProccesser)props;


        public bool NeedUpdate
        {
            get => ChildNodes.NeedUpdate;
            set => ChildNodes.NeedUpdate = value;
        }


        public bool HasPostFX(bool textureMode)
        {
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                try
                {
                    if (comp.HasPostFX(textureMode)) return true;
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
            return false;
        }

        /// <summary>
        /// node container
        /// </summary>
        public NodeContainer ChildNodes => (NodeContainer)GetDirectlyHeldThings();


        /// <summary>
        /// get parent node if it is a node
        /// </summary>
        public CompChildNodeProccesser? ParentProccesser => this.ParentHolder as CompChildNodeProccesser;

        /// <summary>
        /// root of this node tree
        /// </summary>
        public CompChildNodeProccesser RootNode
        {
            get
            {
                if (cachedRootNode != null) return cachedRootNode;
                CompChildNodeProccesser proccesser = this;
                CompChildNodeProccesser? next = ParentProccesser;
                while (next != null)
                {
                    proccesser = next;
                    next = next.ParentProccesser;
                }
                cachedRootNode = proccesser;
                return proccesser;
            }
        }


        /// <summary>
        /// find all comp for node
        /// </summary>
        public IEnumerable<CompBasicNodeComp> AllNodeComp
        {
            get
            {
                for (int i = 0; i < parent.AllComps.Count; i++)
                {
                    ThingComp comp = parent.AllComps[i];
                    CompBasicNodeComp? c = comp as CompBasicNodeComp;
                    if (c != null)
                    {
                        yield return c;
                    }
                }
                yield break;
            }
        }

        public override bool AllowStackWith(Thing? other)
        {
            CompChildNodeProccesser? comp = other;
            if (comp != null)
            {
                return comp.ChildNodes.Count == 0 && ChildNodes.Count == 0;
            }
            return false;
        }

        public override void CompTick()
        {
#if V13 || V14 || V15
            ChildNodes.ThingOwnerTick();
#else
            ChildNodes.DoTick();
#endif
            UpdateNode();
        }


        /// <summary>
        /// set all texture need regenerate
        /// </summary>
        public void ResetRenderedTexture()
        {
            lock (nodeRenderingInfo)
            {
                nodeRenderingInfo.Reset();
                try
                {
                    if (parent.Spawned && parent.def.drawerType >= DrawerType.MapMeshOnly) parent.DirtyMapMesh(parent.Map);
                }
                catch (Exception ex)
                {
                    Log.Warning(ex.ToString());
                }
            }
            ParentProccesser?.ResetRenderedTexture();
        }



        /// <summary>
        /// Rimworld Defined method, used for load and save game saves.
        /// </summary>
        public override void PostExposeData()
        {
            Scribe_Deep.Look(ref this.childNodes, "innerContainer", this);
            //if (Scribe.mode == LoadSaveMode.PostLoadInit) UpdateNode();
            //Scribe_Collections.Look(ref childNodes, "innerContainer", LookMode.Deep, this);
        }


        internal void PostFX(RenderTexture tar)
        {
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                try
                {
                    if (comp.HasPostFX(true)) comp.PostFX(tar);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
        }


        public List<(string?, Thing, List<RenderInfo>)> GetNodeRenderingInfos(Rot4 rot, out bool updated, Graphic? subGraphic = null)
        {
            UpdateNode();
            updated = false;
            if (!UnityData.IsInMainThread) throw new InvalidOperationException("not in main thread");
            List<(string?, Thing, List<RenderInfo>)>? nodeRenderingInfos = this.nodeRenderingInfo[rot];
            if (nodeRenderingInfos != null) return nodeRenderingInfos;
            updated = true;
            nodeRenderingInfos = new List<(string?, Thing, List<RenderInfo>)>(ChildNodes.Count + 1);

            //if (Prefs.DevMode)
            //{
            //    StackTrace stack = new StackTrace();
            //    string stackReport = "";
            //    for(int i =0; i < 8; i++)
            //    {
            //        StackFrame sf = stack.GetFrame(i);
            //        MethodBase method = sf.GetMethod();
            //        stackReport += method.DeclaringType + " -> " + method + " " + sf + "\n";
            //    }
            //    Log.Message(parent + " graphic : " + parent.Graphic + ";\nstack : " + stackReport);
            //}


            //ORIGIN
            subGraphic = (subGraphic ?? parent.Graphic)?.GetGraphic_ChildNode()?.SubGraphic ?? subGraphic ?? parent.Graphic;
            if (subGraphic != null)
            {
                RenderingTools.StartOrEndDrawCatchingBlock = true;
                try
                {
                    subGraphic.Draw(Vector3.zero, rot, parent);
                    nodeRenderingInfos.Add((null, this!, RenderingTools.RenderInfos!));
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
                RenderingTools.StartOrEndDrawCatchingBlock = false;
            }

            NodeContainer container = ChildNodes;
            for (int i = 0; i < container.Count; i++)
            {
                Thing child = container[i];

                if (child != null)
                {
                    nodeRenderingInfos.Add((((IList<string>)container)[i], child, new List<RenderInfo>()));
                }
            }

            Dictionary<string, object?> cachingData = new Dictionary<string, object?>();

            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                try
                {
                    nodeRenderingInfos = comp.internal_PreDrawStep(nodeRenderingInfos, rot, subGraphic, cachingData) ?? nodeRenderingInfos;
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }

            for (int i = 0; i < nodeRenderingInfos.Count; i++)
            {
                string? id = nodeRenderingInfos[i].Item1;
                Thing child = nodeRenderingInfos[i].Item2;
                List<RenderInfo> infos = nodeRenderingInfos[i].Item3 ?? new List<RenderInfo>();
                if (child != null && id != null)
                {
                    RenderingTools.StartOrEndDrawCatchingBlock = true;
                    try
                    {
                        Rot4 rotCache = child.Rotation;
                        child.Rotation = new Rot4((rot.AsInt + rotCache.AsInt) & 3);
#if V13 || V14
                        child.DrawAt(Vector3.zero);
#else
                        child.DrawNowAt(Vector3.zero);
#endif
                        child.Rotation = rotCache;
                        infos.AddRange(RenderingTools.RenderInfos);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.ToString());
                    }
                    RenderingTools.StartOrEndDrawCatchingBlock = false;
                    nodeRenderingInfos[i] = (id, child, infos);
                }
            }

            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                try
                {
                    nodeRenderingInfos = comp.internal_PostDrawStep(nodeRenderingInfos, rot, subGraphic, cachingData) ?? nodeRenderingInfos;
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
            this.nodeRenderingInfo[rot] = nodeRenderingInfos;
            return nodeRenderingInfos;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool AllowNode(Thing? node, string? id = null)
        {
            if (node?.holdingOwner != null) return false;
            if (node?.Destroyed ?? false) return false;
            if (ChildNodes.IsChildOf(node)) return false;
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                try
                {
                    if (!comp.internal_AllowNode(node, id)) return false;
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
            return true;
        }

        /// <summary>
        /// Update node tree
        /// </summary>
        /// <returns></returns>
        public void UpdateNode() => ChildNodes.internal_UpdateNode();



        public void ResetCachedRootNode()
        {
            cachedRootNode = null;
            foreach (Thing? part in ChildNodes.Values)
            {
                CompChildNodeProccesser? childComp = part;
                if (childComp != null) childComp.ResetCachedRootNode();
            }
        }

        internal void internal_Added(NodeContainer container, string? id, bool success, Dictionary<string, object?> cachedData)
        {
            ResetCachedRootNode();
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                try
                {
                    comp.internal_Added(container, id, success, cachedData);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
        }

        internal void internal_Removed(NodeContainer container, string? id, bool success, Dictionary<string, object?> cachedData)
        {
            ResetCachedRootNode();
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                try
                {
                    comp.internal_Removed(container, id, success, cachedData);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.ChildNodes);
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            lock (this)
            {
                if (childNodes == null)
                {
                    childNodes = new NodeContainer(this);
                }
                return childNodes;
            }
        }



        #region operator
        public static implicit operator Thing?(CompChildNodeProccesser? node)
        {
            return node?.parent;
        }


        public static implicit operator CompChildNodeProccesser?(Thing? thing)
        {
            List<ThingComp>? comps = (thing as ThingWithComps)?.AllComps;
            if (comps == null || comps.Count < 1) return null;
            CompChildNodeProccesser? result = comps[0] as CompChildNodeProccesser;
            if (result != null) return result;
            retry:;
            if (!compLoadingCache.TryGetValue(thing!.def, out int index))
            {
                int i = 0;
                for (; i < comps.Count; i++)
                {
                    result = comps[i] as CompChildNodeProccesser;
                    if (result != null) break;
                }
                if (result != null)
                {
                    index = i;
                }
                else
                {
                    index = -1;
                }
                compLoadingCache.Add(thing.def, index);
            }
            else if (index >= 0)
            {
                if (index >= comps.Count)
                {
                    compLoadingCache.Remove(thing.def);
                    goto retry;
                }
                result = comps[index] as CompChildNodeProccesser;
                if (result == null)
                {
                    compLoadingCache.Remove(thing.def);
                    goto retry;
                }
            }
            return result;
        }
        #endregion


        public class NodeRenderingInfoForRot4
        {
            public List<(string?, Thing, List<RenderInfo>)>? this[Rot4 rot]
            {
                get => nodeRenderingInfos[rot.AsInt];
                set => nodeRenderingInfos[rot.AsInt] = value;
            }
            public void Reset()
            {
                for (int i = 0; i < nodeRenderingInfos.Length; i++) nodeRenderingInfos[i] = null;
            }
            public readonly List<(string?, Thing, List<RenderInfo>)>?[] nodeRenderingInfos = new List<(string?, Thing, List<RenderInfo>)>?[4];
        }

        private CompChildNodeProccesser? cachedRootNode;

        private NodeContainer? childNodes;

        internal readonly NodeRenderingInfoForRot4 nodeRenderingInfo = new NodeRenderingInfoForRot4();

        private readonly static Dictionary<ThingDef, int> compLoadingCache = new Dictionary<ThingDef, int>();

        /*
        private static Matrix4x4 matrix =
                            new Matrix4x4(
                                new Vector4(     1,      0,      0,      0      ),
                                new Vector4(     0,      0,     -0.001f, 0      ),
                                new Vector4(     0,      1,      0,      0      ),
                                new Vector4(     0,      0,      0.5f,   1      )
                            );
        */


    }

    [StaticConstructorOnStartup]
    public class CompProperties_ChildNodeProccesser : CompProperties
    {
        public CompProperties_ChildNodeProccesser()
        {
            base.compClass = typeof(CompChildNodeProccesser);
        }


        static CompProperties_ChildNodeProccesser()
        {
            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs)
            {
                if (def.comps.Count < 1) continue;
                for (int i = 0; i < def.comps.Count; i++)
                {
                    CompProperties properties = def.comps[i];
                    if (properties.compClass == typeof(CompChildNodeProccesser))
                    {
                        def.comps.RemoveAt(i);
                        def.comps.Insert(0, properties);
                        break;
                    }
                }
            }
        }
        public override void ResolveReferences(ThingDef parentDef)
        {
            parentDef.comps.Remove(this);
            parentDef.comps.Insert(0, this);
            for (int i = parentDef.comps.Count - 1; i >= 1; i--)
            {
                for (int j = i - 1; j >= 1; j--)
                {
                    if (parentDef.comps[i].compClass == parentDef.comps[j].compClass)
                    {
                        parentDef.comps.RemoveAt(j);
                    }
                }
            }
        }
        public float ExceedanceFactor = 1f;
        public float ExceedanceOffset = 1f;
        public uint TextureSizeFactor = RenderingTools.DefaultTextureSizeFactor;
        public FilterMode TextureFilterMode = FilterMode.Bilinear;
    }
}
