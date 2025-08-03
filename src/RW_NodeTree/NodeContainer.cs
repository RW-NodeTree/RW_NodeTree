using HarmonyLib;
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
    public class NodeContainer : ThingOwner, IDictionary<string, Thing?>
    {

        public NodeContainer(CompChildNodeProccesser proccesser) : base(proccesser) { }

        public string this[uint index]
        {
            get
            {
                lock (this)
                    return innerList[(int)index].Item1;
            }
        }

        public Thing? this[string key]
        {
            get
            {
                TryGetValue(key, out Thing? result);
                return result;
            }
            set
            {
                if (!key.IsVaildityKeyFormat() || !enableWrite) return;
                lock (this)
                {
                    if (value != null && indicesByThing.ContainsKey(value)) return;
                    Thing? origin = this[key];
                    currentKey = key;
                    if ((origin == null || Remove(origin)) && !TryAdd(value))
                    {
                        if (origin != null && !TryAdd(origin))
                        {
                            currentKey = null;
                            throw new InvalidOperationException("Fail to reset after value adding fail");
                        }
                    }
                    currentKey = null;
                }
            }
        }

        public string? this[Thing? item]
        {
            get
            {
                lock (this)
                {
                    int index = IndexOf(item);
                    return index > 0 ? innerList[index].Item1 : null;
                }
            }
            set
            {
                lock (this)
                {
                    if (!value.IsVaildityKeyFormat() || !enableWrite) return;
                    lock (this)
                    {
                        if (item == null || !indicesByThing.ContainsKey(item)) return;
                        string? origin = this[item];
                        currentKey = value;
                        if ((origin == null || Remove(origin)) && !TryAdd(item))
                        {
                            currentKey = origin;
                            if (origin != null && !TryAdd(item))
                            {
                                currentKey = null;
                                throw new InvalidOperationException("Fail to reset after value adding fail");
                            }
                        }
                        currentKey = null;
                    }
                }
            }
        }

        public CompChildNodeProccesser Comp => (CompChildNodeProccesser)base.Owner;

        public NodeContainer? ParentContainer => Comp.ParentProccesser?.ChildNodes;

        public bool NeedUpdate
        {
            get
            {
                lock (this)
                    return needUpdate;
            }
            set
            {
                bool flag;
                lock (this)
                    flag = needUpdate != value && (needUpdate = value);
                if (flag)
                {
                    NodeContainer? parent = ParentContainer;
                    if (parent != null) parent.NeedUpdate = true;
                }
            }
        }

        public ICollection<string> Keys
        {
            get
            {
                lock (this)
                {
                    string[] keys = new string[innerList.Count];
                    for (int i = 0; i < innerList.Count; i++)
                    {
                        keys[i] = innerList[i].Item1;
                    }
                    return keys;
                }
            }
        }

        public ICollection<Thing?> Values => this;


        bool ICollection<KeyValuePair<string, Thing?>>.IsReadOnly => !enableWrite;

        public override int Count => innerList.Count;

        public override void ExposeData()
        {
            base.ExposeData();

            if (Scribe.EnterNode("InnerList"))
            {
                lock (this)
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
                Scribe.ExitNode();
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
            lock (this)
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
                foreach(var child in innerList)
                {
                    prveChilds[child.Item1] = child.Item2;
                }
                Dictionary<string, Thing?> diff = new Dictionary<string, Thing?>(nextChilds.Count + innerList.Count);
                foreach(var child in nextChilds)
                {
                    if(!prveChilds.TryGetValue(child.Key, out Thing? thing) || thing != child.Value)
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
                if (StopEventBubble) goto ret;
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
        }

        public override int GetCountCanAccept(Thing item, bool canMergeWithExistingStacks = true)
        {
            lock (this)
            {
                if (Comp != null && Comp.AllowNode(item, currentKey))
                {
                    return base.GetCountCanAccept(item, false);
                }
                return 0;
            }
        }

        public override int TryAdd(Thing item, int count, bool canMergeWithExistingStacks = true)
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

            lock (this)
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

            lock (this)
            {
                if (!enableWrite)
                {
                    Log.Warning("Tried to add item out side of preUpdate. Blocked...");
                    return false;
                }

                if (!currentKey.IsVaildityKeyFormat())
                {
                    Log.Warning("Invaild key format : " + currentKey);
                    return false;
                }

                if (indicesById.ContainsKey(currentKey!))
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
                    ((CompChildNodeProccesser?)item)?.internal_Added(this, currentKey, false, cachedData);
                    return false;
                }

                innerList.Add((currentKey!, item));
                item.holdingOwner = this;
                ((CompChildNodeProccesser?)item)?.internal_Added(this, currentKey, true, cachedData);
                //NeedUpdate = true;
                return true;
            }
        }

        public override bool Remove(Thing? item)
        {
            if (item == null)
            {
                Log.Warning("Tried to remove null item from ThingOwner.");
                return false;
            }

            lock (this)
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
                indicesById.Remove(key);
                indicesByThing.Remove(item);
                innerList.RemoveAt(index);
                for (int i = index; i < innerList.Count; i++)
                {
                    var kv = innerList[i];
                    indicesById[kv.Item1] = i;
                    indicesByThing[kv.Item2] = i;
                }
                if (item.holdingOwner != this)
                {
                    ((CompChildNodeProccesser?)item)?.internal_Removed(this, key, false, cachedData);
                    return false;
                }

                item.holdingOwner = null;
                ((CompChildNodeProccesser?)item)?.internal_Removed(this, key, true, cachedData);
                //NeedUpdate = true;
                return true;
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

        public bool ContainsKey(string key)
        {
            if (key.IsVaildityKeyFormat())
                lock (this)
                    return indicesById.ContainsKey(key!);
            return false;
        }

        public void Add(string key, Thing? value) => this[key] = value;

        public bool Remove(string key)
        {
            lock (this)
                return Remove(this[key]);
        }

        public bool TryGetValue(string key, out Thing? value)
        {
            value = null;
            if (!key.IsVaildityKeyFormat()) return false;
            lock (this)
            {
                if (!indicesById.TryGetValue(key, out int index)) return false;
                value = this[index];
                return true;
            }
        }

        public IEnumerator<KeyValuePair<string, Thing?>> GetEnumerator()
        {
            List<(string, Thing)> innerListCopy;
            lock (this)
            {
                innerListCopy = new List<(string, Thing)>(innerList);
            }
            foreach(var kv in innerListCopy)
            {
                yield return new KeyValuePair<string, Thing?>(kv.Item1, kv.Item2);
            }
        }

        void ICollection<KeyValuePair<string, Thing?>>.Add(KeyValuePair<string, Thing?> item) => Add(item.Key, item.Value);

        bool ICollection<KeyValuePair<string, Thing?>>.Contains(KeyValuePair<string, Thing?> item) => item.Value != null && this[item.Key] == item.Value;

        public void CopyTo(KeyValuePair<string, Thing?>[] array, int arrayIndex)
        {
            lock (this)
            {
                for (int i = 0; i < Count; i++)
                {
                    var kv = innerList[i];
                    array[i + arrayIndex] = new KeyValuePair<string, Thing?>(kv.Item1, kv.Item2);
                }
            }
        }

        bool ICollection<KeyValuePair<string, Thing?>>.Remove(KeyValuePair<string, Thing?> item)
        {
            lock (this)
            {
                if (((ICollection<KeyValuePair<string, Thing?>>)this).Contains(item))
                {
                    return Remove(item.Value);
                }
                return false;
            }
        }

        public override int IndexOf(Thing? item)
        {
            if (item == null) return -1;
            lock (this)
                return indicesByThing.TryGetValue(item, out int index) ? index : -1;
        }

        protected override Thing? GetAt(int index)
        {
            lock (this)
                return innerList[index].Item2;
        }

        private bool needUpdate = true;

        private bool enableWrite = false;

        private readonly List<(string, Thing)> innerList = new List<(string, Thing)>();
        private readonly Dictionary<string, int> indicesById = new Dictionary<string, int>();
        private readonly Dictionary<Thing, int> indicesByThing = new Dictionary<Thing, int>();
        private string? currentKey;


    }
}
