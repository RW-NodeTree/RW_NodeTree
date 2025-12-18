using HarmonyLib;
using RimWorld;
using RW_NodeTree.Rendering;
using RW_NodeTree.Tools;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Threading;
using System.Xml;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Verse;

namespace RW_NodeTree
{
    /// <summary>
    /// Node storge processer
    /// </summary>
    public class NodeContainer : ThingOwner, IReadOnlyList<string>, IReadOnlyList<Thing>, IReadOnlyDictionary<string, Thing?>, IReadOnlyDictionary<Thing, string?>, IReadOnlyList<(string, Thing)>
    {

        public NodeContainer(Thing processer) : base(processer as INodeProcesser)
        {
            if (processer is not INodeProcesser)
            {
                throw new InvalidCastException("Invalid processer type");
            }
        }

        public Thing? this[string key]
        {
            get
            {
                TryGetValue(key, out Thing? result);
                return result;
            }
        }

        public string? this[Thing key]
        {
            get
            {
                TryGetValue(key, out string? result);
                return result;
            }
        }

        public INodeProcesser Processer => (INodeProcesser)Owner;
        public INodeProcesser? ParentProcesser => Processer.ParentHolder as INodeProcesser;
        public NodeContainer? ParentContainer => ParentProcesser?.ChildNodes;

        private (string?, int) CurrentKey
        {
            get => currentKey.GetOrAdd(Thread.CurrentThread.ManagedThreadId, (null, -1));
            set => currentKey[Thread.CurrentThread.ManagedThreadId] = value;
        }

        public bool NeedUpdate
        {
            get
            {
                bool isReadLockHeld = readerWriterLockSlim.IsReadLockHeld || readerWriterLockSlim.IsWriteLockHeld || readerWriterLockSlim.IsUpgradeableReadLockHeld;
                if (!isReadLockHeld) readerWriterLockSlim.EnterReadLock();
                try
                {
                    return needUpdate;
                }
                finally
                {
                    if (!isReadLockHeld) readerWriterLockSlim.ExitReadLock();
                }
            }
            set
            {
                bool isWriteLockHeld = readerWriterLockSlim.IsWriteLockHeld;
                if (!isWriteLockHeld) readerWriterLockSlim.EnterWriteLock();
                try
                {
                    if (needUpdate != value && (needUpdate = value))
                    {
                        NodeContainer? parent = ParentContainer;
                        if (parent != null) parent.NeedUpdate = true;
                    }
                }
                finally
                {
                    if (!isWriteLockHeld) readerWriterLockSlim.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// root of this node tree
        /// </summary>
        public INodeProcesser RootNode
        {
            get
            {
                bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
                try
                {
                    if (cachedRootNode != null) return cachedRootNode;
                    INodeProcesser processer = Processer;
                    INodeProcesser? next = ParentProcesser;
                    while (next != null)
                    {
                        processer = next;
                        next = next.ParentHolder as INodeProcesser;
                    }
                    cachedRootNode = processer;
                    return processer;
                }
                finally
                {
                    if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
                }
            }
        }


        public override int Count => innerList.Count;

        public IEnumerable<string> Keys => this;

        public IEnumerable<Thing?> Values => this;

        IEnumerable<Thing> IReadOnlyDictionary<Thing, string?>.Keys => this;

        IEnumerable<string?> IReadOnlyDictionary<Thing, string?>.Values => this;

        string IReadOnlyList<string>.this[int index] => ((IReadOnlyList<(string, Thing)>)this)[index].Item1;

        Thing IReadOnlyList<Thing>.this[int index] => ((IReadOnlyList<(string, Thing)>)this)[index].Item2;

        (string, Thing) IReadOnlyList<(string, Thing)>.this[int index]
        {
            get
            {
                bool isReadLockHeld = readerWriterLockSlim.IsReadLockHeld || readerWriterLockSlim.IsWriteLockHeld || readerWriterLockSlim.IsUpgradeableReadLockHeld;
                if (!isReadLockHeld) readerWriterLockSlim.EnterReadLock();
                try
                {
                    return innerList[index];
                }
                finally
                {
                    if (!isReadLockHeld) readerWriterLockSlim.ExitReadLock();
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();

            if (Scribe.EnterNode("InnerList"))
            {
                bool isWriteLockHeld = readerWriterLockSlim.IsWriteLockHeld;
                if (!isWriteLockHeld) readerWriterLockSlim.EnterWriteLock();
                try
                {
                    if (Scribe.mode == LoadSaveMode.Saving)
                    {
                        for (int i = 0; i < Count; i++)
                        {
                            var kv = innerList[i];
                            if (kv.Item2 == null) continue;
                            try
                            {
                                Scribe_Deep.Look(ref kv.Item2, kv.Item1);
                            }
                            catch (Exception e)
                            {
                                Log.Error(e.ToString());
                            }
                        }
                    }
                    else if (Scribe.mode == LoadSaveMode.LoadingVars)
                    {
                        for (XmlNode xmlNode = Scribe.loader.curXmlParent.FirstChild; xmlNode != null; xmlNode = xmlNode.NextSibling)
                        {
                            if (xmlNode.NodeType == XmlNodeType.Element)
                            {
                                try
                                {
                                    Thing? thing = null;
                                    Scribe_Deep.Look(ref thing, xmlNode.Name);
                                    if (thing != null)
                                    {
                                        int index = innerList.Count;
                                        indicesById[xmlNode.Name] = index;
                                        indicesByThing[thing] = index;
                                        innerList.Add((xmlNode.Name, thing));
                                        thing.holdingOwner = this;
                                    }
                                }
                                catch (Exception e)
                                {
                                    Log.Error(e.ToString());
                                }
                            }
                        }
                    }
                    else
                    {
                        for(int i = innerList.Count - 1; i >= 0; i--)
                        {
                            var kv = innerList[i];
                            Thing? thing = kv.Item2;
                            Scribe_Deep.Look(ref thing, kv.Item1);
                            if (thing != null)
                            {
                                indicesByThing.Remove(kv.Item2);
                                innerList[i] = (kv.Item1, thing);
                                indicesByThing[thing] = i;
                                thing.holdingOwner = this;
                            }
                            else
                            {
                                innerList.RemoveAt(i);
                                indicesById.Remove(kv.Item1);
                                indicesByThing.Remove(kv.Item2);
                                for (int j = i; j < innerList.Count; j++)
                                {
                                    kv = innerList[j];
                                    indicesById[kv.Item1] = j;
                                    indicesByThing[kv.Item2] = j;
                                }
                            }
                        }
                    }
                }
                finally
                {
                    if (!isWriteLockHeld) readerWriterLockSlim.ExitWriteLock();
                    Scribe.ExitNode();
                }
            }
            //Scribe_Values.Look<bool>(ref this.needUpdate, "needUpdate");
            //if (Scribe.mode == LoadSaveMode.PostLoadInit) needUpdate = false;
        }

        private void CachedRootNodeNeedUpdate()
        {
            cachedRootNode = null;
            for (int i = Count - 1; i >= 0; i--)
            {
                (this[i] as INodeProcesser)?.ChildNodes?.CachedRootNodeNeedUpdate();
            }
        }


        internal List<RenderInfo> GetNodeRenderingInfos(Rot4 rot, Graphic_ChildNode invokeSource)
        {
            UpdateNode();
            if (!UnityData.IsInMainThread) throw new InvalidOperationException("not in main thread");
            Dictionary<string, List<RenderInfo>> nodeRenderingInfos = new Dictionary<string, List<RenderInfo>>(Count + 1);
            Thing parent = (Thing)Processer;

            Dictionary<string, object?> cachingData = new Dictionary<string, object?>();

            Dictionary<string, Rot4>? ChildForDraw = null;
            RenderingTools.StartOrEndDrawCatchingBlock = true;
            try
            {
                ChildForDraw = Processer.PreGenRenderInfos(rot, invokeSource, cachingData);
                nodeRenderingInfos[""] = RenderingTools.RenderInfos!;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
            RenderingTools.StartOrEndDrawCatchingBlock = false;
            if (ChildForDraw != null)
            {

                foreach (var kv in ChildForDraw)
                {
                    if (kv.Key == null) continue;
                    Thing? child = this[kv.Key];
                    if (child != null)
                    {
                        INodeProcesser? childProcesser = child as INodeProcesser;
                        if(childProcesser != null)
                        {
                            nodeRenderingInfos[kv.Key] = childProcesser.ChildNodes.GetNodeRenderingInfos(kv.Value, invokeSource);
                        }
                        else
                        {
                            RenderingTools.StartOrEndDrawCatchingBlock = true;
                            try
                            {
                                Rot4 rotCache = child.Rotation;
                                child.Rotation = kv.Value;
#if V13 || V14
                                child.DrawAt(Vector3.zero);
#else
                                child.DrawNowAt(Vector3.zero);
#endif
                                child.Rotation = rotCache;
                                nodeRenderingInfos[kv.Key] = RenderingTools.RenderInfos!;
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex.ToString());
                            }
                            RenderingTools.StartOrEndDrawCatchingBlock = false;
                            
                        }
                    }
                }
            }
            return Processer.PostGenRenderInfos(rot, invokeSource, nodeRenderingInfos, cachingData);
            //Humm, I think i should not use state mechine to handle this...
            //Time to instead every state check by using various specific method.
            //But that has too many parts to refactor, FXXK!
        }


        /// <summary>
        /// Update node tree
        /// </summary>
        /// <returns></returns>
        public void UpdateNode() => internal_UpdateNode();

        private void internal_UpdateNode(INodeProcesser? actionNode = null)
        {


            INodeProcesser proccess = this.Processer;
            if (proccess == null) return;

            if (actionNode == null)
            {
                RootNode.ChildNodes.internal_UpdateNode(proccess);
                return;
            }
            readerWriterLockSlim.EnterUpgradeableReadLock();
            try
            {
                if (!NeedUpdate) return;

                NeedUpdate = false;

                readerWriterLockSlim.EnterWriteLock();
                try
                {
                    enableWrite = true;
                    for (int i = Count - 1; i >= 0; i--)
                    {
                        if (this[i].Destroyed)
                        {
                            RemoveAt(i);
                        }
                    }
                    enableWrite = false;
                }
                finally
                {
                    readerWriterLockSlim.ExitWriteLock();
                }

                
                Dictionary<string, object?> cachingData = new Dictionary<string, object?>();
                Dictionary<string, Thing> nextChilds;
                try
                {
                    nextChilds = proccess.GenChilds(actionNode, cachingData) ?? new Dictionary<string, Thing>();
                }
                catch (Exception ex)
                {
                    nextChilds = new Dictionary<string, Thing>();
                    Log.Error(ex.ToString());
                }
                
                Dictionary<string, Thing> prveChilds = new Dictionary<string, Thing>(innerList.Count);
                foreach (var child in innerList)
                {
                    prveChilds[child.Item1] = child.Item2;
                }
                ReadOnlyDictionary<string, Thing> prveChildsReadOnly = new ReadOnlyDictionary<string, Thing>(prveChilds);

                
                readerWriterLockSlim.EnterWriteLock();
                try
                {
                    enableWrite = true;
                    foreach (KeyValuePair<string, Thing> pair in prveChilds)
                    {
                        if (!nextChilds.ContainsKey(pair.Key))
                        {
                            Remove(pair.Value);
                        }
                    }
                    foreach (KeyValuePair<string, Thing> pair in nextChilds)
                    {
                        if(!prveChilds.TryGetValue(pair.Key, out Thing? prveNode) || prveNode != pair.Value)
                        {
                            CurrentKey = (pair.Key, -1);
                            TryAdd(pair.Value);
                        }
                    }
                    enableWrite = false;
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
                finally
                {
                    readerWriterLockSlim.ExitWriteLock();
                }


                try
                {
                    proccess.PreUpdateChilds(actionNode, cachingData, prveChildsReadOnly);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }


                foreach (Thing? node in prveChilds.Values)
                {
                    NodeContainer? container = (node as INodeProcesser)?.ChildNodes;
                    if (container != null && container.NeedUpdate && !this.Contains(node))
                    {
                        try
                        {
                            container.internal_UpdateNode(actionNode);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex.ToString());
                        }
                    }
                }

                foreach (Thing? node in this.Values)
                {
                    NodeContainer? container = (node as INodeProcesser)?.ChildNodes;
                    if (container != null && container.NeedUpdate)
                    {
                        try
                        {
                            container.internal_UpdateNode(actionNode);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex.ToString());
                        }
                    }
                }

                try
                {
                    proccess.PostUpdateChilds(actionNode, cachingData, prveChildsReadOnly);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
            finally
            {
                readerWriterLockSlim.ExitUpgradeableReadLock();
            }
            return;
        }

        public override int IndexOf(Thing? item)
        {
            bool isReadLockHeld = readerWriterLockSlim.IsReadLockHeld || readerWriterLockSlim.IsWriteLockHeld || readerWriterLockSlim.IsUpgradeableReadLockHeld;
            if (!isReadLockHeld) readerWriterLockSlim.EnterReadLock();
            try
            {
                if (item == null) return -1;
                return indicesByThing.TryGetValue(item, out int index) ? index : -1;
            }
            finally
            {
                if (!isReadLockHeld) readerWriterLockSlim.ExitReadLock();
            }
        }

        protected override Thing? GetAt(int index) => ((IReadOnlyList<(string, Thing)>)this)[index].Item2;

        public override int GetCountCanAccept(Thing? item, bool canMergeWithExistingStacks = true)
        {
            var currentKey = CurrentKey;
            bool hasIndex = currentKey.Item2 >= 0 && currentKey.Item2 < innerList.Count;
            bool isReadLockHeld = readerWriterLockSlim.IsReadLockHeld || readerWriterLockSlim.IsWriteLockHeld || readerWriterLockSlim.IsUpgradeableReadLockHeld;
            if (!isReadLockHeld) readerWriterLockSlim.EnterReadLock();
            try
            {
                if (!currentKey.Item1.IsVaildityKeyFormat() && hasIndex)
                {
                    currentKey.Item1 = innerList[currentKey.Item2].Item1;
                }
            }
            finally
            {
                if (!isReadLockHeld) readerWriterLockSlim.ExitReadLock();
            }
            if (item?.Destroyed ?? false) return 0;
            if (item?.holdingOwner != null && item.holdingOwner != this) return 0;
            if (IsChildOf(item)) return 0;
            if (Processer.AllowNode(item, currentKey.Item1))
            {
                return base.GetCountCanAccept(item, false);
            }
            return 0;
        }

        public override int TryAdd(Thing? item, int count, bool canMergeWithExistingStacks = true)
        {

            if (count <= 0)
            {
                return 0;
            }

            if (item == null)
            {
                Log.Warning("Tried to add null item to ThingOwner.");
                return 0;
            }

            if (item.Destroyed)
            {
                Log.Warning("Tried to add destroyed item to ThingOwner.");
                return 0;
            }

            bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
            if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
            try
            {

                if (!enableWrite)
                {
                    Log.Warning("Tried to add item out side of preUpdate. Blocked...");
                    return 0;
                }

                if (Contains(item))
                {
                    Log.Warning(string.Concat("Tried to add ", item, " to ThingOwner but this item is already here."));
                    return 0;
                }

                if (!CanAcceptAnyOf(item, canMergeWithExistingStacks) || item.holdingOwner != null || IsChildOf(item))
                {
                    return 0;
                }

                int stackCount = item.stackCount;
                int num = Mathf.Min(stackCount, count);
                Thing thing = item.SplitOff(num);
                if (!TryAdd(thing, canMergeWithExistingStacks))
                {
                    if (thing != item)
                    {
                        int result = stackCount - item.stackCount - thing.stackCount;
                        item.TryAbsorbStack(thing, respectStackLimit: false);
                        return result;
                    }

                    return stackCount - item.stackCount;
                }

                return num;
            }
            finally
            {
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
            }
        }

        public override bool TryAdd(Thing? item, bool canMergeWithExistingStacks = true)
        {

            if (item == null)
            {
                Log.Warning("Tried to add null item to ThingOwner.");
                return false;
            }

            if (item.Destroyed)
            {
                Log.Warning("Tried to add destroyed item to ThingOwner.");
                return false;
            }
            bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
            if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
            try
            {

                if (!enableWrite)
                {
                    Log.Warning("Tried to add item out side of preUpdate. Blocked...");
                    return false;
                }

                var currentKey = CurrentKey;
                bool hasIndex = currentKey.Item2 >= 0 && currentKey.Item2 < innerList.Count;


                if (currentKey.Item1 == null || !currentKey.Item1.IsVaildityKeyFormat())
                {
                    if (hasIndex)
                    {
                        currentKey.Item1 = innerList[currentKey.Item2].Item1;
                    }
                    else
                    {
                        Log.Warning("Invaild key format : " + currentKey);
                        return false;
                    }
                }

                if (!hasIndex && indicesById.TryGetValue(currentKey.Item1, out int existingIndex))
                {
                    currentKey.Item2 = existingIndex;
                    hasIndex = true;
                }

                bool itemExists = indicesByThing.TryGetValue(item, out existingIndex);

                if (!CanAcceptAnyOf(item, canMergeWithExistingStacks) || (!itemExists && IsChildOf(item)))
                {
                    return false;
                }

                bool isWriteLockHeld = readerWriterLockSlim.IsWriteLockHeld;
                if (!isWriteLockHeld) readerWriterLockSlim.EnterWriteLock();
                try
                {

                    if (hasIndex)
                    {
                        (string, Thing) kv;
                        if (itemExists)
                        {
                            //remove item
                            kv = innerList[existingIndex];
                            innerList.RemoveAt(existingIndex);
                            if(currentKey.Item1 != kv.Item1) indicesById.Remove(kv.Item1);
                            for(int i = existingIndex; i < innerList.Count; i++)
                            {
                                var innerKv = innerList[i];
                                indicesById[innerKv.Item1] = i;
                                indicesByThing[innerKv.Item2] = i;
                            }
                        }
                        //replace existing
                        kv = innerList[currentKey.Item2];
                        innerList[currentKey.Item2] = (currentKey.Item1, item);

                        indicesById[currentKey.Item1] = currentKey.Item2;
                        if(currentKey.Item1 != kv.Item1) indicesById.Remove(kv.Item1);

                        indicesByThing[item] = currentKey.Item2;

                        if(item != kv.Item2)
                        {
                            indicesByThing.Remove(kv.Item2);
                            kv.Item2.holdingOwner = null;
                            item.holdingOwner = this;
                            (kv.Item2 as INodeProcesser)?.ChildNodes.CachedRootNodeNeedUpdate();
                        }
                    }
                    else if(itemExists)
                    {
                        //change key
                        var kv = innerList[existingIndex];
                        innerList[existingIndex] = (currentKey.Item1, item);
                        indicesById.Remove(kv.Item1);
                        indicesById[currentKey.Item1] = existingIndex;
                    }
                    else
                    {
                        //add new
                        indicesByThing[item] = innerList.Count;
                        indicesById[currentKey.Item1] = innerList.Count;
                        innerList.Add((currentKey.Item1, item));
                        item.holdingOwner = this;
                    }
                    if (!itemExists)
                    {
                        (item as INodeProcesser)?.ChildNodes.CachedRootNodeNeedUpdate();
                    }
                }
                finally
                {
                    if (!isWriteLockHeld) readerWriterLockSlim.ExitWriteLock();
                }
                //NeedUpdate = true;
                return true;
            }
            finally
            {
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
            }
        }

        public override bool Remove(Thing? item)
        {
            if (item == null)
            {
                Log.Warning("Tried to remove null item from ThingOwner.");
                return false;
            }

            bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
            if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
            try
            {
                if (!enableWrite)
                {
                    if (item.Destroyed && Contains(item)) NeedUpdate = true;
                    else Log.Warning("Tried to remove item out side of preUpdate. Blocked...");
                    return false;
                }

                if (!indicesByThing.TryGetValue(item, out int index))
                {
                    Log.Warning("Tried to remove item not in list. Blocked...");
                    return false;
                }

                string key = innerList[index].Item1;

                if (!Processer.AllowNode(null, key))
                {
                    return false;
                }

                bool isWriteLockHeld = readerWriterLockSlim.IsWriteLockHeld;
                if (!isWriteLockHeld) readerWriterLockSlim.EnterWriteLock();
                try
                {
                    indicesById.Remove(key);
                    indicesByThing.Remove(item);
                    innerList.RemoveAt(index);
                    for (int i = index; i < innerList.Count; i++)
                    {
                        var kv = innerList[i];
                        indicesById[kv.Item1] = i;
                        indicesByThing[kv.Item2] = i;
                    }

                    item.holdingOwner = null;
                    (item as INodeProcesser)?.ChildNodes.CachedRootNodeNeedUpdate();
                }
                finally
                {
                    if (!isWriteLockHeld) readerWriterLockSlim.ExitWriteLock();
                }
                //NeedUpdate = true;
                return true;
            }
            finally
            {
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
            }
        }


        public bool IsChildOf(Thing? node)
        {
            if (node == null) return false;
            IThingHolder thingHolder = Owner;
            Thing? parent = thingHolder as Thing;
            while (thingHolder != null && parent != node)
            {
                thingHolder = thingHolder.ParentHolder;
                parent = thingHolder as Thing;
            }
            return parent == node;
        }

        public bool IsChild(Thing? node)
        {
            if (node == null) return false;
            ThingOwner? Owner = node.holdingOwner;
            while (Owner != null && Owner != this)
            {
                Owner = Owner.Owner?.ParentHolder?.GetDirectlyHeldThings();
            }
            return Owner == this;
        }

        public bool ContainsKey(string key)
        {
            bool isReadLockHeld = readerWriterLockSlim.IsReadLockHeld || readerWriterLockSlim.IsWriteLockHeld || readerWriterLockSlim.IsUpgradeableReadLockHeld;
            if (!isReadLockHeld) readerWriterLockSlim.EnterReadLock();
            try
            {
                if (key.IsVaildityKeyFormat()) return indicesById.ContainsKey(key!);
                return false;
            }
            finally
            {
                if (!isReadLockHeld) readerWriterLockSlim.ExitReadLock();
            }
        }

        public bool ContainsKey(Thing key)
        {
            bool isReadLockHeld = readerWriterLockSlim.IsReadLockHeld || readerWriterLockSlim.IsWriteLockHeld || readerWriterLockSlim.IsUpgradeableReadLockHeld;
            if (!isReadLockHeld) readerWriterLockSlim.EnterReadLock();
            try
            {
                if (key != null) return indicesByThing.ContainsKey(key);
                return false;
            }
            finally
            {
                if (!isReadLockHeld) readerWriterLockSlim.ExitReadLock();
            }
        }

        public bool TryGetValue(string key, out Thing? value)
        {
            bool isReadLockHeld = readerWriterLockSlim.IsReadLockHeld || readerWriterLockSlim.IsWriteLockHeld || readerWriterLockSlim.IsUpgradeableReadLockHeld;
            if (!isReadLockHeld) readerWriterLockSlim.EnterReadLock();
            try
            {
                value = null;
                if (!key.IsVaildityKeyFormat()) return false;
                if (!indicesById.TryGetValue(key!, out int index)) return false;
                value = innerList[index].Item2;
                return true;
            }
            finally
            {
                if (!isReadLockHeld) readerWriterLockSlim.ExitReadLock();
            }
        }

        public bool TryGetValue(Thing key, out string? value)
        {
            bool isReadLockHeld = readerWriterLockSlim.IsReadLockHeld || readerWriterLockSlim.IsWriteLockHeld || readerWriterLockSlim.IsUpgradeableReadLockHeld;
            if (!isReadLockHeld) readerWriterLockSlim.EnterReadLock();
            try
            {
                value = null;
                if (key == null) return false;
                if (!indicesByThing.TryGetValue(key, out int index)) return false;
                value = innerList[index].Item1;
                return true;
            }
            finally
            {
                if (!isReadLockHeld) readerWriterLockSlim.ExitReadLock();
            }
        }

        public IEnumerator<(string, Thing)> GetEnumerator()
        {
            bool isReadLockHeld = readerWriterLockSlim.IsReadLockHeld || readerWriterLockSlim.IsWriteLockHeld || readerWriterLockSlim.IsUpgradeableReadLockHeld;
            if (!isReadLockHeld) readerWriterLockSlim.EnterReadLock();
            try
            {
                foreach (var kv in innerList)
                {
                    yield return kv;
                }
            }
            finally
            {
                if (!isReadLockHeld) readerWriterLockSlim.ExitReadLock();
            }
        }

        IEnumerator<string> IEnumerable<string>.GetEnumerator()
        {
            foreach (var kv in this)
            {
                yield return kv.Item1;
            }
        }

        IEnumerator<Thing> IEnumerable<Thing>.GetEnumerator()
        {
            foreach (var kv in this)
            {
                yield return kv.Item2;
            }
        }

        IEnumerator<KeyValuePair<string, Thing?>> IEnumerable<KeyValuePair<string, Thing?>>.GetEnumerator()
        {
            foreach (var kv in this)
            {
                yield return new KeyValuePair<string, Thing?>(kv.Item1, kv.Item2);
            }
        }

        IEnumerator<KeyValuePair<Thing, string?>> IEnumerable<KeyValuePair<Thing, string?>>.GetEnumerator()
        {
            foreach (var kv in this)
            {
                yield return new KeyValuePair<Thing, string?>(kv.Item2, kv.Item1);
            }
        }

        private bool needUpdate = true;

        private bool enableWrite = false;

        private INodeProcesser? cachedRootNode;

        private readonly ReaderWriterLockSlim readerWriterLockSlim = new ReaderWriterLockSlim();
        private readonly List<(string, Thing)> innerList = new List<(string, Thing)>();
        private readonly Dictionary<string, int> indicesById = new Dictionary<string, int>();
        private readonly Dictionary<Thing, int> indicesByThing = new Dictionary<Thing, int>();
        private readonly ConcurrentDictionary<int, (string?, int)> currentKey = new ConcurrentDictionary<int, (string?, int)>();

    }
}
