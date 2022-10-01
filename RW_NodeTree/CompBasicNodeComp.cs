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

        /// <summary>
        /// as name, get or set needUpdate prop of this node
        /// </summary>
        public bool NeedUpdate
        {
            get
            {
                NodeContainer ChildNodes = this.ChildNodes;
                return (ChildNodes != null) ? ChildNodes.NeedUpdate: false;
            }
            set
            {
                NodeContainer ChildNodes = this.ChildNodes;
                if (ChildNodes != null) ChildNodes.NeedUpdate = value;
            }
        }

        /// <summary>
        /// parent node
        /// </summary>
        public CompChildNodeProccesser NodeProccesser => parent;

        /// <summary>
        /// node container
        /// </summary>
        public NodeContainer ChildNodes => NodeProccesser?.ChildNodes;


        /// <summary>
        /// get parent node if it is a node
        /// </summary>
        public CompChildNodeProccesser ParentProccesser => this.ParentHolder as CompChildNodeProccesser;

        /// <summary>
        /// root of this node tree
        /// </summary>
        public CompChildNodeProccesser RootNode => NodeProccesser?.RootNode;


        /// <summary>
        /// find all comp for node
        /// </summary>
        public IEnumerable<CompBasicNodeComp> OtherNodeComp
        {
            get
            {
                foreach (ThingComp comp in parent.AllComps)
                {
                    CompBasicNodeComp c = comp as CompBasicNodeComp;
                    if (c != null && c != this)
                    {
                        yield return c;
                    }
                }
                yield break;
            }
        }

        /// <summary>
        /// update event
        /// </summary>
        /// <param name="actionNode">update event action node</param>
        /// <returns>stope bubble</returns>
        protected virtual bool UpdateNode(CompChildNodeProccesser actionNode)
        {
            return false;
        }

        /// <summary>
        /// allow node to append into container
        /// </summary>
        /// <param name="node">node for add</param>
        /// <param name="id">id</param>
        /// <returns>able to add into container</returns>
        protected virtual bool AllowNode(Thing node, string id)
        {
            return true;
        }
        /// <summary>
        /// Adapte draw steep of this node
        /// </summary>
        /// <param name="nodeRenderingInfos">Corresponding rendering infos with id and part</param>
        /// <param name="rot">rotation</param>
        /// <param name="graphic">original graphic</param>
        /// <returns></returns>
        protected virtual List<(Thing, string, List<RenderInfo>)> AdapteDrawSteep(List<(Thing, string, List<RenderInfo>)> nodeRenderingInfos, Rot4 rot, Graphic graphic)
        {
            return nodeRenderingInfos;
        }

        /// <summary>
        /// call when registed node id
        /// </summary>
        /// <param name="regiestedNodeId">registed id</param>
        protected virtual HashSet<string> RegiestedNodeId(HashSet<string> regiestedNodeId)
        {
            return regiestedNodeId;
        }


        internal bool internal_UpdateNode(CompChildNodeProccesser actionNode) => UpdateNode(actionNode);
        internal bool internal_AllowNode(Thing node, string id = null) => AllowNode(node, id);
        internal List<(Thing, string, List<RenderInfo>)> internal_AdapteDrawSteep(List<(Thing, string, List<RenderInfo>)> nodeRenderingInfos, Rot4 rot, Graphic graphic) => AdapteDrawSteep(nodeRenderingInfos, rot, graphic);
        internal HashSet<string> internal_RegiestedNodeId(HashSet<string> regiestedNodeId) => RegiestedNodeId(regiestedNodeId);
    }
}
