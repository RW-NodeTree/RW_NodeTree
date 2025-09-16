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
        public static INodeProccesser? RootNode(this Thing thing) => (((INodeProccesser?)thing) ?? (thing?.ParentHolder as INodeProccesser))?.ChildNodes.RootNode;


        /// <summary>
        /// if graphic has sub graphic, It will return that;
        /// </summary>
        /// <param name="parent">current graphic</param>
        /// <returns>sub graphic</returns>
        public static Graphic? GetSubGraphic(this Graphic parent)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            Type type = parent.GetType();
            AccessTools.FieldRef<Graphic, Graphic?> subGraphic;
            //FieldInfo subGraphic;
            if (!TypeFieldInfos.TryGetValue(type, out subGraphic))
            {
                try
                {
                    subGraphic = AccessTools.FieldRefAccess<Graphic?>(type, "subGraphic");
                }
                catch { }
                //subGraphic = parent?.GetType().GetField("subGraphic", AccessTools.all);
                TypeFieldInfos.Add(type, subGraphic);
            }
            //= parent?.GetType().GetField("subGraphic", AccessTools.all);
            if (subGraphic != null)
            {
                return subGraphic(parent);
                //return subGraphic.GetValue(parent) as Graphic;
            }
            return null;
        }

        /// <summary>
        /// if graphic has sub graphic, It will return that;
        /// </summary>
        /// <param name="parent">current graphic</param>
        /// <returns>sub graphic</returns>
        public static void SetSubGraphic(this Graphic parent, Graphic? graphic)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            Type type = parent.GetType();
            AccessTools.FieldRef<Graphic, Graphic?> subGraphic;
            //FieldInfo subGraphic;
            if (!TypeFieldInfos.TryGetValue(type, out subGraphic))
            {
                try
                {
                    subGraphic = AccessTools.FieldRefAccess<Graphic?>(type, "subGraphic");
                }
                catch { }
                //subGraphic = parent?.GetType().GetField("subGraphic", AccessTools.all);
                TypeFieldInfos.Add(type, subGraphic);
            }
            //= parent?.GetType().GetField("subGraphic", AccessTools.all);
            if (subGraphic != null)
            {
                subGraphic(parent) = graphic;
                //return subGraphic.GetValue(parent) as Graphic;
            }
        }

        /// <summary>
        /// get
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static Graphic_ChildNode? GetGraphic_ChildNode(this Graphic? parent)
        {
            while (parent != null && !(parent is Graphic_ChildNode))
            {
                parent = parent.GetSubGraphic();
            }
            //if (Prefs.DevMode) Log.Message(" parent = " + parent + " graphic = " + graphic);
            return parent as Graphic_ChildNode;
        }


        /// <summary>
        /// get
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static Graphic? GetParentOfGraphic_ChildNode(this Graphic? parent)
        {
            Graphic? graphic = parent?.GetSubGraphic();
            while (graphic != null && !(graphic is Graphic_ChildNode))
            {
                parent = graphic;
                graphic = graphic.GetSubGraphic();
            }
            //if (Prefs.DevMode) Log.Message(" parent = " + parent + " graphic = " + graphic);
            return parent;
        }

        /// <summary>
        /// get
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static void SetGraphic_ChildNode(this Graphic parent, Graphic_ChildNode graphic) => parent.GetParentOfGraphic_ChildNode()?.SetSubGraphic(graphic);

        private static readonly Dictionary<Type, AccessTools.FieldRef<Graphic, Graphic?>> TypeFieldInfos = new Dictionary<Type, AccessTools.FieldRef<Graphic, Graphic?>>();
        //private static Dictionary<Type, FieldInfo> TypeFieldInfos = new Dictionary<Type, FieldInfo>();
    }
}
