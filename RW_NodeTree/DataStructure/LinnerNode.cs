using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RW_NodeTree.DataStructure
{
    public interface LinnerNode<T> : IList<LinnerNode<T>>
    {
        LinnerNode<T> Parent { get; set; }

        T Value { get; set; }
    }
    public interface LinnerNodeWithId<TKey, TValue> : LinnerNode<TValue>, IDictionary<TKey, LinnerNode<TValue>>
    {
        TKey Key { get; set; }
    }
}
