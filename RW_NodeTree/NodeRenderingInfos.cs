using RW_NodeTree.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RW_NodeTree
{
    public struct NodeRenderingInfos
    {
        public NodeRenderingInfos(Thing node, string nodeId, List<RenderInfo> renderInfos)
        {
            this.node = node;
            this.nodeId = nodeId;
            this.renderInfos = new List<RenderInfo>(renderInfos);
        }
        public Thing node;
        public string nodeId;
        public List<RenderInfo> renderInfos;
    }
}
