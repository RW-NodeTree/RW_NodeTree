using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RW_NodeTree
{
    public class Node : ThingOwner
    {
        public Node(Comp_ThingsNode comp) : base(comp,true)
        {

        }

        public Comp_ThingsNode Comp
        {
            get
            {
                return (Comp_ThingsNode)base.Owner;
            }
        }

        public override int Count
        {
            get
            {
                return innerList.Count;
            }
        }

        public Thing this[int index]
        {
            get
            {
                return GetAt(index);
            }
            set
            {
                if(Comp.AllowNode(value))
                {
                    innerList[index] = new KeyValuePair<Thing, TargetInfo>(this[index], value);
                }
            }
        }

        public override int IndexOf(Thing item)
        {
            for(int i = 0; i < innerList.Count;i++)
            {
                if(innerList[i].Key == item)
                {
                    return i;
                }
            }
            return -1;
        }

        public override bool Remove(Thing item)
        {
            for (int i = 0; i < innerList.Count; i++)
            {
                if (innerList[i].Key == item)
                {
                    innerList.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        public override int TryAdd(Thing item, int count, bool canMergeWithExistingStacks = true)
        {
            count = Math.Min(count, item.stackCount);
            for (int i = 0; i < count; i++)
            {
                if(!TryAdd(item))
                {
                    return i;
                }
            }
            return count;
        }

        public override bool TryAdd(Thing item, bool canMergeWithExistingStacks = true)
        {
            if (Comp.AllowNode(item))
            {
                innerList.Add(new KeyValuePair<Thing, TargetInfo>(item, TargetInfo.Invalid));
                return true;
            }
            return false;
        }

        protected override Thing GetAt(int index)
        {
            return innerList[index].Key;
        }

        private List<KeyValuePair<Thing, TargetInfo>> innerList = new List<KeyValuePair<Thing, TargetInfo>>();

    }
}
