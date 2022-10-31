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
        public CompChildNodeProccesser ParentProccesser => NodeProccesser?.ParentProccesser;

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
        /// adapt the node info before you add node in to continer
        /// </summary>
        /// <param name="node"></param>
        /// <param name="id"></param>
        protected virtual void PerAdd(ref Thing node, ref string id)
        {
            return;
        }

        /// <summary>
        /// adapt the node info after you add node in to continer
        /// </summary>
        /// <param name="node"></param>
        /// <param name="id"></param>
        protected virtual void PostAdd(Thing node, string id, bool success)
        {
            return;
        }

        /// <summary>
        /// adapt the node info before you remove node from continer
        /// </summary>
        /// <param name="node"></param>
        /// <param name="id"></param>
        protected virtual void PerRemove(ref Thing node)
        {
            return;
        }

        /// <summary>
        /// adapt the node info after you add remove from continer
        /// </summary>
        /// <param name="node"></param>
        /// <param name="id"></param>
        protected virtual void PostRemove(Thing node, string id, bool success)
        {
            return;
        }

        /// <summary>
        /// Invoke when this item added in to container
        /// </summary>
        /// <param name="container"></param>
        /// <param name="id"></param>
        protected virtual void Added(NodeContainer container, string id)
        {
            return;
        }

        /// <summary>
        /// Invoke when this item removed from container
        /// </summary>
        /// <param name="container"></param>
        /// <param name="id"></param>
        protected virtual void Removed(NodeContainer container, string id)
        {
            return;
        }

        /// <summary>
        /// call when registed node id
        /// </summary>
        /// <param name="regiestedNodeId">registed id</param>
        protected virtual HashSet<string> RegiestedNodeId(HashSet<string> regiestedNodeId)
        {
            return regiestedNodeId;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="orginal"></param>
        /// <returns></returns>
        protected virtual CompChildNodeProccesser OverrideParentProccesser(CompChildNodeProccesser orginal)
        {
            return orginal;
        }

        /// <summary>
        /// Adapte draw steep of this node
        /// </summary>
        /// <param name="nodeRenderingInfos">Corresponding rendering infos with id and part</param>
        /// <param name="rot">rotation</param>
        /// <param name="graphic">original graphic</param>
        /// <returns></returns>
        protected virtual List<(Thing, string, List<RenderInfo>)> OverrideDrawSteep(List<(Thing, string, List<RenderInfo>)> nodeRenderingInfos, Rot4 rot, Graphic graphic)
        {
            return nodeRenderingInfos;
        }


        internal bool internal_UpdateNode(CompChildNodeProccesser actionNode) => UpdateNode(actionNode);
        internal bool internal_AllowNode(Thing node, string id = null) => AllowNode(node, id);
        internal void internal_PerAdd(ref Thing node, ref string id) => PerAdd(ref node, ref id);
        internal void internal_PostAdd(Thing node, string id, bool success) => PostAdd(node, id, success);
        internal void internal_PerRemove(ref Thing node) => PerRemove(ref node);
        internal void internal_PostRemove(Thing node, string id, bool success) => PostRemove(node, id, success);
        internal void internal_Added(NodeContainer container, string id) => Added(container, id);
        internal void internal_Removed(NodeContainer container, string id) => Removed(container, id);
        internal HashSet<string> internal_RegiestedNodeId(HashSet<string> regiestedNodeId) => RegiestedNodeId(regiestedNodeId);
        internal CompChildNodeProccesser internal_OverrideParentProccesser(CompChildNodeProccesser orginal) => OverrideParentProccesser(orginal);
        internal List<(Thing, string, List<RenderInfo>)> internal_OverrideDrawSteep(List<(Thing, string, List<RenderInfo>)> nodeRenderingInfos, Rot4 rot, Graphic graphic) => OverrideDrawSteep(nodeRenderingInfos, rot, graphic);
    }
}
