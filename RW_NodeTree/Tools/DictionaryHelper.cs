using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RW_NodeTree.Tools
{
    public static class DictionaryHelper
    {
        public static V GetOrAdd<K,V>(this Dictionary<K,V> dictionary, K key) where V : new ()
        {
            V value = new V();
            if (dictionary != null && key != null)
            {
                if (!dictionary.TryGetValue(key, out value))
                {
                    dictionary.Add(key, value);
                }
            }
            return value;
        }
    }
}
