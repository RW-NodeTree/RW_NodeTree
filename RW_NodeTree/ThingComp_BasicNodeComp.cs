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
        public abstract Thing GetVerbCorrespondingThing(Verb verb);
        public abstract void UpdateNode(Comp_ChildNodeProccesser actionNode);
        public abstract bool AllowNode(Comp_ChildNodeProccesser node, string id = null);
        public abstract void AdapteDrawSteep(ref List<string> ids, ref List<Thing> nodes, ref List<List<RenderInfo>> renderInfos);
        public abstract void PostStatWorker_GetValueUnfinalized(ref float result, StatWorker statWorker, StatRequest req, bool applyPostProcess);
        public abstract void PostStatWorker_FinalizeValue(ref float result, StatWorker statWorker, StatRequest req, bool applyPostProcess);
        public abstract void PostStatWorker_GetExplanationUnfinalized(ref string result, StatWorker statWorker, StatRequest req, ToStringNumberSense numberSense);
        public abstract void PostStatWorker_GetExplanationFinalizePart(ref string result, StatWorker statWorker, StatRequest req, ToStringNumberSense numberSense, float finalVal);
        public abstract IEnumerable<Dialog_InfoCard.Hyperlink> PostStatWorker_GetInfoCardHyperlinks(IEnumerable<Dialog_InfoCard.Hyperlink> result, StatWorker statWorker, StatRequest reqstatRequest)
        public abstract void PostIVerbOwner_GetVerbProperties(IVerbOwner owner, ref List<VerbProperties> verbProperties);
        public abstract void PostIVerbOwner_GetTools(IVerbOwner owner, ref List<Tool> tools);
    }
}
