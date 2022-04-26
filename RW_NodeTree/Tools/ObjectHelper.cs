using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RW_NodeTree.Tools
{
    /// <summary>
    /// Object simple copy function
    /// </summary>
    public static class ObjectHelper
    {
        /// <summary>
        /// try copy an object on one level
        /// </summary>
        /// <param name="obj">source</param>
        /// <returns>copyed object</returns>
        public static object SimpleCopy(this object obj)
        {
            if(obj == null)
            {
                return null;
            }
            else
            {
                Type type = obj.GetType();
                object result = null;
                if(type.IsClass && !type.IsAbstract)
                {
                    result = Activator.CreateInstance(type);
                    foreach (FieldInfo f in type.GetFields(AccessTools.all))
                    {
                        f.SetValue(result, f.GetValue(obj));
                    }
                }
                return result;
            }
        }
    }
}
