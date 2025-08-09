﻿using HarmonyLib;
using RimWorld;
using RW_NodeTree.Tools;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Xml;
using UnityEngine;
using Verse;

namespace RW_NodeTree
{
    /// <summary>
    /// Node storge proccesser
    /// </summary>
    public class NodeContainer : ThingOwner, IList<string>, IList<Thing>, IDictionary<string, Thing?>, IDictionary<Thing, string?>, IList<(string, Thing)>
    {

        public NodeContainer(CompChildNodeProccesser proccesser) : base(proccesser) { }

        public Thing? this[string key]
        {
            get
            {
                TryGetValue(key, out Thing? result);
                return result;
            }
            set
            {
                bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld;
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
                try
                {
                    if (!enableWrite || key == null) return;
                    if (indicesById.TryGetValue(key, out int index))
                    {
                        if(value == null) RemoveAt(index);
                        else
                        {
                            IList<(string, Thing)> convertedSelf = this;
                            convertedSelf[index] = (key, value);
                        }
                    }
                    else if(value != null && key.IsVaildityKeyFormat())
                    {
                        CurrentKey = (key, -1);
                        TryAdd(value);
                        CurrentKey = (null, -1);
                    }
                }
                finally
                {
                    if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
                }
            }
        }

        public string? this[Thing key]
        {
            get
            {
                TryGetValue(key, out string? result);
                return result;
            }
            set
            {
                bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld;
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
                try
                {
                    if (!enableWrite || key == null) return;
                    if (indicesByThing.TryGetValue(key, out int index))
                    {
                        if (value == null) RemoveAt(index);
                        else
                        {
                            IList<(string, Thing)> convertedSelf = this;
                            convertedSelf[index] = (value, key);
                        }
                    }
                    else if (key != null && value.IsVaildityKeyFormat())
                    {
                        CurrentKey = (value, -1);
                        TryAdd(key);
                        CurrentKey = (null, -1);
                    }
                }
                finally
                {
                    if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
                }
            }
        }

        public CompChildNodeProccesser Comp => (CompChildNodeProccesser)base.Owner;

        public NodeContainer? ParentContainer => Comp.ParentProccesser?.ChildNodes;

        private (string?, int) CurrentKey
        {
            get => currentKey.GetOrAdd(Thread.CurrentThread.ManagedThreadId, (null, -1));
            set => currentKey[Thread.CurrentThread.ManagedThreadId] = value;
        }

        public bool NeedUpdate
        {
            get
            {
                readerWriterLockSlim.EnterReadLock();
                try
                {

                    return needUpdate;
                }
                finally
                {
                    readerWriterLockSlim.ExitReadLock();
                }
            }
            set
            {
                readerWriterLockSlim.EnterWriteLock();
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
                    readerWriterLockSlim.ExitWriteLock();
                }
            }
        }

        public ICollection<string> Keys => this;

        public ICollection<Thing?> Values => this;

        public override int Count => innerList.Count;

        public bool IsReadOnly => !enableWrite;

        ICollection<Thing> IDictionary<Thing, string?>.Keys => this;

        ICollection<string?> IDictionary<Thing, string?>.Values => this!;

        string IList<string>.this[int index]
        {
            get => ((IList<(string, Thing)>)this)[index].Item1;
            set
            {
                bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld;
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
                try
                {
                    if (!enableWrite) return;
                    var origin = innerList[index];
                    if (origin.Item1 == value) return;
                    CurrentKey = (value, index);
                    if (Remove(origin.Item2) && value.IsVaildityKeyFormat() && !TryAdd(origin.Item2))
                    {
                        CurrentKey = (origin.Item1, index);
                        if (!TryAdd(origin.Item2))
                        {
                            CurrentKey = (null, -1);
                            throw new InvalidOperationException("Fail to reset after value adding fail");
                        }
                    }
                    CurrentKey = (null, -1);
                }
                finally
                {
                    if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
                }
            }
        }

        Thing IList<Thing>.this[int index]
        {
            get => ((IList<(string, Thing)>)this)[index].Item2;
            set
            {
                bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld;
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
                try
                {
                    if (!enableWrite) return;
                    var origin = innerList[index];
                    if (origin.Item2 == value) return;
                    CurrentKey = (origin.Item1, index);
                    if (Remove(origin.Item2) && value != null && !TryAdd(value))
                    {
                        CurrentKey = (origin.Item1, index);
                        if (!TryAdd(origin.Item2))
                        {
                            CurrentKey = (null, -1);
                            throw new InvalidOperationException("Fail to reset after value adding fail");
                        }
                    }
                    CurrentKey = (null, -1);
                }
                finally
                {
                    if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
                }
            }
        }

        (string, Thing) IList<(string, Thing)>.this[int index]
        {
            get
            {
                readerWriterLockSlim.EnterReadLock();
                try
                {
                    return innerList[index];
                }
                finally
                {
                    readerWriterLockSlim.ExitReadLock();
                }
            }
            set
            {
                bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld;
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
                try
                {
                    if (!enableWrite || !value.Item1.IsVaildityKeyFormat()) return;
                    var origin = innerList[index];
                    if(origin == value) return;
                    CurrentKey = (value.Item1, index);
                    if (Remove(origin.Item2) && !TryAdd(value.Item2))
                    {
                        CurrentKey = (origin.Item1, index);
                        if (!TryAdd(origin.Item2))
                        {
                            CurrentKey = (null, -1);
                            throw new InvalidOperationException("Fail to reset after value adding fail");
                        }
                    }
                    CurrentKey = (null, -1);
                }
                finally
                {
                    if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();

            if (Scribe.EnterNode("InnerList"))
            {
                readerWriterLockSlim.EnterWriteLock();
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
                        int i = 0;
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
                                i++;
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
                    readerWriterLockSlim.ExitWriteLock();
                    Scribe.ExitNode();
                }
            }
            //Scribe_Values.Look<bool>(ref this.needUpdate, "needUpdate");
            //if (Scribe.mode == LoadSaveMode.PostLoadInit) needUpdate = false;
        }

        internal bool internal_UpdateNode(CompChildNodeProccesser? actionNode = null)
        {

            bool StopEventBubble = false;
            bool NotUpdateTexture = false;

            CompChildNodeProccesser proccess = this.Comp;
            if (proccess == null) return StopEventBubble;

            if (actionNode == null) return proccess.RootNode.ChildNodes.internal_UpdateNode(proccess);
            readerWriterLockSlim.EnterUpgradeableReadLock();
            try
            {
                if (!NeedUpdate) return StopEventBubble;

                NeedUpdate = false;

                enableWrite = true;
                for (int i = Count - 1; i >= 0; i--)
                {
                    if (this[i].Destroyed)
                    {
                        RemoveAt(i);
                    }
                }
                //lockUpdate = true;
                enableWrite = false;
                //Log.Message("1:"+state.ToString());
                Dictionary<string, object?> cachingData = new Dictionary<string, object?>();
                Dictionary<string, Thing> nextChilds = new Dictionary<string, Thing>();
                //Log.Message("2:" + state.ToString());
                //Log.Message("3:" + state.ToString());
                foreach (CompBasicNodeComp comp in proccess.AllNodeComp)
                {
                    try
                    {
                        Dictionary<string, Thing>? nexts = comp.internal_PreUpdateNode(actionNode, cachingData, out bool blockEvent, out bool notUpdateTexture);
                        if (nexts != null)
                        {
                            foreach (var next in nexts)
                            {
                                if (next.Value != null)
                                    nextChilds[next.Key] = next.Value;
                            }
                        }
                        StopEventBubble |= blockEvent;
                        NotUpdateTexture |= notUpdateTexture;
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.ToString());
                    }
                }
                //Log.Message("4:" + state.ToString());
                Dictionary<string, Thing> prveChilds = new Dictionary<string, Thing>(innerList.Count);
                foreach (var child in innerList)
                {
                    prveChilds[child.Item1] = child.Item2;
                }
                Dictionary<string, Thing?> diff = new Dictionary<string, Thing?>(nextChilds.Count + innerList.Count);
                foreach (var child in nextChilds)
                {
                    if (!prveChilds.TryGetValue(child.Key, out Thing? thing) || thing != child.Value)
                        diff[child.Key] = child.Value;
                }
                foreach (var child in prveChilds)
                {
                    if (!nextChilds.ContainsKey(child.Key))
                        diff[child.Key] = null;
                }
                //Log.Message("6:" + state.ToString());

                //Log.Message("7:" + state.ToString());
                enableWrite = true;
                foreach (KeyValuePair<string, Thing?> pair in diff)
                {
                    this[pair.Key] = pair.Value;
                }
                enableWrite = false;

                bool reset = true;
                if (StopEventBubble) goto ret;
                foreach (Thing? node in prveChilds.Values)
                {
                    NodeContainer? container = ((CompChildNodeProccesser?)node)?.ChildNodes;
                    if (container != null && container.NeedUpdate)
                    {
                        StopEventBubble = container.internal_UpdateNode(actionNode) || StopEventBubble;
                        reset = false;
                    }
                }

                foreach (Thing? node in diff.Values)
                {
                    NodeContainer? container = ((CompChildNodeProccesser?)node)?.ChildNodes;
                    if (container != null && container.NeedUpdate)
                    {
                        StopEventBubble = container.internal_UpdateNode(actionNode) || StopEventBubble;
                        //reset = false;
                    }
                }
                if (!StopEventBubble) goto ret;
                ReadOnlyDictionary<string, Thing> prveChildsReadOnly = new ReadOnlyDictionary<string, Thing>(prveChilds);
                foreach (CompBasicNodeComp comp in proccess.AllNodeComp)
                {
                    try
                    {
                        comp.internal_PostUpdateNode(actionNode, cachingData, prveChildsReadOnly, out bool blockEvent, out bool notUpdateTexture);
                        StopEventBubble |= blockEvent;
                        NotUpdateTexture |= notUpdateTexture;
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.ToString());
                    }
                }
                ret:;
                if (reset)
                {
                    proccess.ResetVerbs();
                    if (!NotUpdateTexture) proccess.ResetRenderedTexture();
                }
                return StopEventBubble;
            }
            finally
            {
                readerWriterLockSlim.ExitUpgradeableReadLock();
            }
        }

        public override int GetCountCanAccept(Thing? item, bool canMergeWithExistingStacks = true)
        {
            var currentKey = CurrentKey;
            bool hasIndex = currentKey.Item2 >= 0 && currentKey.Item2 < innerList.Count;
            readerWriterLockSlim.EnterReadLock();
            try
            {
                if (!currentKey.Item1.IsVaildityKeyFormat() && hasIndex)
                {
                    currentKey.Item1 = innerList[currentKey.Item2].Item1;
                }
            }
            finally
            {
                readerWriterLockSlim.ExitReadLock();
            }
            if (Comp != null && Comp.AllowNode(item, currentKey.Item1))
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

            bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld;
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
            bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld;
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

                if (!currentKey.Item1.IsVaildityKeyFormat())
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

                if (indicesById.ContainsKey(currentKey.Item1!))
                {
                    Log.Warning("Key has arrady exist : " + currentKey);
                    return false;
                }

                if (indicesByThing.ContainsKey(item))
                {
                    Log.Warning("Thing has arrady exist : " + item);
                    return false;
                }

                Dictionary<string, object?> cachedData = new Dictionary<string, object?>();

                if (!CanAcceptAnyOf(item, canMergeWithExistingStacks) || item.holdingOwner != null || IsChildOf(item))
                {
                    ((CompChildNodeProccesser?)item)?.internal_Added(this, currentKey.Item1, false, cachedData);
                    return false;
                }
                readerWriterLockSlim.EnterWriteLock();
                try
                {
                    indicesByThing[item] = innerList.Count;
                    indicesById[currentKey.Item1!] = innerList.Count;
                    if (hasIndex)
                    {
                        innerList.Insert(currentKey.Item2, (currentKey.Item1!, item));
                        for (int i = currentKey.Item2 + 1; i < innerList.Count; i++)
                        {
                            var kv = innerList[i];
                            indicesById[kv.Item1] = i;
                            indicesByThing[kv.Item2] = i;
                        }
                    }
                    else
                    {
                        innerList.Add((currentKey.Item1!, item));
                    }
                    item.holdingOwner = this;
                }
                finally
                {
                    readerWriterLockSlim.ExitWriteLock();
                }
                ((CompChildNodeProccesser?)item)?.internal_Added(this, currentKey.Item1, true, cachedData);
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
            Thing? parent = (thingHolder as ThingComp)?.parent ?? (thingHolder as Thing);
            while (thingHolder != null && parent != node)
            {
                thingHolder = thingHolder.ParentHolder;
                parent = (thingHolder as ThingComp)?.parent ?? (thingHolder as Thing);
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

        public override int IndexOf(Thing? item)
        {
            readerWriterLockSlim.EnterReadLock();
            try
            {
                if (item == null) return -1;
                return indicesByThing.TryGetValue(item, out int index) ? index : -1;
            }
            finally
            {
                readerWriterLockSlim.ExitReadLock();
            }
        }

        public int IndexOf(string? item)
        {
            readerWriterLockSlim.EnterReadLock();
            try
            {
                if (item == null) return -1;
                return indicesById.TryGetValue(item, out int index) ? index : -1;
            }
            finally
            {
                readerWriterLockSlim.ExitReadLock();
            }
        }

        public int IndexOf((string, Thing) item)
        {
            readerWriterLockSlim.EnterReadLock();
            try
            {
                int finalIndex = -1;
                bool findById = indicesById.TryGetValue(item.Item1, out int indexById);
                bool findByThing = indicesByThing.TryGetValue(item.Item2, out int indexByThing);
                if (findById && findByThing && indexById == indexByThing && innerList[indexById] == item) finalIndex = indexById;
                return finalIndex;
            }
            finally
            {
                readerWriterLockSlim.ExitReadLock();
            }
        }

        public bool ContainsKey(string key)
        {
            readerWriterLockSlim.EnterReadLock();
            try
            {
                if (key.IsVaildityKeyFormat()) return indicesById.ContainsKey(key!);
                return false;
            }
            finally
            {
                readerWriterLockSlim.ExitReadLock();
            }
        }

        public bool ContainsKey(Thing key)
        {
            readerWriterLockSlim.EnterReadLock();
            try
            {
                if (key != null) return indicesByThing.ContainsKey(key);
                return false;
            }
            finally
            {
                readerWriterLockSlim.ExitReadLock();
            }
        }

        public bool Contains((string, Thing) item)
        {
            int index = IndexOf(item);
            return index >= 0 && index < innerList.Count;
        }

        public bool Contains(KeyValuePair<string, Thing?> item) => item.Key != null && item.Value != null && Contains((item.Key, item.Value));

        public bool Contains(KeyValuePair<Thing, string?> item) => item.Key != null && item.Value != null && Contains((item.Value, item.Key));

        public bool Contains(string item) => ContainsKey(item);

        public void Add((string, Thing) item) => this[item.Item1] = item.Item2;

        public void Add(string item) => CurrentKey = (item, -1);

        public void Add(Thing item) => TryAdd(item);

        public void Add(KeyValuePair<string, Thing?> item) => this[item.Key] = item.Value;

        public void Add(KeyValuePair<Thing, string?> item) => this[item.Key] = item.Value;

        public void Add(string key, Thing? value) => this[key] = value;

        public void Add(Thing key, string? value) => this[key] = value;

        public void Insert(int index, (string, Thing) item)
        {
            bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld;
            if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
            try
            {
                if (!enableWrite || !item.Item1.IsVaildityKeyFormat()) return;
                CurrentKey = (item.Item1, index);
                TryAdd(item.Item2);
                CurrentKey = (null, -1);
            }
            finally
            {
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
            }
        }

        public void Insert(int index, Thing item)
        {
            bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld;
            if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
            try
            {
                (string?, int) key = CurrentKey;
                if (!enableWrite || !key.Item1.IsVaildityKeyFormat()) return;
                CurrentKey = (key.Item1, index);
                TryAdd(item);
                CurrentKey = key;
            }
            finally
            {
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
            }
        }

        void IList<string>.Insert(int index, string item) => CurrentKey = (item, index);

        public bool Remove(string key)
        {

            bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld;
            if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
            try
            {
                return Remove(this[key]);
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

            bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld;
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
                Dictionary<string, object?> cachedData = new Dictionary<string, object?>();
                if (!Comp.AllowNode(item, key))
                {
                    ((CompChildNodeProccesser?)item)?.internal_Removed(this, key, false, cachedData);
                    return false;
                }

                readerWriterLockSlim.EnterWriteLock();
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
                }
                finally
                {
                    readerWriterLockSlim.ExitWriteLock();
                }

                if(item.holdingOwner != this)
                {
                    ((CompChildNodeProccesser?)item)?.internal_Removed(this, key, false, cachedData);
                    return false;
                }

                item.holdingOwner = null;
                ((CompChildNodeProccesser?)item)?.internal_Removed(this, key, true, cachedData);
                //NeedUpdate = true;
                return true;
            }
            finally
            {
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
            }
        }

        public bool Remove((string, Thing) item)
        {
            bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld;
            if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
            try
            {
                int index = IndexOf(item);
                if (index >= 0 && index < innerList.Count)
                {
                    return Remove(item.Item2);
                }
                return false;
            }
            finally
            {
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
            }
        }

        public bool Remove(KeyValuePair<string, Thing?> item) => item.Key != null && item.Value != null && Remove((item.Key, item.Value));

        public bool Remove(KeyValuePair<Thing, string?> item) => item.Key != null && item.Value != null && Remove((item.Value, item.Key));

        public bool TryGetValue(string key, out Thing? value)
        {
            readerWriterLockSlim.EnterReadLock();
            try
            {
                value = null;
                if (!key.IsVaildityKeyFormat()) return false;
                if (!indicesById.TryGetValue(key!, out int index)) return false;
                value = this[index];
                return true;
            }
            finally
            {
                readerWriterLockSlim.ExitReadLock();
            }
        }

        public bool TryGetValue(Thing key, out string? value)
        {
            readerWriterLockSlim.EnterReadLock();
            try
            {
                value = null;
                if (key == null) return false;
                if (!indicesByThing.TryGetValue(key, out int index)) return false;
                value = ((IList<string>)this)[index];
                return true;
            }
            finally
            {
                readerWriterLockSlim.ExitReadLock();
            }
        }

        public void CopyTo((string, Thing)[] array, int arrayIndex)
        {
            readerWriterLockSlim.EnterReadLock();
            try
            {
                innerList.CopyTo(array, arrayIndex);
            }
            finally
            {
                readerWriterLockSlim.ExitReadLock();
            }
        }

        public void CopyTo(KeyValuePair<string, Thing?>[] array, int arrayIndex)
        {
            foreach (var kv in this)
            {
                array[arrayIndex++] = new KeyValuePair<string, Thing?>(kv.Item1, kv.Item2);
            }
        }

        public void CopyTo(KeyValuePair<Thing, string?>[] array, int arrayIndex)
        {
            foreach (var kv in this)
            {
                array[arrayIndex++] = new KeyValuePair<Thing, string?>(kv.Item2, kv.Item1);
            }
        }

        public void CopyTo(string[] array, int arrayIndex)
        {
            foreach (var kv in this)
            {
                array[arrayIndex++] = kv.Item1;
            }
        }

        public void CopyTo(Thing[] array, int arrayIndex)
        {
            foreach (var kv in this)
            {
                array[arrayIndex++] = kv.Item2;
            }
        }

        public IEnumerator<(string, Thing)> GetEnumerator()
        {
            readerWriterLockSlim.EnterReadLock();
            try
            {
                foreach (var kv in innerList)
                {
                    yield return kv;
                }
            }
            finally
            {
                readerWriterLockSlim.ExitReadLock();
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

        protected override Thing? GetAt(int index) => ((IList<(string, Thing)>)this)[index].Item2;

        private bool needUpdate = true;

        private bool enableWrite = false;

        private readonly ReaderWriterLockSlim readerWriterLockSlim = new ReaderWriterLockSlim();
        private readonly List<(string, Thing)> innerList = new List<(string, Thing)>();
        private readonly Dictionary<string, int> indicesById = new Dictionary<string, int>();
        private readonly Dictionary<Thing, int> indicesByThing = new Dictionary<Thing, int>();
        private readonly ConcurrentDictionary<int, (string?, int)> currentKey = new ConcurrentDictionary<int, (string?, int)>();


    }
}
