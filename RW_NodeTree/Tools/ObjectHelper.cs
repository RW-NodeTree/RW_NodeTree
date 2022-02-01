using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RW_NodeTree.Tools
{
    public static class ObjectHelper
    {
        public static object SimpleCopy(object obj)
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
                    foreach (FieldInfo f in type.GetFields(BindingFlagsAll))
                    {
                        f.SetValue(result, f.GetValue(obj));
                    }
                }
                return result;
            }
        }

        public const BindingFlags BindingFlagsAll = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
    }
}
