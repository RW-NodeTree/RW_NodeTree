using RW_NodeTree.Rendering;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Verse;

namespace RW_NodeTree
{
    public partial interface INodeProcesser : IThingHolder
    {
        uint TextureSizeFactor { get; }

        float ExceedanceFactor { get; }

        float ExceedanceOffset { get; }

        GraphicsFormat TextureFormat { get; }

        FilterMode TextureFilterMode { get; }

        /// <summary>
        /// node container
        /// </summary>
        NodeContainer ChildNodes { get; }

        /// <summary>
        /// update event
        /// </summary>
        /// <param name="actionNode">update event action node</param>
        Dictionary<string, Thing>? PreUpdateChilds(INodeProcesser actionNode, Dictionary<string, object?> cachedDataToPostUpatde);

        /// <summary>
        /// update event
        /// </summary>
        /// <param name="actionNode">update event action node</param>
        /// <returns>stope bubble</returns>
        void PostUpdateChilds(INodeProcesser actionNode, Dictionary<string, object?> cachedDataFromPerUpdate, ReadOnlyDictionary<string, Thing> prveChilds);

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
        /// <param name="rot">rotation</param>
        /// <param name="invokeSource">Rendering Invoke Source</param>
        /// <param name="cachedDataToPostDrawStep">cached data for PostGenRenderInfos</param>
        /// <returns></returns>
        HashSet<string> PreGenRenderInfos(Rot4 rot, Graphic_ChildNode invokeSource, Dictionary<string, object?> cachedDataToPostDrawStep);

        /// <summary>
        /// Adapte draw steep of this node
        /// </summary>
        /// <param name="rot">rotation</param>
        /// <param name="invokeSource">Rendering Invoke Source</param>
        /// <param name="nodeRenderingInfos">Corresponding rendering infos with id and part</param>
        /// <param name="cachedDataFromPerDrawStep">cached data from PreGenRenderInfos</param>
        /// <returns></returns>
        Dictionary<string, List<RenderInfo>>? PostGenRenderInfos(Rot4 rot, Graphic_ChildNode invokeSource, Dictionary<string, List<RenderInfo>> nodeRenderingInfos, Dictionary<string, object?> cachedDataFromPerDrawStep);
    }
}
