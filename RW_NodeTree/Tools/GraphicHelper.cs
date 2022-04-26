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
    public static class GraphicHelper
    {
        /// <summary>
        /// if graphic has sub graphic, It will return that;
        /// </summary>
        /// <param name="parent">current graphic</param>
        /// <returns>sub graphic</returns>
        public static Graphic subGraphic(this Graphic parent)
        {
            FieldInfo fieldInfo = parent?.GetType().GetField("subGraphic", AccessTools.all);
            if(fieldInfo != null)
            {
                return fieldInfo.GetValue(parent) as Graphic;
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
                graphic = graphic.subGraphic();
            }
            //if (Prefs.DevMode) Log.Message(" parent = " + parent + " graphic = " + graphic);
            return graphic ?? parent;
        }
    }
}
