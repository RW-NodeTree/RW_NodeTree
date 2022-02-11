using RimWorld;
using RW_NodeTree;
using RW_NodeTree.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RW_ModularizationWeapon.NodeComponent
{
    internal class NodeComp_InitializeConstructor : ThingComp_BasicNodeComp
    {

        public CompProperties_InitializeConstructor Props => (CompProperties_InitializeConstructor)props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (Validity && !respawningAfterLoad)
            {
                Comp_ChildNodeProccesser proccesser = NodeProccesser;
                foreach(ThingDef def in Props.thingDefs)
                {
                    Thing node = ThingMaker.MakeThing(def);
                    proccesser.AppendChild(node);
                }
            }
        }

        public override void AdapteDrawSteep(ref List<string> ids, ref List<Thing> nodes, ref List<List<RenderInfo>> renderInfos)
        {
            return;
        }

        public override bool AllowNode(Comp_ChildNodeProccesser node, string id = null)
        {
            return true;
        }

        public override void UpdateNode(Comp_ChildNodeProccesser actionNode)
        {
            return;
        }

        public override void PostStatWorker_GetValueUnfinalized(ref float result, StatWorker statWorker, StatRequest req, bool applyPostProcess)
        {
            return;
        }

        public override void PostStatWorker_FinalizeValue(ref float result, StatWorker statWorker, StatRequest req, bool applyPostProcess)
        {
            return;
        }

        public override void PostStatWorker_GetExplanationUnfinalized(ref string result, StatWorker statWorker, StatRequest req, ToStringNumberSense numberSense)
        {
            return;
        }

        public override void PostStatWorker_GetExplanationFinalizePart(ref string result, StatWorker statWorker, StatRequest req, ToStringNumberSense numberSense, float finalVal)
        {
            return;
        }

        public override void PostIVerbOwner_GetVerbProperties(IVerbOwner owner, ref List<VerbProperties> verbProperties)
        {
            return;
        }

        public override void PostIVerbOwner_GetTools(IVerbOwner owner, ref List<Tool> tools)
        {
            return;
        }

        public override Thing GetVerbCorrespondingThing(Verb verb)
        {
            return null;
        }
    }

    public class CompProperties_InitializeConstructor : CompProperties
    {
        public CompProperties_InitializeConstructor()
        {
            base.compClass = typeof(NodeComp_InitializeConstructor);
        }

        public List<ThingDef> thingDefs = new List<ThingDef>();
    }
}
