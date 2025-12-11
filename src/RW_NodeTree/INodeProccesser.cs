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

        /// <summary>
        /// node container
        /// </summary>
        NodeContainer ChildNodes { get; }


        /// <summary>
        /// update event
        /// </summary>
        /// <param name="actionNode">update event action node</param>
        Dictionary<string, Thing>? GenChilds(INodeProcesser actionNode, Dictionary<string, object?> cachedDataToPostUpatde);


        /// <summary>
        /// update event
        /// </summary>
        /// <param name="actionNode">update event action node</param>
        void PreUpdateChilds(INodeProcesser actionNode, Dictionary<string, object?> cachedDataToPostUpatde, ReadOnlyDictionary<string, Thing> prveChilds);

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
        /// Adapte draw steep of this node
        /// </summary>
        /// <param name="rot">rotation</param>
        /// <param name="invokeSource">Rendering Invoke Source</param>
        /// <param name="cachedDataToPostDrawStep">cached data for PostGenRenderInfos</param>
        /// <returns></returns>
        Dictionary<string, Rot4> PreGenRenderInfos(Rot4 rot, Graphic_ChildNode invokeSource, Dictionary<string, object?> cachedDataToPostDrawStep);

        /// <summary>
        /// Adapte draw steep of this node
        /// </summary>
        /// <param name="rot">rotation</param>
        /// <param name="invokeSource">Rendering Invoke Source</param>
        /// <param name="nodeRenderingInfos">Corresponding rendering infos with id and part</param>
        /// <param name="cachedDataFromPerDrawStep">cached data from PreGenRenderInfos</param>
        /// <returns></returns>
        List<RenderInfo> PostGenRenderInfos(Rot4 rot, Graphic_ChildNode invokeSource, Dictionary<string, List<RenderInfo>> nodeRenderingInfos, Dictionary<string, object?> cachedDataFromPerDrawStep);
    }
}
