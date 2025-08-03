using RW_NodeTree.Rendering;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
                NodeContainer? ChildNodes = this.ChildNodes;
                return (ChildNodes != null) ? ChildNodes.NeedUpdate : false;
            }
            set
            {
                NodeContainer? ChildNodes = this.ChildNodes;
                if (ChildNodes != null) ChildNodes.NeedUpdate = value;
            }
        }

        /// <summary>
        /// parent node
        /// </summary>
        public CompChildNodeProccesser? NodeProccesser => parent;

        /// <summary>
        /// node container
        /// </summary>
        public NodeContainer? ChildNodes => NodeProccesser?.ChildNodes;


        /// <summary>
        /// get parent node if it is a node
        /// </summary>
        public CompChildNodeProccesser? ParentProccesser => NodeProccesser?.ParentProccesser;

        /// <summary>
        /// root of this node tree
        /// </summary>
        public CompChildNodeProccesser? RootNode => NodeProccesser?.RootNode;


        /// <summary>
        /// find all comp for node
        /// </summary>
        public IEnumerable<CompBasicNodeComp> OtherNodeComp
        {
            get
            {
                foreach (ThingComp comp in parent.AllComps)
                {
                    CompBasicNodeComp? c = comp as CompBasicNodeComp;
                    if (c != null && c != this)
                    {
                        yield return c;
                    }
                }
                yield break;
            }
        }


        public virtual bool HasPostFX(bool textureMode) => false;



        public virtual void PostFX(RenderTexture tar) { }


        /// <summary>
        /// update event
        /// </summary>
        /// <param name="actionNode">update event action node</param>
        protected virtual Dictionary<string, Thing>? PreUpdateNode(CompChildNodeProccesser actionNode, Dictionary<string, object?> cachedDataToPostUpatde, out bool blockEvent, out bool notUpdateTexture)
        {
            blockEvent = false;
            notUpdateTexture = false;
            return null;
        }

        /// <summary>
        /// update event
        /// </summary>
        /// <param name="actionNode">update event action node</param>
        /// <returns>stope bubble</returns>
        protected virtual void PostUpdateNode(CompChildNodeProccesser actionNode, Dictionary<string, object?> cachedDataFromPerUpdate, ReadOnlyDictionary<string, Thing> prveChilds, out bool blockEvent, out bool notUpdateTexture)
        {
            blockEvent = false;
            notUpdateTexture = false;
        }

        /// <summary>
        /// allow node to append into container
        /// </summary>
        /// <param name="node">node for add</param>
        /// <param name="id">id</param>
        /// <returns>able to add into container</returns>
        protected virtual bool AllowNode(Thing? node, string? id)
        {
            return true;
        }

        /// <summary>
        /// Invoke when this item added in to container
        /// </summary>
        /// <param name="container"></param>
        /// <param name="id"></param>
        protected virtual void Added(NodeContainer container, string? id, bool success, Dictionary<string, object?> cachedData)
        {
            return;
        }

        /// <summary>
        /// Invoke when this item removed from container
        /// </summary>
        /// <param name="container"></param>
        /// <param name="id"></param>
        protected virtual void Removed(NodeContainer container, string? id, bool success, Dictionary<string, object?> cachedData)
        {
            return;
        }

        /// <summary>
        /// Adapte draw steep of this node
        /// </summary>
        /// <param name="nodeRenderingInfos">Corresponding rendering infos with id and part</param>
        /// <param name="rot">rotation</param>
        /// <param name="graphic">original graphic</param>
        /// <returns></returns>
        protected virtual List<(string?, Thing, List<RenderInfo>)> PreDrawStep(List<(string?, Thing, List<RenderInfo>)> nodeRenderingInfos, Rot4 rot, Graphic? graphic, Dictionary<string, object> cachedDataToPostDrawStep)
        {
            return nodeRenderingInfos;
        }

        /// <summary>
        /// Adapte draw steep of this node
        /// </summary>
        /// <param name="nodeRenderingInfos">Corresponding rendering infos with id and part</param>
        /// <param name="rot">rotation</param>
        /// <param name="graphic">original graphic</param>
        /// <returns></returns>
        protected virtual List<(string?, Thing, List<RenderInfo>)> PostDrawStep(List<(string?, Thing, List<RenderInfo>)> nodeRenderingInfos, Rot4 rot, Graphic? graphic, Dictionary<string, object> cachedDataFromPerDrawStep)
        {
            return nodeRenderingInfos;
        }


        protected virtual List<VerbPropertiesRegiestInfo> VerbPropertiesRegiestInfoUpadte(Type ownerType, List<VerbPropertiesRegiestInfo> result)
        {
            return result;
        }

        protected virtual List<VerbToolRegiestInfo> VerbToolRegiestInfoUpdate(Type ownerType, List<VerbToolRegiestInfo> result)
        {
            return result;
        }

        internal Dictionary<string, Thing>? internal_PreUpdateNode(CompChildNodeProccesser actionNode, Dictionary<string, object?> cachedDataToPostUpatde, out bool blockEvent, out bool notUpdateTexture) => PreUpdateNode(actionNode, cachedDataToPostUpatde, out blockEvent, out notUpdateTexture);
        internal void internal_PostUpdateNode(CompChildNodeProccesser actionNode, Dictionary<string, object?> cachedDataFromPerUpdate, ReadOnlyDictionary<string, Thing> prveChilds, out bool blockEvent, out bool notUpdateTexture) => PostUpdateNode(actionNode, cachedDataFromPerUpdate, prveChilds, out blockEvent, out notUpdateTexture);
        internal bool internal_AllowNode(Thing? node, string? id = null) => AllowNode(node, id);
        internal void internal_Added(NodeContainer container, string? id, bool success, Dictionary<string, object?> cachedData) => Added(container, id, success, cachedData);
        internal void internal_Removed(NodeContainer container, string? id, bool success, Dictionary<string, object?> cachedData) => Removed(container, id, success, cachedData);
        internal List<(string?, Thing, List<RenderInfo>)> internal_PreDrawStep(List<(string?, Thing, List<RenderInfo>)> nodeRenderingInfos, Rot4 rot, Graphic? graphic, Dictionary<string, object> cachedDataToPostDrawStep) => PreDrawStep(nodeRenderingInfos, rot, graphic, cachedDataToPostDrawStep);
        internal List<(string?, Thing, List<RenderInfo>)> internal_PostDrawStep(List<(string?, Thing, List<RenderInfo>)> nodeRenderingInfos, Rot4 rot, Graphic? graphic, Dictionary<string, object> cachedDataFromPerDrawStep) => PostDrawStep(nodeRenderingInfos, rot, graphic, cachedDataFromPerDrawStep);
        internal List<VerbPropertiesRegiestInfo> internal_VerbPropertiesRegiestInfoUpadte(Type ownerType, List<VerbPropertiesRegiestInfo> result) => VerbPropertiesRegiestInfoUpadte(ownerType, result);
        internal List<VerbToolRegiestInfo> internal_VerbToolRegiestInfoUpdate(Type ownerType, List<VerbToolRegiestInfo> result) => VerbToolRegiestInfoUpdate(ownerType, result);
    }
}
