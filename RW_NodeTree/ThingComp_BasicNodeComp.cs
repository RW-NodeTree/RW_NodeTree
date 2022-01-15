using RimWorld;
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
        public bool Validity => NodeProccesser != null;
        public Comp_ChildNodeProccesser NodeProccesser => parent;
        public abstract void UpdateNode(Comp_ChildNodeProccesser actionNode);
        public abstract bool AllowNode(Comp_ChildNodeProccesser node, string id = null);
        public abstract void AdapteDrawSteep(List<string> ids, List<Thing> nodes, List<List<RenderInfo>> renderInfos);
        public abstract void PostStatWorker_GetValueUnfinalized(ref float result, StatWorker statWorker, StatRequest req, bool applyPostProcess = true);
        public abstract void PostStatWorker_FinalizeValue(ref float result, StatWorker statWorker, StatRequest req, bool applyPostProcess = true);
    }
}
