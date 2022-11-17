using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using Verse;
using RW_NodeTree.Rendering;
using RW_NodeTree.Tools;
using System.Diagnostics;
using System.Reflection;

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

        /// <summary>
        /// node container
        /// </summary>
        public NodeContainer ChildNodes => (NodeContainer)GetDirectlyHeldThings();


        /// <summary>
        /// get parent node if it is a node
        /// </summary>
        public CompChildNodeProccesser ParentProccesser => this.ParentHolder as CompChildNodeProccesser;

        /// <summary>
        /// root of this node tree
        /// </summary>
        public CompChildNodeProccesser RootNode
        {
            get
            {
                if(cachedRootNode != null) return cachedRootNode;
                CompChildNodeProccesser proccesser = this;
                CompChildNodeProccesser next = ParentProccesser;
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
                    CompBasicNodeComp c = comp as CompBasicNodeComp;
                    if (c != null)
                    {
                        yield return c;
                    }
                }
                yield break;
            }
        }

        public HashSet<string> RegiestedNodeId
        {
            get
            {
                if (regiestedNodeId.Count <= 0)
                {
                    HashSet<string> cache = new HashSet<string>();
                    foreach (CompBasicNodeComp comp in AllNodeComp)
                    {
                        cache = comp.internal_RegiestedNodeId(cache) ?? cache;
                    }
                    regiestedNodeId.AddRange(cache);
                }
                return new HashSet<string>(regiestedNodeId);
            }
        }

        public override bool AllowStackWith(Thing other)
        {
            CompChildNodeProccesser comp = other;
            if(comp != null)
            {
                return comp.ChildNodes.Count == 0 && ChildNodes.Count == 0;
            }
            return false;
        }

        public override void CompTick()
        {
            if(parent.def.tickerType == TickerType.Normal) UpdateNode();
            ChildNodes.ThingOwnerTick();
            IList<Thing> list = ChildNodes;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                Thing t = list[i];
                if (t.def.tickerType == TickerType.Never)
                {
                    if((t is IVerbOwner) || (t as ThingWithComps)?.AllComps.Find(x => x is IVerbOwner) != null || (CompChildNodeProccesser)t != null)
                    {
                        t.Tick();
                        if (t.Destroyed)
                        {
                            list.Remove(t);
                        }
                    }
                }
            }
            if (Find.TickManager.TicksGame % 250 == 0)
            {
                CompTickRare();
            }
        }

        public override void CompTickRare()
        {
            if (parent.def.tickerType == TickerType.Rare) UpdateNode();
            ChildNodes.ThingOwnerTickRare();
            if (Find.TickManager.TicksGame % 2000 < 250)
            {
                CompTickLong();
            }
        }

        public override void CompTickLong()
        {
            if (parent.def.tickerType == TickerType.Long) UpdateNode();
            ChildNodes.ThingOwnerTickLong();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ownerType"></param>
        /// <param name="thing"></param>
        /// <param name="verb"></param>
        /// <param name="tool"></param>
        /// <param name="verbProperties"></param>
        /// <returns></returns>
        public static bool CheckVerbDatasVaildityAndAdapt(Type ownerType, Thing thing, ref Verb verb, ref Tool tool, ref VerbProperties verbProperties)
        {
            if (ownerType == null || !typeof(IVerbOwner).IsAssignableFrom(ownerType) || thing == null || (verb == null && tool == null && verbProperties == null)) return false;

            List<VerbProperties> verbsPropertiesCache = tool?.VerbsProperties.ToList();
            List<Verb> allVerbs = GetOriginalAllVerbs(GetSameTypeVerbOwner(ownerType, thing)?.VerbTracker);

            if (verb != null)
            {
                if (allVerbs != null && !allVerbs.Contains(verb))
                {
                    return false;
                }
                else
                {
                    tool = verb.tool;
                    verbProperties = verb.verbProps;
                }
            }
            else if (verbsPropertiesCache != null && (verbProperties == null || !verbsPropertiesCache.Contains(verbProperties)))
            {
                verbProperties = verbsPropertiesCache.FirstOrDefault();
            }

            if (allVerbs != null)
            {
                Tool toolCache = tool;
                VerbProperties verbPropertiesCache = verbProperties;
                verb = allVerbs.Find(x => x.tool == toolCache && x.verbProps == verbPropertiesCache);
                if (verb == null)
                {
                    return false;
                }
            }
            return true;
        }


        /// <summary>
        /// Return the correct verb ownner and complemented before&after verb
        /// </summary>
        /// <param name="verbAfterConvert">Verb after convert</param>
        /// <returns>Verb infos before convert</returns>
        public (Thing, Verb, Tool, VerbProperties) GetBeforeConvertVerbCorrespondingThing(Type ownerType, Verb verbAfterConvert, bool needVerb = false)
        {
            return GetBeforeConvertVerbCorrespondingThing(ownerType, verbAfterConvert, null, null, needVerb);
        }


        /// <summary>
        /// Return the correct verb ownner and complemented before&after verb info
        /// </summary>
        /// <param name="verbPropertiesAfterConvert">verbProperties of verbAfterConvert</param>
        /// <param name="toolAfterConvert">tool of verbAfterConvert</param>
        /// <returns>Verb infos before convert</returns>
        public (Thing, Verb, Tool, VerbProperties) GetBeforeConvertVerbCorrespondingThing(Type ownerType, Tool toolAfterConvert, VerbProperties verbPropertiesAfterConvert, bool needVerb = false)
        {
            return GetBeforeConvertVerbCorrespondingThing(ownerType, null, toolAfterConvert, verbPropertiesAfterConvert, needVerb);
        }


        /// <summary>
        /// Return the correct verb ownner and complemented before&after verb info
        /// </summary>
        /// <param name="verbAfterConvert">Verb after convert</param>
        /// <param name="verbPropertiesAfterConvert">verbProperties of verbAfterConvert</param>
        /// <param name="toolAfterConvert">tool of verbAfterConvert</param>
        /// <returns>Verb infos before convert</returns>
        public (Thing, Verb, Tool, VerbProperties) GetBeforeConvertVerbCorrespondingThing(Type ownerType, Verb verbAfterConvert, Tool toolAfterConvert, VerbProperties verbPropertiesAfterConvert, bool needVerb = false)
        {
            (Thing, Verb, Tool, VerbProperties) result = default((Thing, Verb, Tool, VerbProperties));

            if (!CheckVerbDatasVaildityAndAdapt(ownerType, parent, ref verbAfterConvert, ref toolAfterConvert, ref verbPropertiesAfterConvert)) return result;

            Dictionary<(Thing, Verb, Tool, VerbProperties, bool), (Thing, Verb, Tool, VerbProperties)> caches;
            if (!BeforeConvertVerbCorrespondingThingCache.TryGetValue(ownerType, out caches))
            {
                caches = new Dictionary<(Thing, Verb, Tool, VerbProperties, bool), (Thing, Verb, Tool, VerbProperties)>();
                BeforeConvertVerbCorrespondingThingCache.Add(ownerType, caches);
            }

            if (caches.TryGetValue((parent, verbAfterConvert, toolAfterConvert, verbPropertiesAfterConvert, needVerb), out result)) return result;
            result = (parent, verbAfterConvert, toolAfterConvert, verbPropertiesAfterConvert);

            if (ownerType != null && typeof(IVerbOwner).IsAssignableFrom(ownerType) && verbPropertiesAfterConvert != null)
            {
                (Thing, Verb, Tool, VerbProperties) cache = result;
                if(toolAfterConvert != null)
                {
                    List<VerbToolRegiestInfo> Registed = GetRegiestedNodeVerbToolInfos(ownerType);
                    for (int i = 0; i < Registed.Count; i++)
                    {
                        VerbToolRegiestInfo regiestInfo = Registed[i];
                        if (regiestInfo.afterCobvertTool == toolAfterConvert)
                        {
                            cache = (ChildNodes[regiestInfo.id] ?? parent, null, regiestInfo.berforConvertTool, null);
                            break;
                        }
                    }
                }
                else
                {
                    List<VerbPropertiesRegiestInfo> Registed = GetRegiestedNodeVerbPropertiesInfos(ownerType);
                    for (int i = 0; i < Registed.Count; i++)
                    {
                        VerbPropertiesRegiestInfo regiestInfo = Registed[i];
                        if (regiestInfo.afterConvertProperties == verbPropertiesAfterConvert)
                        {
                            cache = (ChildNodes[regiestInfo.id] ?? parent, null, null, regiestInfo.berforConvertProperties);
                            break;
                        }
                    }
                }
                //if (Prefs.DevMode) Log.Message(cache.ToString());

                if (!CheckVerbDatasVaildityAndAdapt(ownerType, cache.Item1, ref cache.Item2, ref cache.Item3, ref cache.Item4)) return result;

                if (needVerb && cache.Item2 == null) return result;

                result = cache;

                if (result.Item1 != null && result.Item1 != parent && ((CompChildNodeProccesser)result.Item1) != null)
                {
                    Thing before = result.Item1;
                    result = ((CompChildNodeProccesser)result.Item1).GetBeforeConvertVerbCorrespondingThing(ownerType, result.Item2, result.Item3, result.Item4, needVerb);
                    result.Item1 = result.Item1 ?? before;
                }
            }
            caches.Add((parent, verbAfterConvert, toolAfterConvert, verbPropertiesAfterConvert, needVerb), result);
            return result;
        }


        /// <summary>
        /// Return the correct verb ownner and complemented before&after verb
        /// </summary>
        /// <param name="verbOwner">Verb container</param>
        /// <param name="verbBeforeConvert">Verb before convert</param>
        /// <param name="verbAfterConvert">Verb after convert</param>
        /// <returns>correct verb ownner</returns>
        public (Thing, Verb, Tool, VerbProperties) GetAfterConvertVerbCorrespondingThing(Type ownerType, Verb verbBeforeConvert)
        {
            return GetAfterConvertVerbCorrespondingThing(ownerType, verbBeforeConvert, null, null);
        }


        /// <summary>
        /// Return the correct verb ownner and complemented before&after verb info
        /// </summary>
        /// <param name="verbOwner">Verb container</param>
        /// <param name="verbPropertiesBeforeConvert">verbProperties of verbBeforeConvert</param>
        /// <param name="toolBeforeConvert">tool of verbBeforeConvert</param>
        /// <param name="verbPropertiesAfterConvert">verbProperties of verbAfterConvert</param>
        /// <param name="toolAfterConvert">tool of verbAfterConvert</param>
        /// <returns>correct verb ownner</returns>
        public (Thing, Verb, Tool, VerbProperties) GetAfterConvertVerbCorrespondingThing(Type ownerType, Tool toolBeforeConvert, VerbProperties verbPropertiesBeforeConvert)
        {
            return GetAfterConvertVerbCorrespondingThing(ownerType, null, toolBeforeConvert, verbPropertiesBeforeConvert);
        }


        /// <summary>
        /// Return the correct verb ownner and complemented before&after verb info
        /// </summary>
        /// <param name="ownerType">Verb container</param>
        /// <param name="verbBeforeConvert">Verb before convert</param>
        /// <param name="verbPropertiesBeforeConvert">verbProperties of verbBeforeConvert</param>
        /// <param name="toolBeforeConvert">tool of verbBeforeConvert</param>
        /// <returns>correct verb ownner</returns>
        public (Thing, Verb, Tool, VerbProperties) GetAfterConvertVerbCorrespondingThing(Type ownerType, Verb verbBeforeConvert, Tool toolBeforeConvert, VerbProperties verbPropertiesBeforeConvert)
        {
            (Thing, Verb, Tool, VerbProperties) result = default((Thing, Verb, Tool, VerbProperties));

            if (!CheckVerbDatasVaildityAndAdapt(ownerType, parent, ref verbBeforeConvert, ref toolBeforeConvert, ref verbPropertiesBeforeConvert)) return result;

            result = (parent, verbBeforeConvert, toolBeforeConvert, verbPropertiesBeforeConvert);

            if (ownerType != null && typeof(IVerbOwner).IsAssignableFrom(ownerType) && verbPropertiesBeforeConvert != null)
            {
                (Thing, Verb, Tool, VerbProperties) cache = result;
                if(ParentProccesser != null)
                {
                    if (toolBeforeConvert != null)
                    {
                        List<VerbToolRegiestInfo> Registed = GetRegiestedNodeVerbToolInfos(ownerType);
                        for (int i = 0; i < Registed.Count; i++)
                        {
                            VerbToolRegiestInfo regiestInfo = Registed[i];
                            if (regiestInfo.berforConvertTool == toolBeforeConvert)
                            {
                                cache = (ParentProccesser, null, regiestInfo.afterCobvertTool, null);
                                break;
                            }
                        }
                    }
                    else
                    {
                        List<VerbPropertiesRegiestInfo> Registed = GetRegiestedNodeVerbPropertiesInfos(ownerType);
                        for (int i = 0; i < Registed.Count; i++)
                        {
                            VerbPropertiesRegiestInfo regiestInfo = Registed[i];
                            if (regiestInfo.berforConvertProperties == verbPropertiesBeforeConvert)
                            {
                                cache = (ParentProccesser, null, null, regiestInfo.afterConvertProperties);
                                break;
                            }
                        }
                    }
                }

                if (!CheckVerbDatasVaildityAndAdapt(ownerType, cache.Item1, ref cache.Item2, ref cache.Item3, ref cache.Item4)) return result;

                result = cache;

                if (result.Item1 != null && result.Item1 != parent && ((CompChildNodeProccesser)result.Item1) != null)
                {
                    Thing before = result.Item1;
                    result = ((CompChildNodeProccesser)result.Item1).GetAfterConvertVerbCorrespondingThing(ownerType, result.Item2, result.Item3, result.Item4);
                    result.Item1 = result.Item1 ?? before;
                }
            }
            return result;
        }

        /// <summary>
        /// set all texture need regenerate
        /// </summary>
        public void ResetRenderedTexture()
        {
            renderingCache.ResetRenderedTexture();
            if(parent.Spawned && parent.def.drawerType >= DrawerType.MapMeshOnly) parent.DirtyMapMesh(parent.Map);
            ParentProccesser?.ResetRenderedTexture();
        }

        /// <summary>
        /// set all Verbs need regenerate
        /// </summary>
        public void ResetVerbs()
        {
            foreach (ThingComp comp in parent.AllComps)
            {
                (comp as IVerbOwner)?.VerbTracker?.VerbsNeedReinitOnLoad();
            }
            (parent as IVerbOwner)?.VerbTracker?.VerbsNeedReinitOnLoad();
            regiestedNodeVerbPropertiesInfos.Clear();
            regiestedNodeVerbToolInfos.Clear();
            BeforeConvertVerbCorrespondingThingCache.Clear();
            ParentProccesser?.ResetVerbs();
        }


        public void ResetRegiestedNodeId()
        {
            regiestedNodeId.Clear();
            ParentProccesser?.ResetRegiestedNodeId();
        }


        public List<VerbToolRegiestInfo> GetRegiestedNodeVerbToolInfos(Type ownerType)
        {
            if (ownerType != null && typeof(IVerbOwner).IsAssignableFrom(ownerType))
            {
                return regiestedNodeVerbToolInfos.TryGetValue(ownerType) ?? new List<VerbToolRegiestInfo>();
            }
            return new List<VerbToolRegiestInfo>();
        }


        public List<VerbPropertiesRegiestInfo> GetRegiestedNodeVerbPropertiesInfos(Type ownerType)
        {
            if(ownerType != null && typeof(IVerbOwner).IsAssignableFrom(ownerType))
            {
                return regiestedNodeVerbPropertiesInfos.TryGetValue(ownerType) ?? new List<VerbPropertiesRegiestInfo>();
            }
            return new List<VerbPropertiesRegiestInfo>();
        }

        /// <summary>
        /// Rimworld Defined method, used for load and save game saves.
        /// </summary>
        public override void PostExposeData()
        {
            Scribe_Deep.Look(ref this.childNodes, "innerContainer", this);
            //Scribe_Collections.Look(ref childNodes, "innerContainer", LookMode.Deep, this);
        }

        /// <summary>
        /// Render all child things
        /// </summary>
        /// <param name="rot">rotate</param>
        /// <param name="subGraphic">orging Graphic of this</param>
        /// <returns>result of rendering</returns>
        public Material GetAndUpdateChildTexture(Rot4 rot, Graphic subGraphic = null)
        {
            (Material material, Texture2D texture, RenderTexture cachedRenderTarget, bool IsRandered) = renderingCache[rot];
            if (IsRandered && material != null) return material;
            List<(Thing, string, List<RenderInfo>)> nodeRenderingInfos = new List<(Thing, string, List<RenderInfo>)>(ChildNodes.Count + 1);

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
            subGraphic = ((subGraphic ?? parent.Graphic)?.GetGraphic_ChildNode() as Graphic_ChildNode)?.SubGraphic ?? subGraphic;
            if (subGraphic != null)
            {
                RenderingTools.StartOrEndDrawCatchingBlock = true;
                try
                {
                    subGraphic.Draw(Vector3.zero, rot, parent);
                    nodeRenderingInfos.Add((this, null, RenderingTools.RenderInfos));
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
                RenderingTools.StartOrEndDrawCatchingBlock = true;
                try
                {
                    if (child != null)
                    {
                        Rot4 rotCache = child.Rotation;
                        child.Rotation = new Rot4((rot.AsInt + rotCache.AsInt) & 3);
                        child.DrawAt(Vector3.zero);
                        child.Rotation = rotCache;
                        nodeRenderingInfos.Add((child, container[(uint)i], RenderingTools.RenderInfos));
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
                RenderingTools.StartOrEndDrawCatchingBlock = false;
            }

            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                nodeRenderingInfos = comp.internal_OverrideDrawSteep(nodeRenderingInfos, rot, subGraphic) ?? nodeRenderingInfos;
            }

            List<RenderInfo> final = new List<RenderInfo>();
            foreach((Thing, string, List<RenderInfo>) infos in nodeRenderingInfos)
            {
                final.AddRange(infos.Item3);
            }

            RenderingTools.RenderToTarget(final, ref cachedRenderTarget, ref texture, default(Vector2Int), Props.TextureSizeFactor, Props.ExceedanceFactor, Props.ExceedanceOffset);


            Shader shader = subGraphic.Shader;

            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = Props.TextureFilterMode;

            if (material == null)
            {
                material = new Material(shader);
            }
            else if(shader != null)
            {
                material.shader = shader;
            }
            material.mainTexture = texture;
            IsRandered = true;
            renderingCache[rot] = (material, texture, cachedRenderTarget, IsRandered);
            return material;
        }


        public Vector2 GetAndUpdateDrawSize(Rot4 rot, Graphic subGraphic = null)
        {
            (Material material, Texture2D texture, RenderTexture cachedRenderTarget, bool IsRandered) = renderingCache[rot];
            if (!IsRandered || texture == null) GetAndUpdateChildTexture(rot, subGraphic);
            (material, texture, cachedRenderTarget, IsRandered) = renderingCache[rot];
            Vector2 result = new Vector2(texture.width, texture.height) / Props.TextureSizeFactor;
            //if (Prefs.DevMode) Log.Message(" DrawSize: thing=" + parent + "; Rot4=" + rot + "; textureWidth=" + textures[rot_int].width + "; result=" + result + ";\n");
            renderingCache[rot] = (material, texture, cachedRenderTarget, IsRandered);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool AllowNode(Thing node, string id = null)
        {
            if (id.NullOrEmpty() || ChildNodes.IsChildOf(node)) return false;
            if (Props.ForceNodeIdControl && !RegiestedNodeId.Contains(id)) return false;
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                if (!comp.internal_AllowNode(node, id)) return false;
            }
            return true;
        }

        /// <summary>
        /// Update node tree
        /// </summary>
        /// <returns></returns>
        public bool UpdateNode() => ChildNodes.internal_UpdateNode();


        public void ResetCachedRootNode()
        {
            cachedRootNode = null;
            foreach (Thing part in ChildNodes.Values)
            {
                CompChildNodeProccesser childComp = part;
                if (childComp != null) childComp.ResetCachedRootNode();
            }
        }

        internal void internal_PerAdd(ref Thing node, ref string id)
        {
            Thing nodeCache = node;
            string idCache = id;
            foreach(CompBasicNodeComp comp in AllNodeComp)
            {
                comp.internal_PerAdd(ref nodeCache, ref idCache);
                nodeCache = nodeCache ?? node;
                idCache = idCache ?? id;
            }
            node = nodeCache;
            id = idCache;
        }

        internal void internal_PostAdd(Thing node, string id, bool success)
        {
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                comp.internal_PostAdd(node, id, success);
            }
        }

        internal void internal_Added(NodeContainer container, string id)
        {
            ResetCachedRootNode();
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                comp.internal_Added(container, id);
            }
        }

        internal void internal_PerRemove(ref Thing node)
        {
            Thing nodeCache = node;
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                comp.internal_PerRemove(ref nodeCache);
                nodeCache = nodeCache ?? node;
            }
            node = nodeCache;
        }

        internal void internal_PostRemove(Thing node, string id, bool success)
        {
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                comp.internal_PostRemove(node, id, success);
            }
        }

        internal void internal_Removed(NodeContainer container, string id)
        {
            ResetCachedRootNode();
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                comp.internal_Removed(container, id);
            }
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.ChildNodes);
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            if (childNodes == null)
            {
                childNodes = new NodeContainer(this);
            }
            return childNodes;
        }

        public static IVerbOwner GetSameTypeVerbOwner(Type ownerType, Thing thing)
        {
            if(thing != null && ownerType != null && typeof(IVerbOwner).IsAssignableFrom(ownerType))
            {
                IVerbOwner verbOwner = null;
                ThingWithComps t = thing as ThingWithComps;
                if (ownerType.IsAssignableFrom(thing.GetType()))
                {
                    verbOwner = (thing as IVerbOwner);
                }
                else if (t != null)
                {
                    foreach (ThingComp comp in t.AllComps)
                    {
                        if (ownerType.IsAssignableFrom(comp.GetType()))
                        {
                            verbOwner = (comp as IVerbOwner);
                            break;
                        }
                    }
                }
                return verbOwner;
            }
            return null;
        }


        private class OffScreenRenderingCache
        {
            ~OffScreenRenderingCache()
            {
                GameObject.Destroy(materialNorth);
                GameObject.Destroy(materialEast);
                GameObject.Destroy(materialSouth);
                GameObject.Destroy(materialWest);
                GameObject.Destroy(textureNorth);
                GameObject.Destroy(textureEast);
                GameObject.Destroy(textureSouth);
                GameObject.Destroy(textureWest);
                GameObject.Destroy(cachedRenderTargetNorth);
                GameObject.Destroy(cachedRenderTargetEast);
                GameObject.Destroy(cachedRenderTargetSouth);
                GameObject.Destroy(cachedRenderTargetWest);
            }
            public (Material, Texture2D, RenderTexture, bool) this[Rot4 index]
            {
                get
                {
                    switch(index.AsByte)
                    {
                        case 0: return (materialNorth, textureNorth, cachedRenderTargetNorth, (IsRandereds & 1) == 1);
                        case 1: return (materialEast, textureEast, cachedRenderTargetEast, ((IsRandereds >> 1) & 1) == 1);
                        case 2: return (materialSouth, textureSouth, cachedRenderTargetSouth, ((IsRandereds >> 2) & 1) == 1);
                        case 3: return (materialWest, textureWest, cachedRenderTargetWest, ((IsRandereds >> 3) & 1) == 1);
                        default: return (materialNorth, textureNorth, cachedRenderTargetNorth, (IsRandereds & 1) == 1);
                    }
                } 
                set
                {
                    switch (index.AsByte)
                    {
                        case 0:
                            materialNorth = value.Item1;
                            textureNorth = value.Item2;
                            cachedRenderTargetNorth = value.Item3;
                            break;
                        case 1:
                            materialEast = value.Item1;
                            textureEast = value.Item2;
                            cachedRenderTargetEast = value.Item3;
                            break;
                        case 2:
                            materialSouth = value.Item1;
                            textureSouth = value.Item2;
                            cachedRenderTargetSouth = value.Item3;
                            break;
                        case 3:
                            materialWest = value.Item1;
                            textureWest = value.Item2;
                            cachedRenderTargetWest = value.Item3;
                            break;
                        default:
                            materialNorth = value.Item1;
                            textureNorth = value.Item2;
                            cachedRenderTargetNorth = value.Item3;
                            break;
                    }
                    if (value.Item4) IsRandereds |= (byte)(1 << index.AsByte);
                    else IsRandereds &= (byte)~(1 << index.AsByte);

                }
            }

            public void ResetRenderedTexture() => IsRandereds = 0;

            public Material materialNorth, materialEast, materialSouth, materialWest;

            public Texture2D textureNorth, textureEast, textureSouth, textureWest;

            public RenderTexture cachedRenderTargetNorth, cachedRenderTargetEast, cachedRenderTargetSouth, cachedRenderTargetWest;

            public byte IsRandereds;
        }

        #region operator
        public static implicit operator Thing(CompChildNodeProccesser node)
        {
            return node?.parent;
        }

        public static implicit operator CompChildNodeProccesser(Thing thing)
        {
            return thing?.TryGetComp<CompChildNodeProccesser>();
        }
        #endregion

        private CompChildNodeProccesser cachedRootNode;

        private NodeContainer childNodes;

        private readonly OffScreenRenderingCache renderingCache = new OffScreenRenderingCache();

        private readonly HashSet<string> regiestedNodeId = new HashSet<string>();

        private readonly Dictionary<Type, List<VerbToolRegiestInfo>> regiestedNodeVerbToolInfos = new Dictionary<Type, List<VerbToolRegiestInfo>>();

        private readonly Dictionary<Type, List<VerbPropertiesRegiestInfo>> regiestedNodeVerbPropertiesInfos = new Dictionary<Type, List<VerbPropertiesRegiestInfo>>();

        private readonly Dictionary<Type, Dictionary<(Thing, Verb, Tool, VerbProperties, bool), (Thing, Verb, Tool, VerbProperties)>> BeforeConvertVerbCorrespondingThingCache = new Dictionary<Type, Dictionary<(Thing, Verb, Tool, VerbProperties, bool), (Thing, Verb, Tool, VerbProperties)>>();


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

    public class CompProperties_ChildNodeProccesser : CompProperties
    {
        public CompProperties_ChildNodeProccesser()
        {
            base.compClass = typeof(CompChildNodeProccesser);
        }

        public override void ResolveReferences(ThingDef parentDef)
        {
            parentDef.comps.Remove(this);
            parentDef.comps.Insert(0, this);
        }

        public bool VerbDirectOwnerRedictory = false;
        public bool VerbEquipmentSourceRedictory = true;
        public bool VerbIconVerbInstanceSource = false;
        public bool ForceNodeIdControl = false;
        public bool NodeIdAutoInsertByRegiested = true;
        public float ExceedanceFactor = 1f;
        public float ExceedanceOffset = 1f;
        public int TextureSizeFactor = (int)RenderingTools.DefaultTextureSizeFactor;
        public FilterMode TextureFilterMode = FilterMode.Bilinear;
    }
}
