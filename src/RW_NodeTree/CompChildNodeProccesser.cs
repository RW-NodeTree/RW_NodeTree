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

        public (Thing, List<Verb>, Tool) GetBeforeConvertThingWithTool(Type ownerType, Tool toolAfterConvert, VerbProperties? verbPropertiesAfterConvert = null, bool needVerb = false)
        {
            if (ownerType == null || !typeof(IVerbOwner).IsAssignableFrom(ownerType)) throw new AggregateException(nameof(ownerType));
            if (toolAfterConvert == null) throw new AggregateException(nameof(toolAfterConvert));
            UpdateNode();
            lock (beforeConvertVerbCorrespondingThingCache)
            {
                (Type, Tool?, VerbProperties?, bool) key = (ownerType, toolAfterConvert, verbPropertiesAfterConvert, needVerb);

                if (beforeConvertVerbCorrespondingThingCache.TryGetValue(key, out (Thing, List<Verb>, Tool?, VerbProperties?) value))
                {
                    if (value.Item1 != null && value.Item2 != null && value.Item3 != null)
                        return (value.Item1, value.Item2, value.Item3);
                    else
                        throw new Exception($"{this} can't find VerbToolRegiestInfo for {toolAfterConvert}");
                }
                IVerbOwner? verbOwner = GetSameTypeVerbOwner(ownerType, parent);
                List<VerbProperties> props = toolAfterConvert.VerbsProperties.ToList();
                if (verbPropertiesAfterConvert != null)
                {
                    if (props.Contains(verbPropertiesAfterConvert))
                        props = [verbPropertiesAfterConvert];
                    else
                        throw new AggregateException(nameof(verbPropertiesAfterConvert));
                }
                List<Verb?> list = verbOwner?.VerbTracker?.GetOriginalAllVerbs() ?? new List<Verb?>();
                List<Verb> verbs = new List<Verb>(list.Count);
                foreach (Verb? verb in list)
                {
                    if (verb?.tool == toolAfterConvert && props.Contains(verb.verbProps))
                    {
                        verbs.Add(verb);
                    }
                }
                value = (
                    parent,
                    verbs,
                    toolAfterConvert,
                    verbPropertiesAfterConvert
                );
                foreach (VerbToolRegiestInfo regiestInfo in internal_GetRegiestedNodeVerbToolInfos(ownerType))
                {
                    if (regiestInfo.afterConvertTool == toolAfterConvert)
                    {
                        if (regiestInfo.id.NullOrEmpty())
                        {
                            if (!needVerb)
                            {
                                if (verbPropertiesAfterConvert == null || regiestInfo.berforConvertTool.VerbsProperties.Contains(verbPropertiesAfterConvert))
                                {
                                    value = (
                                        parent,
                                        new List<Verb>(),
                                        regiestInfo.berforConvertTool,
                                        verbPropertiesAfterConvert
                                    );
                                }
                            }
                            else if (value.Item2.Count <= 0) throw new AggregateException(nameof(ownerType));
                        }
                        else
                        {
                            Thing? child = ChildNodes[regiestInfo.id!];
                            if (child == null) throw new Exception($"{this} RegiestedNodeVerbToolInfos has error");

                            verbOwner = GetSameTypeVerbOwner(ownerType, child);
                            props = regiestInfo.berforConvertTool.VerbsProperties.ToList();
                            if (verbPropertiesAfterConvert != null)
                            {
                                if (props.Contains(verbPropertiesAfterConvert))
                                    props = [verbPropertiesAfterConvert];
                                else
                                    props = [];
                            }
                            list = verbOwner?.VerbTracker?.GetOriginalAllVerbs() ?? new List<Verb?>();
                            verbs = new List<Verb>(list.Count);
                            foreach (Verb? verb in list)
                            {
                                if (verb?.tool == regiestInfo.berforConvertTool && props.Contains(verb.verbProps))
                                {
                                    verbs.Add(verb);
                                }
                            }
                            if (needVerb)
                            {
                                if (verbs.Count > 0)
                                {
                                    CompChildNodeProccesser? proccesser = child;
                                    if (proccesser != null)
                                    {
                                        (Thing, List<Verb>, Tool) next = proccesser.GetBeforeConvertThingWithTool(ownerType, regiestInfo.berforConvertTool, verbPropertiesAfterConvert, needVerb);

                                        value = (
                                            next.Item1,
                                            next.Item2,
                                            next.Item3,
                                            verbPropertiesAfterConvert
                                        );
                                    }
                                }
                                else if (value.Item2.Count <= 0) throw new AggregateException(nameof(ownerType));
                            }
                            else if (props.Count > 0)
                            {
                                CompChildNodeProccesser? proccesser = child;
                                if (proccesser != null)
                                {
                                    (Thing, List<Verb>, Tool) next = proccesser.GetBeforeConvertThingWithTool(ownerType, regiestInfo.berforConvertTool, verbPropertiesAfterConvert, needVerb);

                                    value = (
                                        next.Item1,
                                        next.Item2,
                                        next.Item3,
                                        verbPropertiesAfterConvert
                                    );
                                }
                                else
                                {
                                    value = (
                                        child,
                                        verbs,
                                        regiestInfo.berforConvertTool,
                                        verbPropertiesAfterConvert
                                    );
                                }
                            }
                        }
                        break;
                    }
                }
                beforeConvertVerbCorrespondingThingCache[key] = value;
                if (value.Item1 != null && value.Item2 != null && value.Item3 != null)
                    return (value.Item1, value.Item2, value.Item3);
                else
                    throw new Exception($"{this} can't find VerbToolRegiestInfo for {toolAfterConvert}");
            }
        }


        public (Thing, Verb?, VerbProperties) GetBeforeConvertThingWithVerbProperties(Type ownerType, VerbProperties verbPropertiesAfterConvert, bool needVerb = false)
        {
            if (ownerType == null || !typeof(IVerbOwner).IsAssignableFrom(ownerType)) throw new AggregateException(nameof(ownerType));
            if (verbPropertiesAfterConvert == null) throw new AggregateException(nameof(verbPropertiesAfterConvert));
            UpdateNode();
            lock (beforeConvertVerbCorrespondingThingCache)
            {
                (Type, Tool?, VerbProperties?, bool) key = (ownerType, null, verbPropertiesAfterConvert, needVerb);

                if (beforeConvertVerbCorrespondingThingCache.TryGetValue(key, out (Thing, List<Verb>, Tool?, VerbProperties?) value))
                {
                    if (value.Item1 != null && value.Item2 != null && value.Item4 != null)
                        return (value.Item1, value.Item2.FirstOrDefault(), value.Item4);
                    else
                        throw new Exception($"{this} can't find VerbPropertiesRegiestInfo for {verbPropertiesAfterConvert}");
                }
                IVerbOwner? verbOwner = GetSameTypeVerbOwner(ownerType, parent);
                List<Verb?> list = verbOwner?.VerbTracker?.GetOriginalAllVerbs() ?? new List<Verb?>();
                List<Verb> verbs = new List<Verb>(list.Count);
                foreach (Verb? verb in list)
                {
                    if (verb?.verbProps == verbPropertiesAfterConvert)
                    {
                        verbs.Add(verb);
                    }
                }
                value = (
                    parent,
                    verbs,
                    null,
                    verbPropertiesAfterConvert
                );
                foreach (VerbPropertiesRegiestInfo regiestInfo in internal_GetRegiestedNodeVerbPropertiesInfos(ownerType))
                {
                    if (regiestInfo.afterConvertProperties == verbPropertiesAfterConvert)
                    {
                        if (regiestInfo.id.NullOrEmpty())
                        {
                            if (!needVerb)
                            {
                                value = (
                                    parent,
                                    new List<Verb>(),
                                    null,
                                    regiestInfo.berforConvertProperties
                                );
                            }
                            else if (value.Item2.Count <= 0) throw new AggregateException(nameof(ownerType));
                        }
                        else
                        {
                            Thing? child = ChildNodes[regiestInfo.id!];
                            if (child == null) throw new Exception($"{this} RegiestedVerbPropertiesInfos has error");
                            verbOwner = GetSameTypeVerbOwner(ownerType, parent);
                            list = verbOwner?.VerbTracker?.GetOriginalAllVerbs() ?? new List<Verb?>();
                            verbs = new List<Verb>(list.Count);
                            foreach (Verb? verb in list)
                            {
                                if (verb?.verbProps == regiestInfo.berforConvertProperties)
                                {
                                    verbs.Add(verb);
                                }
                            }
                            if (needVerb)
                            {
                                if (verbs.Count > 0)
                                {
                                    CompChildNodeProccesser? proccesser = child;
                                    if (proccesser != null)
                                    {
                                        (Thing, Verb?, VerbProperties) next = proccesser.GetBeforeConvertThingWithVerbProperties(ownerType, regiestInfo.berforConvertProperties, needVerb);

                                        value = (
                                            next.Item1,
                                            [next.Item2!],
                                            null,
                                            next.Item3
                                        );
                                    }
                                }
                                else if (value.Item2.Count <= 0) throw new AggregateException(nameof(ownerType));
                            }
                            else
                            {
                                CompChildNodeProccesser? proccesser = child;
                                if (proccesser != null)
                                {
                                    (Thing, Verb?, VerbProperties) next = proccesser.GetBeforeConvertThingWithVerbProperties(ownerType, regiestInfo.berforConvertProperties, needVerb);

                                    value = (
                                        next.Item1,
                                        next.Item2 == null ? [] : [next.Item2],
                                        null,
                                        next.Item3
                                    );
                                }
                                else
                                {
                                    value = (
                                        child,
                                        verbs,
                                        null,
                                        regiestInfo.berforConvertProperties
                                    );
                                }
                            }
                        }
                        break;
                    }
                }
                beforeConvertVerbCorrespondingThingCache[key] = value;
                if (value.Item1 != null && value.Item2 != null && value.Item4 != null)
                    return (value.Item1, value.Item2.FirstOrDefault(), value.Item4);
                else
                    throw new Exception($"{this} can't find VerbPropertiesRegiestInfo for {verbPropertiesAfterConvert}");
            }
        }

        public (Thing, Verb?, Tool?, VerbProperties) GetBeforeConvertThingWithVerb(Type ownerType, Verb verbAfterConvert, bool needVerb = false)
        {
            if (verbAfterConvert == null) throw new AggregateException(nameof(verbAfterConvert));
            if (verbAfterConvert.tool != null)
            {
                (Thing, List<Verb>, Tool) tool = GetBeforeConvertThingWithTool(ownerType, verbAfterConvert.tool, verbAfterConvert.verbProps, needVerb);
                return (tool.Item1, tool.Item2.FirstOrDefault(), tool.Item3, verbAfterConvert.verbProps);
            }
            else
            {
                (Thing, Verb?, VerbProperties) verbProperties = GetBeforeConvertThingWithVerbProperties(ownerType, verbAfterConvert.verbProps, needVerb);
                return (verbProperties.Item1, verbProperties.Item2, null, verbProperties.Item3);
            }
        }



        public (Thing, List<Verb>, Tool) GetAfterConvertThingWithTool(Type ownerType, Tool toolBeforeConvert, VerbProperties? verbPropertiesBeforeConvert, bool needVerb = false)
        {
            if (ownerType == null || !typeof(IVerbOwner).IsAssignableFrom(ownerType)) throw new AggregateException(nameof(ownerType));
            if (toolBeforeConvert == null) throw new AggregateException(nameof(toolBeforeConvert));
            UpdateNode();
            IVerbOwner? verbOwner = GetSameTypeVerbOwner(ownerType, parent);
            List<VerbProperties> props = toolBeforeConvert.VerbsProperties.ToList();
            if (verbPropertiesBeforeConvert != null)
            {
                if (props.Contains(verbPropertiesBeforeConvert))
                    props = [verbPropertiesBeforeConvert];
                else
                    throw new AggregateException(nameof(verbPropertiesBeforeConvert));
            }
            List<Verb?> list = verbOwner?.VerbTracker?.GetOriginalAllVerbs() ?? new List<Verb?>();
            List<Verb> verbs = new List<Verb>(list.Count);
            foreach (Verb? verb in list)
            {
                if (verb?.tool == toolBeforeConvert && props.Contains(verb.verbProps))
                {
                    verbs.Add(verb);
                }
            }
            (Thing, List<Verb>, Tool?, VerbProperties?)
            value = (
                parent,
                verbs,
                toolBeforeConvert,
                verbPropertiesBeforeConvert
            );
            CompChildNodeProccesser? parentProccesser = ParentProccesser;
            if (parentProccesser != null)
            {
                foreach (VerbToolRegiestInfo regiestInfo in parentProccesser.internal_GetRegiestedNodeVerbToolInfos(ownerType))
                {
                    if (regiestInfo.berforConvertTool == toolBeforeConvert && regiestInfo.id != null && parentProccesser.ChildNodes[regiestInfo.id] == parent)
                    {
                        verbOwner = GetSameTypeVerbOwner(ownerType, parentProccesser.parent);
                        props = regiestInfo.afterConvertTool.VerbsProperties.ToList();
                        if (verbPropertiesBeforeConvert != null)
                        {
                            if (props.Contains(verbPropertiesBeforeConvert))
                                props = [verbPropertiesBeforeConvert];
                            else
                                props = [];
                        }
                        list = verbOwner?.VerbTracker?.GetOriginalAllVerbs() ?? new List<Verb?>();
                        verbs = new List<Verb>(list.Count);
                        foreach (Verb? verb in list)
                        {
                            if (verb?.tool == regiestInfo.afterConvertTool && props.Contains(verb.verbProps))
                            {
                                verbs.Add(verb);
                            }
                        }
                        if (needVerb)
                        {
                            if (verbs.Count > 0)
                            {
                                (Thing, List<Verb>, Tool) next = parentProccesser.GetAfterConvertThingWithTool(ownerType, regiestInfo.afterConvertTool, verbPropertiesBeforeConvert, needVerb);

                                value = (
                                    next.Item1,
                                    next.Item2,
                                    next.Item3,
                                    verbPropertiesBeforeConvert
                                );
                            }
                            else if (value.Item2.Count <= 0) throw new AggregateException(nameof(ownerType));
                        }
                        else if (props.Count > 0)
                        {
                            (Thing, List<Verb>, Tool) next = parentProccesser.GetAfterConvertThingWithTool(ownerType, regiestInfo.afterConvertTool, verbPropertiesBeforeConvert, needVerb);

                            value = (
                                next.Item1,
                                next.Item2,
                                next.Item3,
                                verbPropertiesBeforeConvert
                            );
                        }
                        break;
                    }
                }
            }

            if (value.Item1 != null && value.Item2 != null && value.Item3 != null)
                return (value.Item1, value.Item2, value.Item3);
            else
                throw new Exception($"{this} can't find VerbToolRegiestInfo for {toolBeforeConvert}");
        }


        public (Thing, Verb?, VerbProperties) GetAfterConvertThingWithVerbProperties(Type ownerType, VerbProperties verbPropertiesBeforeConvert, bool needVerb = false)
        {
            if (ownerType == null || !typeof(IVerbOwner).IsAssignableFrom(ownerType)) throw new AggregateException(nameof(ownerType));
            if (verbPropertiesBeforeConvert == null) throw new AggregateException(nameof(verbPropertiesBeforeConvert));
            UpdateNode();
            IVerbOwner? verbOwner = GetSameTypeVerbOwner(ownerType, parent);
            List<Verb?> list = verbOwner?.VerbTracker?.GetOriginalAllVerbs() ?? new List<Verb?>();
            List<Verb> verbs = new List<Verb>(list.Count);
            foreach (Verb? verb in list)
            {
                if (verb?.verbProps == verbPropertiesBeforeConvert)
                {
                    verbs.Add(verb);
                }
            }
            (Thing, List<Verb>, Tool?, VerbProperties?)
            value = (
                parent,
                verbs,
                null,
                verbPropertiesBeforeConvert
            );
            CompChildNodeProccesser? parentProccesser = ParentProccesser;
            if (parentProccesser != null)
            {
                foreach (VerbPropertiesRegiestInfo regiestInfo in parentProccesser.internal_GetRegiestedNodeVerbPropertiesInfos(ownerType))
                {
                    if (regiestInfo.berforConvertProperties == verbPropertiesBeforeConvert && regiestInfo.id != null && parentProccesser.ChildNodes[regiestInfo.id] == parent)
                    {
                        verbOwner = GetSameTypeVerbOwner(ownerType, parentProccesser.parent);
                        list = verbOwner?.VerbTracker?.GetOriginalAllVerbs() ?? new List<Verb?>();
                        verbs = new List<Verb>(list.Count);
                        foreach (Verb? verb in list)
                        {
                            if (verb?.verbProps == regiestInfo.afterConvertProperties)
                            {
                                verbs.Add(verb);
                            }
                        }
                        if (needVerb)
                        {
                            if (verbs.Count > 0)
                            {
                                (Thing, Verb?, VerbProperties) next = parentProccesser.GetAfterConvertThingWithVerbProperties(ownerType, regiestInfo.afterConvertProperties, needVerb);

                                value = (
                                    next.Item1,
                                    next.Item2 == null ? [] : [next.Item2],
                                    null,
                                    next.Item3
                                );
                            }
                            else if (value.Item2.Count <= 0) throw new AggregateException(nameof(ownerType));
                        }
                        else
                        {
                            (Thing, Verb?, VerbProperties) next = parentProccesser.GetAfterConvertThingWithVerbProperties(ownerType, regiestInfo.afterConvertProperties, needVerb);

                            value = (
                                next.Item1,
                                next.Item2 == null ? [] : [next.Item2],
                                null,
                                next.Item3
                            );
                        }
                        break;
                    }
                }
            }

            if (value.Item1 != null && value.Item2 != null && value.Item4 != null)
                return (value.Item1, value.Item2.FirstOrDefault(), value.Item4);
            else
                throw new Exception($"{this} can't find VerbPropertiesRegiestInfo for {verbPropertiesBeforeConvert}");
        }


        public (Thing, Verb?, Tool?, VerbProperties) GetAfterConvertThingWithVerb(Type ownerType, Verb verbAfterConvert, bool needVerb = false)
        {
            if (verbAfterConvert == null) throw new AggregateException(nameof(verbAfterConvert));
            if (verbAfterConvert.tool != null)
            {
                (Thing, List<Verb>, Tool) tool = GetAfterConvertThingWithTool(ownerType, verbAfterConvert.tool, verbAfterConvert.verbProps, needVerb);
                return (tool.Item1, tool.Item2.FirstOrDefault(), tool.Item3, verbAfterConvert.verbProps);
            }
            else
            {
                (Thing, Verb?, VerbProperties) verbProperties = GetAfterConvertThingWithVerbProperties(ownerType, verbAfterConvert.verbProps, needVerb);
                return (verbProperties.Item1, verbProperties.Item2, null, verbProperties.Item3);
            }
        }

        /// <summary>
        /// set all texture need regenerate
        /// </summary>
        public void ResetRenderedTexture()
        {
            lock (ChildNodes)
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
        /// set all Verbs need regenerate
        /// </summary>
        public void ResetVerbs()
        {
            lock (ChildNodes)
                lock (regiestedNodeVerbPropertiesInfos)
                    lock (regiestedNodeVerbToolInfos)
                        lock (beforeConvertVerbCorrespondingThingCache)
                        {
                            foreach (ThingComp comp in parent.AllComps)
                            {
                                (comp as IVerbOwner)?.VerbTracker?.VerbsNeedReinitOnLoad();
                            }
                            (parent as IVerbOwner)?.VerbTracker?.VerbsNeedReinitOnLoad();
                            regiestedNodeVerbPropertiesInfos.Clear();
                            regiestedNodeVerbToolInfos.Clear();
                            beforeConvertVerbCorrespondingThingCache.Clear();
                        }
            ParentProccesser?.ResetVerbs();
        }

        public List<VerbPropertiesRegiestInfo> GetRegiestedNodeVerbPropertiesInfos(Type ownerType) => internal_GetRegiestedNodeVerbPropertiesInfos(ownerType);

        internal List<VerbPropertiesRegiestInfo> internal_GetRegiestedNodeVerbPropertiesInfos(Type ownerType, List<VerbProperties?>? verbProperties = null)
        {
            lock (regiestedNodeVerbPropertiesInfos)
            {
                verbProperties = verbProperties ?? GetSameTypeVerbOwner(ownerType, parent)?.VerbProperties ?? new List<VerbProperties?>();
                if (!regiestedNodeVerbPropertiesInfos.TryGetValue(ownerType, out List<VerbPropertiesRegiestInfo> info))
                {
                    info = new List<VerbPropertiesRegiestInfo>(verbProperties.Count);
                    foreach (VerbProperties? verbProperty in verbProperties)
                    {
                        if (verbProperty == null) continue;
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
                        List<VerbPropertiesRegiestInfo>? childInfo = regiestInfo.id != null ? ((CompChildNodeProccesser?)ChildNodes[regiestInfo.id])?.internal_GetRegiestedNodeVerbPropertiesInfos(ownerType) : null;
                        if (regiestInfo.Vaildity && (regiestInfo.id.NullOrEmpty() || ChildNodes.ContainsKey(regiestInfo.id!)) && ((childInfo != null && childInfo.Find(x => x.afterConvertProperties == regiestInfo.berforConvertProperties).Vaildity) || childInfo == null))
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
                return info;
            }
        }


        public List<VerbToolRegiestInfo> GetRegiestedNodeVerbToolInfos(Type ownerType) => internal_GetRegiestedNodeVerbToolInfos(ownerType);

        internal List<VerbToolRegiestInfo> internal_GetRegiestedNodeVerbToolInfos(Type ownerType, List<Tool?>? tools = null)
        {
            lock (regiestedNodeVerbToolInfos)
            {
                tools = tools ?? GetSameTypeVerbOwner(ownerType, parent)?.Tools ?? new List<Tool?>();
                if (!regiestedNodeVerbToolInfos.TryGetValue(ownerType, out List<VerbToolRegiestInfo> info))
                {
                    info = new List<VerbToolRegiestInfo>(tools.Count);
                    foreach (Tool? tool in tools)
                    {
                        if (tool == null) continue;
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
                        List<VerbToolRegiestInfo>? childInfo = regiestInfo.id != null ? ((CompChildNodeProccesser?)ChildNodes[regiestInfo.id])?.internal_GetRegiestedNodeVerbToolInfos(ownerType) : null;
                        if (info[i].Vaildity && (regiestInfo.id.NullOrEmpty() || ChildNodes.ContainsKey(regiestInfo.id!)) && ((childInfo != null && childInfo.Find(x => x.afterConvertTool == regiestInfo.berforConvertTool).Vaildity) || childInfo == null))
                        {
                            regiestInfo.afterConvertTool = Gen.MemberwiseClone(regiestInfo.afterConvertTool);
                            regiestInfo.afterConvertTool.id = i.ToString();
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
                    nodeRenderingInfos.Add((container[(uint)i], child, new List<RenderInfo>()));
                }
            }

            Dictionary<string, object> cachingData = new Dictionary<string, object>();

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
            if (!id.IsVaildityKeyFormat() || ChildNodes.IsChildOf(node)) return false;
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

        public static IVerbOwner? GetSameTypeVerbOwner(Type ownerType, Thing thing)
        {
            if (ownerType == null || !typeof(IVerbOwner).IsAssignableFrom(ownerType)) throw new AggregateException(nameof(ownerType));
            if (thing == null) throw new AggregateException(nameof(thing));
            IVerbOwner? verbOwner = null;
            ThingWithComps? t = thing as ThingWithComps;
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
                        verbOwner = comp as IVerbOwner;
                        break;
                    }
                }
            }
            return verbOwner;
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

        internal readonly Dictionary<Type, List<VerbToolRegiestInfo>> regiestedNodeVerbToolInfos = new Dictionary<Type, List<VerbToolRegiestInfo>>();

        internal readonly Dictionary<Type, List<VerbPropertiesRegiestInfo>> regiestedNodeVerbPropertiesInfos = new Dictionary<Type, List<VerbPropertiesRegiestInfo>>();

        internal readonly Dictionary<(Type, Tool?, VerbProperties?, bool), (Thing, List<Verb>, Tool?, VerbProperties?)> beforeConvertVerbCorrespondingThingCache = new Dictionary<(Type, Tool?, VerbProperties?, bool), (Thing, List<Verb>, Tool?, VerbProperties?)>();

        private static bool blockUpdate = false;

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
