using RW_NodeTree.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RW_NodeTree
{
    public abstract class ThingComp_BasicNodeComp : ThingComp
    {
        public Comp_ChildNodeProccesser nodeProccesser => (Comp_ChildNodeProccesser)parent;
        public abstract void UpdateNode(Comp_ChildNodeProccesser actionNode);
        public abstract bool AllowNode(Comp_ChildNodeProccesser node, string id = null);
        public abstract void AdapteDrawSteep(List<string> ids, List<Thing> nodes, List<List<RenderInfo>> renderInfos);
    }
}
