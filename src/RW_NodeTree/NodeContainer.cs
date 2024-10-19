using HarmonyLib;
using RW_NodeTree.Tools;
using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using Verse;

namespace RW_NodeTree
{
    /// <summary>
    /// Node storge proccesser
    /// </summary>
    public class NodeContainer : ThingOwner, IDictionary<string, Thing>
    {

        public NodeContainer(CompChildNodeProccesser proccesser) : base(proccesser) { }

        public string this[uint index]
        {
            get
            {
                lock (this)
                {
                    return innerIdList[(int)index];
                }
            }
        }

        public Thing this[string key]
        {
            get
            {
                Thing result;
                TryGetValue(key, out result);
                return result;
            }
            set => Add(key, value);
        }

        public string this[Thing item]
        {
            get
            {
                lock (this)
                {
                    int index = IndexOf(item);
                    if (index < 0 || index >= Count) return null;
                    return innerIdList[index];
                }
            }
            set
            {
                lock (this)
                {
                    if (state == stateCode.r) return;
                    if (Comp != null && value.IsVaildityKeyFormat() && !innerIdList.Contains(value) && Comp.AllowNode(item, value))
                    {
                        int index = IndexOf(item);
                        if (index >= 0 && index < Count)
                        {
                            innerIdList[index] = value;
                        }
                    }
                }
            }
        }

        public CompChildNodeProccesser Comp => (CompChildNodeProccesser)base.Owner;

        public NodeContainer ParentContainer => Comp?.ParentProccesser?.ChildNodes;

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
                    NodeContainer parent = ParentContainer;
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
                    return new List<string>(innerIdList);
                }
            }
        }

        public ICollection<Thing> Values => this;


        bool ICollection<KeyValuePair<string, Thing>>.IsReadOnly
        {
            get
            {
                lock (this)
                    return state == stateCode.r;
            }
        }

        public override int Count
        {
            get
            {
                lock (this)
                    return Math.Min(innerList.Count, innerIdList.Count);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();

            if(Scribe.EnterNode("InnerList"))
            {
                lock (this)
                {
                    if (Scribe.mode == LoadSaveMode.Saving)
                    {
                        HashSet<string> UsedIds = DebugLoadIDsSavingErrorsChecker_deepSaved(Scribe.saver.loadIDsErrorsChecker);
                        for (int i = 0; i < Count; i++)
                        {
                            Thing thing = innerList[i];
                            string id = innerIdList[i];
                            try
                            {
                                if (UsedIds.Contains(thing.GetUniqueLoadID())) Scribe_References.Look(ref thing, id);
                                else Scribe_Deep.Look(ref thing, id);
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
                                    Thing thing = null;
                                    innerList.Add(thing);
                                    innerIdList.Add(xmlNode.Name);
                                    if (xmlNode.ChildNodes.Count == 1 && xmlNode.FirstChild.NodeType == XmlNodeType.Text)
                                    {
                                        Scribe_References.Look(ref thing, innerIdList[i]);
                                        innerIdList[i] += " !ref";
                                    }
                                    else
                                    {
                                        Scribe_Deep.Look(ref thing, innerIdList[i]);
                                    }
                                    innerList[i] = thing;
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
                        for (int i = 0; i < Count; i++)
                        {
                            try
                            {
                                Thing thing = innerList[i];
                                string id = innerIdList[i];
                                if (id.EndsWith(" !ref"))
                                {
                                    id = id.Substring(0, id.Length - 5);
                                    Scribe_References.Look(ref thing, id);
                                    innerIdList[i] = id;
                                    innerList[i] = thing;
                                }
                            }
                            catch (Exception e)
                            {
                                Log.Error(e.ToString());
                            }
                        }
                    }
                }
                Scribe.ExitNode();
            }

            if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
            {
                lock (this)
                {
                    for (int i = Count - 1; i >= 0; i--)
                    {
                        if (innerList[i] == null || innerIdList[i] == null)
                        {
                            innerList.RemoveAt(i);
                            innerIdList.RemoveAt(i);
                        }
                    }
                    for (int i = innerList.Count - 1; i >= Count; i--)
                    {
                        innerList.RemoveAt(i);
                    }
                    for (int i = innerIdList.Count - 1; i >= Count; i--)
                    {
                        innerIdList.RemoveAt(i);
                    }
                    foreach (Thing thing in innerList)
                    {
                        thing.holdingOwner = this;
                    }
                }
            }
            //Scribe_Values.Look<bool>(ref this.needUpdate, "needUpdate");
            //if (Scribe.mode == LoadSaveMode.PostLoadInit) needUpdate = false;
        }

        internal bool internal_UpdateNode(CompChildNodeProccesser actionNode = null)
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


                state = stateCode.rw;
                for (int i = Count - 1; i >= 0; i--)
                {
                    if (this[i] == null)
                    {
                        innerIdList.RemoveAt(i);
                        innerList.RemoveAt(i);
                    }
                    else if (this[i].Destroyed)
                    {
                        RemoveAt(i);
                    }
                }
                //lockUpdate = true;
                state = stateCode.rt;
                //Log.Message("1:"+state.ToString());
                Dictionary<string, object> cachingData = new Dictionary<string, object>();
                Dictionary<string, Thing> prveChilds = new Dictionary<string, Thing>(this);
                //Log.Message("2:" + state.ToString());
                this.Clear();
                //Log.Message("3:" + state.ToString());
                foreach (CompBasicNodeComp comp in proccess.AllNodeComp)
                {
                    try
                    {
                        comp.internal_PreUpdateNode(actionNode, cachingData, new Dictionary<string, Thing>(prveChilds), out bool blockEvent, out bool notUpdateTexture);
                        StopEventBubble |= blockEvent;
                        NotUpdateTexture |= notUpdateTexture;
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.ToString());
                    }
                }
                //Log.Message("4:" + state.ToString());
                Dictionary<string, Thing> diff = new Dictionary<string, Thing>(Math.Max(prveChilds.Count, Count));
                foreach (KeyValuePair<string, Thing> pair in this)
                {
                    if (!prveChilds.TryGetValue(pair.Key, out Thing prveNode) || pair.Value != prveNode) diff.Add(pair.Key, pair.Value);
                }
                //Log.Message("5:" + state.ToString());
                foreach (KeyValuePair<string, Thing> pair in prveChilds)
                {
                    if (diff.ContainsKey(pair.Key)) continue;
                    if (!this.TryGetValue(pair.Key, out Thing currentNode) || pair.Value != currentNode) diff.Add(pair.Key, currentNode);
                }
                //Log.Message("6:" + state.ToString());
                this.Clear();

                //Log.Message("7:" + state.ToString());
                state = stateCode.fw;
                foreach (KeyValuePair<string, Thing> pair in prveChilds)
                {
                    this[pair.Key] = pair.Value;
                }

                state = stateCode.rw;

                foreach (KeyValuePair<string, Thing> pair in diff)
                {
                    this[pair.Key] = pair.Value;
                }

                state = stateCode.r;
                bool reset = true;
                if (StopEventBubble) goto ret;
                foreach (Thing node in this.Values)
                {
                    NodeContainer container = ((CompChildNodeProccesser)node)?.ChildNodes;
                    if (container != null && container.NeedUpdate)
                    {
                        StopEventBubble = container.internal_UpdateNode(actionNode) || StopEventBubble;
                        reset = false;
                    }
                }

                foreach (string id in diff.Keys)
                {
                    Thing node = prveChilds.TryGetValue(id);
                    NodeContainer container = ((CompChildNodeProccesser)node)?.ChildNodes;
                    if (container != null && container.NeedUpdate)
                    {
                        StopEventBubble = container.internal_UpdateNode(actionNode) || StopEventBubble;
                        //reset = false;
                    }
                }
                if (StopEventBubble) goto ret;

                foreach (CompBasicNodeComp comp in proccess.AllNodeComp)
                {
                    try
                    {
                        comp.internal_PostUpdateNode(actionNode, cachingData, new Dictionary<string, Thing>(prveChilds), out bool blockEvent, out bool notUpdateTexture);
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
                if (Comp != null && innerIdList.Count > Count && Comp.AllowNode(item, innerIdList[Count]))
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

            if(item.Destroyed)
            {
                Log.Warning("Tried to add destroyed item to ThingOwner.");
                return 0;
            }

            lock (this)
            {

                if (state == stateCode.r)
                {
                    Log.Warning("Tried to add item out side of preUpdate. Blocked...");
                    return 0;
                }

                if (Contains(item))
                {
                    Log.Warning(string.Concat("Tried to add ", item, " to ThingOwner but this item is already here."));
                    return 0;
                }

                if ((state != stateCode.fw && !CanAcceptAnyOf(item, canMergeWithExistingStacks)) || item.holdingOwner != null || IsChildOf(item))
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

        public override bool TryAdd(Thing item, bool canMergeWithExistingStacks = true)
        {

            if (item == null)
            {
                Log.Warning("Tried to add null item to ThingOwner.");
                return false;
            }

            if(item.Destroyed)
            {
                Log.Warning("Tried to add destroyed item to ThingOwner.");
                return false;
            }

            lock (this)
            {
                if (state == stateCode.r)
                {
                    Log.Warning("Tried to add item out side of preUpdate. Blocked...");
                    return false;
                }

                string id = null;
                bool flag = innerIdList.Count > innerList.Count;
                if (flag)
                {
                    for (int i = innerIdList.Count - 2; i >= Count; i--)
                    {
                        innerIdList.RemoveAt(i);
                    }
                    id = innerIdList[Count];
                }
                else
                {
                    for (int i = innerList.Count - 1; i >= Count; i--)
                    {
                        innerList.RemoveAt(i);
                    }
                }
                Dictionary<string, object> cachedData = new Dictionary<string, object>();
                if (!innerList.Contains(item))
                {
                    if (id.IsVaildityKeyFormat())
                    {
                        if (flag) innerIdList[Count] = id;
                        else innerIdList.Add(id);
                    }
                    else
                    {
                        Log.Warning("Invaild key format : " + id);
                        goto fail;
                    }
                }
                else
                {
                    Log.Warning("Tried to add " + item.ToStringSafe() + " to ThingOwner but this item is already here.");
                    goto fail;
                }

                if ((state != stateCode.fw && !CanAcceptAnyOf(item, canMergeWithExistingStacks)) || item.holdingOwner != null || IsChildOf(item)) goto fail;

                if (Count >= maxStacks) goto fail;

                innerList.Add(item);
                if (state == stateCode.rw) ((CompChildNodeProccesser)item)?.internal_Added(this, id, true, cachedData);
                item.holdingOwner = this;
                //NeedUpdate = true;
                return true;
                fail:
                if (state == stateCode.rw) ((CompChildNodeProccesser)item)?.internal_Added(this, id, false, cachedData);
                return false;
            }
        }

        public override bool Remove(Thing item)
        {
            if (item == null)
            {
                Log.Warning("Tried to remove null item from ThingOwner.");
                return false;
            }

            lock (this)
            {
                if (state == stateCode.r)
                {
                    if (item.Destroyed && Contains(item)) NeedUpdate = true;
                    else Log.Warning("Tried to remove item out side of preUpdate. Blocked...");
                    return false;
                }

                for (int i = innerList.Count - 1; i >= Count; i--)
                {
                    innerList.RemoveAt(i);
                }
                for (int i = innerIdList.Count - 1; i >= Count; i--)
                {
                    innerIdList.RemoveAt(i);
                }

                Dictionary<string, object> cachedData = new Dictionary<string, object>();

                int index = innerList.LastIndexOf(item);
                string id = (index >= 0 && index < Count) ? innerIdList[index] : null;

                if (index < 0 || index >= Count)
                {
                    if (state == stateCode.rw) ((CompChildNodeProccesser)item)?.internal_Removed(this, id, false, cachedData);
                    return false;
                }

                innerList.RemoveAt(index);
                innerIdList.RemoveAt(index);

                if (state == stateCode.rw) ((CompChildNodeProccesser)item)?.internal_Removed(this, id, true, cachedData);
                item.holdingOwner = null;
                //NeedUpdate = true;
                return true;
            }
        }


        public bool IsChildOf(Thing node)
        {
            if (node == null) return false;
            IThingHolder thingHolder = Owner;
            Thing parent = (thingHolder as ThingComp)?.parent ?? (thingHolder as Thing);
            while (thingHolder != null && parent != node)
            {
                thingHolder = thingHolder.ParentHolder;
                parent = (thingHolder as ThingComp)?.parent ?? (thingHolder as Thing);
            }
            return parent == node;
        }

        public bool IsChild(Thing node)
        {
            if (node == null) return false;
            ThingOwner Owner = node.holdingOwner;
            while (Owner != null && Owner != this)
            {
                Owner = Owner.Owner?.ParentHolder?.GetDirectlyHeldThings();
            }
            return Owner == this;
        }

        public bool ContainsKey(string key)
        {
            lock (this)
                return innerIdList.Contains(key);
        }

        public void Add(string key, Thing value)
        {
            if (key.IsVaildityKeyFormat())
            {
                
                lock (this)
                {
                    Thing t = this[key];
                    if (value == t) return;
                    //ThingOwner owner = value?.holdingOwner;
                    if ((t != null ? Remove(t) : true) && value != null)
                    {
                        innerIdList.Add(key);
                        if (!TryAdd(value))
                        {
                            //owner?.TryAdd(value,false);
                            if (t == null || !TryAdd(t)) innerIdList.RemoveAt(Count);
                        }
                    }
                }
            }
        }

        public bool Remove(string key)
        {
            lock (this)
                return Remove(this[key]);
        }

        public bool TryGetValue(string key, out Thing value)
        {
            value = null;
            if (key.IsVaildityKeyFormat())
            {
                lock (this)
                {
                    int index = innerIdList.IndexOf(key);
                    if (index >= 0 && index < Count)
                    {
                        value = this[index];
                        return true;
                    }
                }
            }
            return false;
        }

        public IEnumerator<KeyValuePair<string, Thing>> GetEnumerator()
        {
            int count;
            List<string> keys;
            List<Thing> values;
            lock (this)
            {
                count = Count;
                keys = new List<string>(innerIdList);
                values = new List<Thing>(innerList);
            }
            for (int i = 0; i < Count; i++)
            {
                yield return new KeyValuePair<string, Thing>(keys[i], values[i]);
            }
        }

        void ICollection<KeyValuePair<string, Thing>>.Add(KeyValuePair<string, Thing> item) => Add(item.Key, item.Value);

        bool ICollection<KeyValuePair<string, Thing>>.Contains(KeyValuePair<string, Thing> item) => item.Value != null && this[item.Key] == item.Value;

        public void CopyTo(KeyValuePair<string, Thing>[] array, int arrayIndex)
        {
            lock (this)
            {
                for (int i = 0; i < Count; i++)
                {
                    array[i + arrayIndex] = new KeyValuePair<string, Thing>(this[(uint)i], this[i]);
                }
            }
        }

        bool ICollection<KeyValuePair<string, Thing>>.Remove(KeyValuePair<string, Thing> item)
        {
            lock (this)
            {
                if (((ICollection<KeyValuePair<string, Thing>>)this).Contains(item))
                {
                    return Remove(item.Value);
                }
                return false;
            }
        }

        public override int IndexOf(Thing item)
        {
            lock (this)
                return innerList.IndexOf(item);
        }

        protected override Thing GetAt(int index)
        {
            lock (this)
                return innerList[index];
        }

        private bool needUpdate = true;

        private List<Thing> innerList = new List<Thing>();

        private List<string> innerIdList = new List<string>();

        private stateCode state = 0;

        private enum stateCode : byte
        {
            r  = 0,
            rt = 1,
            fw = 2,
            rw = 3
        }

        private static readonly AccessTools.FieldRef<DebugLoadIDsSavingErrorsChecker,HashSet<string>> DebugLoadIDsSavingErrorsChecker_deepSaved = AccessTools.FieldRefAccess<DebugLoadIDsSavingErrorsChecker,HashSet<string>>( "deepSaved");

    }
}
