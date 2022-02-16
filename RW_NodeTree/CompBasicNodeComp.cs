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
    public abstract partial class CompBasicNodeComp : ThingComp
    {
        public bool Validity => NodeProccesser != null;
        public CompChildNodeProccesser NodeProccesser => parent;
        public virtual Thing GetVerbCorrespondingThing(VerbTracker verbTracker, Thing result, ref Verb verbBeforeConvert, ref Verb verbAfterConvert)
        {
            return result;
        }
        public virtual void UpdateNode(CompChildNodeProccesser actionNode)
        {
            return;
        }
        public virtual bool AllowNode(CompChildNodeProccesser node, string id = null)
        {
            return true;
        }
        public virtual void AdapteDrawSteep(ref List<string> ids, ref List<Thing> nodes, ref List<List<RenderInfo>> renderInfos)
        {
            return;
        }
    }
}
