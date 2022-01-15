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
                return this[index];
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
                needUpdate = true;
            }
        }

        public List<string> InnerIdListForReading => this.innerIdList;

        public Comp_ChildNodeProccesser Comp => (Comp_ChildNodeProccesser)base.Owner;

        public bool NeedUpdate
        {
            get
            {
                if(needUpdate) return true;
                foreach(Thing thing in this)
                {
                    Comp_ChildNodeProccesser comp_ChildNodeProccesser = thing;
                    if(comp_ChildNodeProccesser != null && (comp_ChildNodeProccesser.ChildNodes).NeedUpdate)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public override void ExposeData()
        {
            Scribe_Values.Look<bool>(ref this.needUpdate, "needUpdate");
            base.ExposeData();
            Scribe_Collections.Look<string>(ref this.innerIdList, "innerIdList", LookMode.Value);
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
                    needUpdate = true;
                    item.DeSpawn();
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

        public bool Insert(int index, Thing item)
        {
            if (this.TryAdd(item))
            {
                InnerListForReading.Remove(item);
                InnerListForReading.Insert(index, item);
                string id = innerIdList[innerIdList.Count - 1];
                innerIdList.RemoveAt(innerIdList.Count - 1);
                innerIdList.Insert(index, id);
                needUpdate = true;
                return true;
            }
            return false;
        }

        void IList<Thing>.Insert(int index, Thing item)
        {
            Insert(index, item);
        }

        private bool needUpdate = false;

        private List<string> innerIdList = new List<string>();
    }
}
