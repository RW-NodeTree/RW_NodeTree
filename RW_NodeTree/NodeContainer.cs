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
    public class NodeContainer : ThingOwner<Thing>,IList<Thing>
    {

        public NodeContainer(Comp_ChildNodeProccesser proccesser) : base(proccesser)
        {
        }

        public Thing this[string id]
        {
            get
            {
                int index = innerIdList.IndexOf(id);
                return ((index >= 0) ? this[index] : null);
            }
            set
            {
                int index = innerIdList.IndexOf(id);
                Thing t = ((index >= 0) ? this[index] : null);
                if (t != null)
                {
                    Remove(t);
                }
                if (value != null && TryAdd(value))
                {
                    innerIdList[innerIdList.Count - 1] = id;
                }
            }
        }

        public string this[Thing item]
        {
            get
            {
                int index = base.IndexOf(item);
                if(index == -1) return null;
                return innerIdList[index];
            }
            set
            {
                int index = base.IndexOf(item);
                innerIdList[index] = value;
                NeedUpdate = true;
            }
        }

        public List<string> InnerIdListForReading => this.innerIdList;

        public Comp_ChildNodeProccesser Comp => (Comp_ChildNodeProccesser)base.Owner;

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

        public void UpdateNode(Comp_ChildNodeProccesser actionNode = null)
        {
            if (NeedUpdate)
            {
                Comp_ChildNodeProccesser proccess = this.Comp;
                if (actionNode == null) actionNode = proccess;
                foreach (Thing node in this)
                {
                    ((Comp_ChildNodeProccesser)node)?.UpdateNode(actionNode);
                }
                foreach (ThingComp_BasicNodeComp comp in proccess.AllNodeComp)
                {
                    comp.UpdateNode(actionNode);
                }
                foreach(ThingComp comp in proccess.parent.AllComps)
                {
                    (comp as IVerbOwner)?.VerbTracker?.VerbsNeedReinitOnLoad();
                }
                (proccess.parent as IVerbOwner)?.VerbTracker?.VerbsNeedReinitOnLoad();
                proccess.ResetRenderedTexture();
                NeedUpdate = false;
            }
        }

        public override int GetCountCanAccept(Thing item, bool canMergeWithExistingStacks = true)
        {
            Comp_ChildNodeProccesser comp_ChildNodeProccesser = (Comp_ChildNodeProccesser)Owner;
            if(comp_ChildNodeProccesser != null && comp_ChildNodeProccesser.AllowNode(item))
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
            if(Comp != null && Comp.AllowNode(item))
            {
                if (base.TryAdd(item, false))
                {
                    innerIdList.Add(null);
                    NeedUpdate = true;
                    if(item.Spawned) item.DeSpawn();
                    return true;
                }
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

        protected override Thing GetAt(int index)
        {
            return base.GetAt(index);
        }

        private bool needUpdate = true;

        private List<string> innerIdList = new List<string>();
    }
}
