using HarmonyLib;
using System;
using System.Collections.Generic;
using Verse;

namespace RW_NodeTree.Tools
{
    /// <summary>
    /// Graphic Function
    /// </summary>
    public static class NodeHelper
    {
        public static INodeProcesser? RootNode(this Thing thing) => (((INodeProcesser?)thing) ?? (thing?.ParentHolder as INodeProcesser))?.ChildNodes.RootNode;

        //private static Dictionary<Type, FieldInfo> TypeFieldInfos = new Dictionary<Type, FieldInfo>();
    }
}
