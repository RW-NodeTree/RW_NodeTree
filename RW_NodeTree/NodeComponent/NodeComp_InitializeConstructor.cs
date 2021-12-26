using RW_NodeTree.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RW_NodeTree.NodeComponent
{
    internal class NodeComp_InitializeConstructor : ThingComp_BasicNodeComp
    {

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            Comp_ChildNodeProccesser proccesser = nodeProccesser;

        }
        public override void AdapteDrawSteep(List<string> ids, List<Thing> nodes, List<List<RenderInfo>> renderInfos)
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
    }
}
