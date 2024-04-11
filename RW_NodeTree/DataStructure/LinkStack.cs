using System;
using System.Collections;
using System.Collections.Generic;

namespace RW_NodeTree.DataStructure
{
    public class LinkStack<T> : IEnumerable<T>, IEnumerable, IReadOnlyCollection<T>, ICollection
    {
        public int Count
        {
            get
            {
                int result = 0;
                LinkNodeLinkNext<T> cache = peekNode;
                while (cache != null)
                {
                    cache = cache.Next as LinkNodeLinkNext<T>;
                    result++;
                }
                return result;
            }
        }

        public bool IsSynchronized => false;

        public object SyncRoot => null;

        public void CopyTo(Array array, int index)
        {
            if(array != null)
            {
                foreach(T item in this)
                {
                    array.SetValue(item, index++);
                }
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return peekNode.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public T Peek()
        {
            return peekNode.Value;
        }

        public T Pop()
        {
            LinkNodeLinkNext<T> cache = peekNode;
            peekNode = cache.Next as LinkNodeLinkNext<T>;
            cache.Next = null;
            return cache.Value;
        }

        public void Push(T obj)
        {
            LinkNodeLinkNext<T> cache = new LinkNodeLinkNext<T>(obj);
            cache.Next = peekNode;
            peekNode = cache;
        }

        private LinkNodeLinkNext<T> peekNode;
    }
}
