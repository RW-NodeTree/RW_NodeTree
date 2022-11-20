using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RW_NodeTree.Tools
{
    /// <summary>
    /// Graphic Function
    /// </summary>
    public static class NodeHelper
    {
        public static CompChildNodeProccesser RootNode(this Thing thing) => (((CompChildNodeProccesser)thing) ?? (thing?.ParentHolder as CompChildNodeProccesser))?.RootNode;


        /// <summary>
        /// if graphic has sub graphic, It will return that;
        /// </summary>
        /// <param name="parent">current graphic</param>
        /// <returns>sub graphic</returns>
        public static Graphic SubGraphic(this Graphic parent)
        {
            if(parent != null)
            {
                Type type = parent.GetType();
                AccessTools.FieldRef<Graphic, Graphic> subGraphic;
                //FieldInfo subGraphic;
                if (!TypeFieldInfos.TryGetValue(type, out subGraphic))
                {
                    try
                    {
                        subGraphic = AccessTools.FieldRefAccess<Graphic>(type, "subGraphic");
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
            }
            return null;
        }

        /// <summary>
        /// get
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static Graphic GetGraphic_ChildNode(this Graphic parent)
        {
            Graphic graphic = parent;
            while (graphic != null && !(graphic is Graphic_ChildNode))
            {
                parent = graphic;
                graphic = graphic.SubGraphic();
            }
            //if (Prefs.DevMode) Log.Message(" parent = " + parent + " graphic = " + graphic);
            return graphic ?? parent;
        }

        private static readonly Dictionary<Type, AccessTools.FieldRef<Graphic, Graphic>> TypeFieldInfos = new Dictionary<Type, AccessTools.FieldRef<Graphic, Graphic>>();
        //private static Dictionary<Type, FieldInfo> TypeFieldInfos = new Dictionary<Type, FieldInfo>();
    }
}
