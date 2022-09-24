using HarmonyLib;
using RW_NodeTree.DataStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RW_NodeTree
{
    /// <summary>
    /// Node storge proccesser
    /// </summary>
    public class NodeContainer : ThingOwner<Thing>, IDictionary<string, Thing>
    {

        public NodeContainer(CompChildNodeProccesser proccesser) : base(proccesser) { }

        public string this[uint index] => innerIdList[(int)index];


        public Thing this[string key]
        {
            get
            {
                if (!key.NullOrEmpty())
                {
                    int index = innerIdList.IndexOf(key);
                    return ((index >= 0) ? this[index] : null);
                }
                return null;
            }
            set
            {
                if (!key.NullOrEmpty())
                {
                    Thing t = this[key];
                    if((t != null ? Remove(t) : true) && value != null)
                    {
                        innerIdList.Add(key);
                        if (!TryAdd(value) && t != null && !TryAdd(t))
                        {
                            innerIdList.RemoveAt(Count);
                        }
                    }
                }
            }
        }

        public string this[Thing item]
        {
            get
            {
                int index = base.IndexOf(item);
                if(index < 0) return null;
                return innerIdList[index];
            }
            set
            {
                if(Comp != null && !value.NullOrEmpty() && !innerIdList.Contains(value) && Comp.AllowNode(item, value))
                {
                    int index = base.IndexOf(item);
                    innerIdList[index] = value;
                    NeedUpdate = true;
                }
            }
        }

        public List<string> InnerIdListForReading => this.innerIdList;

        public CompChildNodeProccesser Comp => (CompChildNodeProccesser)base.Owner;

        public NodeContainer ParentContainer => Comp?.ParentProccesser?.ChildNodes;

        public bool NeedUpdate
        {
            get => needUpdate;
            set
            {
                if(needUpdate = value)
                {
                    NodeContainer parent = ParentContainer;
                    if(parent != null) parent.NeedUpdate = true;
                }
            }
        }

        ICollection<string> IDictionary<string, Thing>.Keys => InnerIdListForReading;

        ICollection<Thing> IDictionary<string, Thing>.Values => InnerListForReading;

        bool ICollection<KeyValuePair<string, Thing>>.IsReadOnly => ((ICollection<Thing>)this).IsReadOnly;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<string>(ref this.innerIdList, "innerIdList", LookMode.Value);
            Scribe_Values.Look<bool>(ref this.needUpdate, "needUpdate", true);
            //if (Scribe.mode == LoadSaveMode.PostLoadInit) needUpdate = false;
        }

        internal bool internal_UpdateNode(CompChildNodeProccesser actionNode = null)
        {
            bool StopEventBubble = false;
            if (NeedUpdate)
            {
                CompChildNodeProccesser proccess = this.Comp;
                if(proccess != null)
                {
                    bool reset = true;
                    if (actionNode == null) actionNode = proccess;
                    foreach (Thing node in this)
                    {
                        NodeContainer container = ((CompChildNodeProccesser)node)?.ChildNodes;
                        if (container != null)
                        {
                            StopEventBubble = container.internal_UpdateNode(actionNode) || StopEventBubble;
                            reset = false;
                        }
                    }
                    if (!StopEventBubble)
                    {
                        foreach (CompBasicNodeComp comp in proccess.AllNodeComp)
                        {
                            StopEventBubble = comp.internal_UpdateNode(actionNode) || StopEventBubble;
                        }
                    }
                    if (reset)
                    {
                        proccess.ResetVerbs();
                        proccess.ResetRenderedTexture();
                        proccess.ResetRegiestedNodeId();
                        NeedUpdate = false;
                    }
                }
            }
            return StopEventBubble;
        }

        public override int GetCountCanAccept(Thing item, bool canMergeWithExistingStacks = true)
        {
            if(Comp != null && innerIdList.Count > Count && !innerIdList[Count].NullOrEmpty() && !IsChildOf(item) && Comp.AllowNode(item, innerIdList[Count]))
            {
                return base.GetCountCanAccept(item, false);
            }
            return 0;
        }

        public override int TryAdd(Thing item, int count, bool canMergeWithExistingStacks = true)
        {
            return base.TryAdd(item, count, false);
        }

        public override bool TryAdd(Thing item, bool canMergeWithExistingStacks = true)
        {
            ThingOwner owner = item.holdingOwner;
            if(owner != this)
            {
                item.holdingOwner = null;
                if (base.TryAdd(item, false))
                {
                    ThingOwner cache = item.holdingOwner;
                    item.holdingOwner = owner;
                    item.holdingOwner?.Remove(item);
                    item.holdingOwner = cache;
                    if (item.Spawned) item.DeSpawn();
                    NeedUpdate = true;
                    return true;
                }
                item.holdingOwner = owner;
            }
            return false;
        }

        public override bool Remove(Thing item)
        {
            int index = IndexOf(item);
            if(base.Remove(item))
            {
                innerIdList.RemoveAt(index);
                return true;
            }
            return false;
        }


        public bool IsChildOf(Thing node)
        {
            if(node == null) return false;
            IThingHolder thingHolder = Owner;
            Thing parent = (thingHolder as ThingComp)?.parent ?? (thingHolder as Thing);
            while(thingHolder != null && parent != node)
            {
                thingHolder = thingHolder.ParentHolder;
                parent = (thingHolder as ThingComp)?.parent ?? (thingHolder as Thing);
            }
            return parent == node;
        }

        public bool ContainsKey(string key)
        {
            return innerIdList.Contains(key);
        }

        public void Add(string key, Thing value)
        {
            if (!key.NullOrEmpty())
            {
                Thing t = this[key];
                if (t == null && value != null)
                {
                    innerIdList.Add(key);
                    if (!TryAdd(value) && t != null && !TryAdd(t))
                    {
                        innerIdList.RemoveAt(Count);
                    }
                }
            }
        }

        public bool Remove(string key)
        {
            return Remove(this[key]);
        }

        public bool TryGetValue(string key, out Thing value)
        {
            value = null;
            if (!key.NullOrEmpty())
            {
                int index = innerIdList.IndexOf(key);
                if(index >= 0)
                {
                    value = this[index];
                    return true;
                }
            }
            return false;
        }

        void ICollection<KeyValuePair<string, Thing>>.Add(KeyValuePair<string, Thing> item)
        {
            Add(item.Key, item.Value);
        }

        bool ICollection<KeyValuePair<string, Thing>>.Contains(KeyValuePair<string, Thing> item)
        {
            return item.Value != null && this[item.Key] == item.Value;
        }

        public void CopyTo(KeyValuePair<string, Thing>[] array, int arrayIndex)
        {
            for(int i = 0; i < Count; i++)
            {
                array[i + arrayIndex] = new KeyValuePair<string, Thing>(this[(uint)i], this[i]);
            }
        }

        bool ICollection<KeyValuePair<string, Thing>>.Remove(KeyValuePair<string, Thing> item)
        {
            if(((ICollection<KeyValuePair<string, Thing>>)this).Contains(item))
            {
                return Remove(item.Value);
            }
            return false;
        }

        IEnumerator<KeyValuePair<string, Thing>> IEnumerable<KeyValuePair<string, Thing>>.GetEnumerator()
        {
            for(int i = 0; i < Count; i++)
            {
                yield return new KeyValuePair<string, Thing>(this[(uint)i], this[i]);
            }
        }

        private bool needUpdate = true;

        private List<string> innerIdList = new List<string>();
    }
}
