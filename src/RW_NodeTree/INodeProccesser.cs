using RW_NodeTree.Rendering;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using Verse;

namespace RW_NodeTree
{
    public partial interface INodeProccesser : IThingHolder
    {
        uint TextureSizeFactor { get; }

        float ExceedanceFactor { get; }

        float ExceedanceOffset { get; }


        FilterMode TextureFilterMode { get; }

        /// <summary>
        /// node container
        /// </summary>
        NodeContainer ChildNodes { get; }


        void PostFX(RenderTexture tar);

        bool HasPostFX(bool textureMode);

        /// <summary>
        /// update event
        /// </summary>
        /// <param name="actionNode">update event action node</param>
        Dictionary<string, Thing>? PreUpdateChilds(INodeProccesser actionNode, Dictionary<string, object?> cachedDataToPostUpatde);

        /// <summary>
        /// update event
        /// </summary>
        /// <param name="actionNode">update event action node</param>
        /// <returns>stope bubble</returns>
        void PostUpdateChilds(INodeProccesser actionNode, Dictionary<string, object?> cachedDataFromPerUpdate, ReadOnlyDictionary<string, Thing> prveChilds, out bool notUpdateTexture);

        /// <summary>
        /// allow node to append into container
        /// </summary>
        /// <param name="node">node for add</param>
        /// <param name="id">id</param>
        /// <returns>able to add into container</returns>
        bool AllowNode(Thing? node, string? id);

        /// <summary>
        /// Invoke when this item added in to container
        /// </summary>
        /// <param name="container"></param>
        /// <param name="id"></param>
        void Added(NodeContainer container, string? id, bool success);

        /// <summary>
        /// Invoke when this item removed from container
        /// </summary>
        /// <param name="container"></param>
        /// <param name="id"></param>
        void Removed(NodeContainer container, string? id, bool success);

        /// <summary>
        /// Adapte draw steep of this node
        /// </summary>
        /// <param name="nodeRenderingInfos">Corresponding rendering infos with id and part</param>
        /// <param name="rot">rotation</param>
        /// <param name="graphic">original graphic</param>
        /// <returns></returns>
        HashSet<string> PreGenRenderInfos(Rot4 rot, Dictionary<string, object?> cachedDataToPostDrawStep);

        /// <summary>
        /// Adapte draw steep of this node
        /// </summary>
        /// <param name="nodeRenderingInfos">Corresponding rendering infos with id and part</param>
        /// <param name="rot">rotation</param>
        /// <param name="graphic">original graphic</param>
        /// <returns></returns>
        Dictionary<string, List<RenderInfo>>? PostGenRenderInfos(Dictionary<string, List<RenderInfo>> nodeRenderingInfos, Rot4 rot, Graphic? graphic, Dictionary<string, object?> cachedDataFromPerDrawStep);
    }
}
