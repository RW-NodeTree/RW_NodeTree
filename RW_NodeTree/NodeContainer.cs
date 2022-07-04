using HarmonyLib;
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
    public class NodeContainer : ThingOwner<Thing>, IList<Thing>
    {

        public NodeContainer(CompChildNodeProccesser proccesser) : base(proccesser)
        {
        }

        public string this[uint index] => innerIdList[(int)index];


        public Thing this[string id]
        {
            get
            {
                int index = innerIdList.IndexOf(id);
                return ((index >= 0) ? this[index] : null);
            }
            set
            {
                if (!id.NullOrEmpty())
                {
                    Thing t = this[id];
                    if (t != null)
                    {
                        Remove(t);
                    }
                    innerIdList.Add(id);
                    if (value == null || (!TryAdd(value) && t != null && !TryAdd(t)))
                    {
                        innerIdList.RemoveAt(Count);
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
                if(Comp != null && !value.NullOrEmpty() && !innerIdList.Contains(value) && Comp.AllowNode(item,value))
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

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<string>(ref this.innerIdList, "innerIdList", LookMode.Value);
        }

        internal bool internal_UpdateNode(CompChildNodeProccesser actionNode = null)
        {
            bool StopEventBubble = false;
            if (NeedUpdate)
            {
                CompChildNodeProccesser proccess = this.Comp;
                if (actionNode == null) actionNode = proccess;
                foreach (Thing node in this)
                {
                    StopEventBubble = (((CompChildNodeProccesser)node)?.ChildNodes?.internal_UpdateNode(actionNode) ?? false) || StopEventBubble;
                }
                if(!StopEventBubble)
                {
                    foreach (CompBasicNodeComp comp in proccess.AllNodeComp)
                    {
                        StopEventBubble = comp.internal_UpdateNode(actionNode) || StopEventBubble;
                    }
                }
                proccess?.ResetVerbs();
                proccess?.ResetRenderedTexture();
                NeedUpdate = false;
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

        private bool needUpdate = true;

        private List<string> innerIdList = new List<string>();
    }
}
