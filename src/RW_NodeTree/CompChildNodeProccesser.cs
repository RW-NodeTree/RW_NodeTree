using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RW_NodeTree.Rendering;
using RW_NodeTree.Tools;

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
        public CompChildNodeProccesser ParentProccesser => this.ParentHolder as CompChildNodeProccesser;

        /// <summary>
        /// root of this node tree
        /// </summary>
        public CompChildNodeProccesser RootNode
        {
            get
            {
                lock (this)
                {
                    if (cachedRootNode != null) return cachedRootNode;
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

        public override bool AllowStackWith(Thing other)
        {
            CompChildNodeProccesser comp = other;
            if (comp != null)
            {
                return comp.ChildNodes.Count == 0 && ChildNodes.Count == 0;
            }
            return false;
        }

        public override void CompTick()
        {
            ChildNodes.ThingOwnerTick();
            UpdateNode();
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
            UpdateNode();

            (Thing, Verb, Tool, VerbProperties) result = default((Thing, Verb, Tool, VerbProperties));

            if (!CheckVerbDatasVaildityAndAdapt(ownerType, parent, ref verbAfterConvert, ref toolAfterConvert, ref verbPropertiesAfterConvert)) return result;

            lock (this)
            {
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
                    if (toolAfterConvert != null)
                    {
                        List<VerbToolRegiestInfo> Registed = internal_GetRegiestedNodeVerbToolInfos(ownerType);
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
                        List<VerbPropertiesRegiestInfo> Registed = internal_GetRegiestedNodeVerbPropertiesInfos(ownerType);
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


                    if (cache.Item1 != null && cache.Item1 != parent && ((CompChildNodeProccesser)cache.Item1) != null)
                    {
                        Thing before = cache.Item1;
                        cache = ((CompChildNodeProccesser)cache.Item1).GetBeforeConvertVerbCorrespondingThing(ownerType, cache.Item2, cache.Item3, cache.Item4, needVerb);
                        cache.Item1 = cache.Item1 ?? before;
                    }

                    if (needVerb && cache.Item2 == null) return result;

                    result = cache;
                }
                caches.Add((parent, verbAfterConvert, toolAfterConvert, verbPropertiesAfterConvert, needVerb), result);
                return result;
            }
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
        public (Thing, Verb, Tool, VerbProperties) GetAfterConvertVerbCorrespondingThing(Type ownerType, Verb verbBeforeConvert, Tool toolBeforeConvert, VerbProperties verbPropertiesBeforeConvert, bool needVerb = false)
        {
            UpdateNode();

            (Thing, Verb, Tool, VerbProperties) result = default((Thing, Verb, Tool, VerbProperties));

            if (!CheckVerbDatasVaildityAndAdapt(ownerType, parent, ref verbBeforeConvert, ref toolBeforeConvert, ref verbPropertiesBeforeConvert)) return result;

            result = (parent, verbBeforeConvert, toolBeforeConvert, verbPropertiesBeforeConvert);

            if (ownerType != null && typeof(IVerbOwner).IsAssignableFrom(ownerType) && verbPropertiesBeforeConvert != null)
            {
                (Thing, Verb, Tool, VerbProperties) cache = result;
                if (ParentProccesser != null)
                {
                    if (toolBeforeConvert != null)
                    {
                        List<VerbToolRegiestInfo> Registed = internal_GetRegiestedNodeVerbToolInfos(ownerType);
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
                        List<VerbPropertiesRegiestInfo> Registed = internal_GetRegiestedNodeVerbPropertiesInfos(ownerType);
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

                if (cache.Item1 != null && cache.Item1 != parent && ((CompChildNodeProccesser)cache.Item1) != null)
                {
                    Thing before = cache.Item1;
                    cache = ((CompChildNodeProccesser)cache.Item1).GetAfterConvertVerbCorrespondingThing(ownerType, cache.Item2, cache.Item3, cache.Item4);
                    cache.Item1 = cache.Item1 ?? before;
                }

                if (needVerb && cache.Item2 == null) return result;

                result = cache;
            }
            return result;
        }

        /// <summary>
        /// set all texture need regenerate
        /// </summary>
        public void ResetRenderedTexture()
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
            ParentProccesser?.ResetRenderedTexture();
        }

        /// <summary>
        /// set all Verbs need regenerate
        /// </summary>
        public void ResetVerbs()
        {
            lock (this)
            {
                foreach (ThingComp comp in parent.AllComps)
                {
                    (comp as IVerbOwner)?.VerbTracker?.VerbsNeedReinitOnLoad();
                }
                (parent as IVerbOwner)?.VerbTracker?.VerbsNeedReinitOnLoad();
                regiestedNodeVerbPropertiesInfos.Clear();
                regiestedNodeVerbToolInfos.Clear();
                BeforeConvertVerbCorrespondingThingCache.Clear();
            }
            ParentProccesser?.ResetVerbs();
        }

        public List<VerbPropertiesRegiestInfo> GetRegiestedNodeVerbPropertiesInfos(Type ownerType) => internal_GetRegiestedNodeVerbPropertiesInfos(ownerType);

        internal List<VerbPropertiesRegiestInfo> internal_GetRegiestedNodeVerbPropertiesInfos(Type ownerType, List<VerbProperties> verbProperties = null)
        {
            lock (this)
            {
                verbProperties = verbProperties ?? GetSameTypeVerbOwner(ownerType, parent)?.VerbProperties ?? new List<VerbProperties>();
                if (!regiestedNodeVerbPropertiesInfos.TryGetValue(ownerType, out List<VerbPropertiesRegiestInfo> info))
                {
                    info = new List<VerbPropertiesRegiestInfo>(verbProperties.Count);
                    foreach (VerbProperties verbProperty in verbProperties)
                    {
                        info.Add(new VerbPropertiesRegiestInfo(null, verbProperty, verbProperty));
                    }

                    foreach (CompBasicNodeComp comp in AllNodeComp)
                    {
                        try
                        {
                            info = comp.internal_VerbPropertiesRegiestInfoUpadte(ownerType, info) ?? info;
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex.ToString());
                        }
                    }

                    for (int i = info.Count - 1; i >= 0; i--)
                    {
                        VerbPropertiesRegiestInfo regiestInfo = info[i];
                        List<VerbPropertiesRegiestInfo> childInfo = ((CompChildNodeProccesser)ChildNodes[regiestInfo.id])?.internal_GetRegiestedNodeVerbPropertiesInfos(ownerType);
                        if (regiestInfo.Vaildity && (regiestInfo.id.NullOrEmpty() || ChildNodes.ContainsKey(regiestInfo.id)) && ((childInfo != null && childInfo.Find(x => x.afterConvertProperties == regiestInfo.berforConvertProperties).Vaildity) || childInfo == null))
                        {
                            regiestInfo.afterConvertProperties = Gen.MemberwiseClone(regiestInfo.afterConvertProperties);
                            info[i] = regiestInfo;
                        }
                        else
                        {
                            Log.Warning($"Invaildity regiest info: {info[i]}; removeing...");
                            info.RemoveAt(i);
                        }
                    }
                    regiestedNodeVerbPropertiesInfos.Add(ownerType, info);
                }
                return info ?? new List<VerbPropertiesRegiestInfo>();
            }
        }


        public List<VerbToolRegiestInfo> GetRegiestedNodeVerbToolInfos(Type ownerType) => internal_GetRegiestedNodeVerbToolInfos(ownerType);

        internal List<VerbToolRegiestInfo> internal_GetRegiestedNodeVerbToolInfos(Type ownerType, List<Tool> tools = null)
        {
            lock (this)
            {
                tools = tools ?? GetSameTypeVerbOwner(ownerType, parent)?.Tools ?? new List<Tool>();
                if (!regiestedNodeVerbToolInfos.TryGetValue(ownerType, out List<VerbToolRegiestInfo> info))
                {
                    info = new List<VerbToolRegiestInfo>(tools.Count);
                    foreach (Tool tool in tools)
                    {
                        info.Add(new VerbToolRegiestInfo(null, tool, tool));
                    }

                    foreach (CompBasicNodeComp comp in AllNodeComp)
                    {
                        try
                        {
                            info = comp.internal_VerbToolRegiestInfoUpdate(ownerType, info) ?? info;
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex.ToString());
                        }
                    }


                    for (int i = info.Count - 1; i >= 0; i--)
                    {
                        VerbToolRegiestInfo regiestInfo = info[i];
                        List<VerbToolRegiestInfo> childInfo = ((CompChildNodeProccesser)ChildNodes[regiestInfo.id])?.internal_GetRegiestedNodeVerbToolInfos(ownerType);
                        if (info[i].Vaildity && (regiestInfo.id.NullOrEmpty() || ChildNodes.ContainsKey(regiestInfo.id)) && ((childInfo != null && childInfo.Find(x => x.afterCobvertTool == regiestInfo.berforConvertTool).Vaildity) || childInfo == null))
                        {
                            regiestInfo.afterCobvertTool = Gen.MemberwiseClone(regiestInfo.afterCobvertTool);
                            regiestInfo.afterCobvertTool.id = i.ToString();
                            info[i] = regiestInfo;
                        }
                        else
                        {
                            Log.Warning($"Invaildity regiest info: {info[i]}; removeing...");
                            info.RemoveAt(i);
                        }
                    }
                    regiestedNodeVerbToolInfos.Add(ownerType, info);
                }
                return info ?? new List<VerbToolRegiestInfo>();
            }
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


        public List<(string, Thing, List<RenderInfo>)> GetNodeRenderingInfos(Rot4 rot, out bool updated, Graphic subGraphic = null)
        {

            UpdateNode();
            updated = false;
            if (this.nodeRenderingInfo[rot] != null) return this.nodeRenderingInfo[rot];
            updated = true;
            List<(string, Thing, List<RenderInfo>)> nodeRenderingInfos = new List<(string, Thing, List<RenderInfo>)>(ChildNodes.Count + 1);

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
                    nodeRenderingInfos.Add((null, this, RenderingTools.RenderInfos));
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
                    nodeRenderingInfos.Add((container[(uint)i], child, new List<RenderInfo>()));
                }
            }

            Dictionary<string, object> cachingData = new Dictionary<string, object>();

            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                try
                {
                    nodeRenderingInfos = comp.internal_PreDrawSteep(nodeRenderingInfos, rot, subGraphic, cachingData) ?? nodeRenderingInfos;
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }

            for (int i = 0; i < nodeRenderingInfos.Count; i++)
            {
                string id = nodeRenderingInfos[i].Item1;
                Thing child = nodeRenderingInfos[i].Item2;
                List<RenderInfo> infos = nodeRenderingInfos[i].Item3 ?? new List<RenderInfo>();
                if (child != null && id != null)
                {
                    RenderingTools.StartOrEndDrawCatchingBlock = true;
                    try
                    {
                        Rot4 rotCache = child.Rotation;
                        child.Rotation = new Rot4((rot.AsInt + rotCache.AsInt) & 3);
#if DEBUGV13 || RELEASEV13 || DEBUGV14 || RELEASEV14
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
                }
                nodeRenderingInfos[i] = (id, child, infos);
            }

            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                try
                {
                    nodeRenderingInfos = comp.internal_PostDrawSteep(nodeRenderingInfos, rot, subGraphic, cachingData) ?? nodeRenderingInfos;
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
        public bool AllowNode(Thing node, string id = null)
        {
            if (node?.holdingOwner != null) return false;
            if (node?.Destroyed ?? false) return false;
            if (id.NullOrEmpty() || ChildNodes.IsChildOf(node)) return false;
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
        public bool UpdateNode()
        {
            lock (compLoadingCache)
            {
                if (blockUpdate) return false;
                blockUpdate = true;
            }
            bool result = ChildNodes.internal_UpdateNode();
            lock (compLoadingCache)
            blockUpdate = false;
            return result;
        }

        

        public void ResetCachedRootNode()
        {
            lock (this)
            {
                cachedRootNode = null;
                foreach (Thing part in ChildNodes.Values)
                {
                    CompChildNodeProccesser childComp = part;
                    if (childComp != null) childComp.ResetCachedRootNode();
                }
            }
        }

        internal void internal_Added(NodeContainer container, string id, bool success, Dictionary<string, object> cachedData)
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

        internal void internal_Removed(NodeContainer container, string id, bool success, Dictionary<string, object> cachedData)
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

        public static IVerbOwner GetSameTypeVerbOwner(Type ownerType, Thing thing)
        {
            if (thing != null && ownerType != null && typeof(IVerbOwner).IsAssignableFrom(ownerType))
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



        #region operator
        public static implicit operator Thing(CompChildNodeProccesser node)
        {
            return node?.parent;
        }

        public static implicit operator CompChildNodeProccesser(Thing thing)
        {
            List<ThingComp> comps = (thing as ThingWithComps)?.AllComps;
            if (comps == null || comps.Count < 1) return null;
            CompChildNodeProccesser result = comps[0] as CompChildNodeProccesser;
            if(result != null) return result;
            retry:;
            if (!compLoadingCache.TryGetValue(thing.def, out int index))
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
                compLoadingCache.Add(thing.def,index);
            }
            else if(index >= 0)
            {
                if(index >= comps.Count)
                {
                    compLoadingCache.Remove(thing.def);
                    goto retry;
                }
                result = comps[index] as CompChildNodeProccesser;
                if(result == null)
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
            public List<(string, Thing, List<RenderInfo>)> this[Rot4 rot]
            {
                get => nodeRenderingInfos[rot.AsInt];
                set => nodeRenderingInfos[rot.AsInt] = value;
            }
            public void Reset()
            {
                for (int i = 0; i < nodeRenderingInfos.Length; i++) nodeRenderingInfos[i] = null;
            }
            public readonly List<(string, Thing, List<RenderInfo>)>[] nodeRenderingInfos = new List<(string, Thing, List<RenderInfo>)>[4];
        }

        private CompChildNodeProccesser cachedRootNode;

        private NodeContainer childNodes;

        private readonly NodeRenderingInfoForRot4 nodeRenderingInfo = new NodeRenderingInfoForRot4();

        private readonly Dictionary<Type, List<VerbToolRegiestInfo>> regiestedNodeVerbToolInfos = new Dictionary<Type, List<VerbToolRegiestInfo>>();

        private readonly Dictionary<Type, List<VerbPropertiesRegiestInfo>> regiestedNodeVerbPropertiesInfos = new Dictionary<Type, List<VerbPropertiesRegiestInfo>>();

        private readonly Dictionary<Type, Dictionary<(Thing, Verb, Tool, VerbProperties, bool), (Thing, Verb, Tool, VerbProperties)>> BeforeConvertVerbCorrespondingThingCache = new Dictionary<Type, Dictionary<(Thing, Verb, Tool, VerbProperties, bool), (Thing, Verb, Tool, VerbProperties)>>();

        private static bool blockUpdate = false;

        private readonly static Dictionary<ThingDef,int> compLoadingCache = new Dictionary<ThingDef,int>();

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
            foreach(ThingDef def in DefDatabase<ThingDef>.AllDefs)
            {
                if (def.comps.Count < 1) continue;
                for (int i = 0; i < def.comps.Count; i++)
                {
                    CompProperties properties = def.comps[i];
                    if(properties.compClass == typeof(CompChildNodeProccesser))
                    {
                        def.comps.RemoveAt(i);
                        def.comps.Insert(0,properties);
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

        public bool VerbDirectOwnerRedictory = false;
        public bool VerbEquipmentSourceRedictory = true;
        public bool VerbTrackerAllVerbRedictory = false;
        public bool VerbIconVerbInstanceSource = false;
        public float ExceedanceFactor = 1f;
        public float ExceedanceOffset = 1f;
        public uint TextureSizeFactor = RenderingTools.DefaultTextureSizeFactor;
        public FilterMode TextureFilterMode = FilterMode.Bilinear;
    }
}
