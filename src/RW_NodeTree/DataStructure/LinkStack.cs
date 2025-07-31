using System;
using System.Collections;
using System.Collections.Generic;

namespace RW_NodeTree.DataStructure
{
    public class LinkStack<T> : IEnumerable<T?>, IEnumerable, IReadOnlyCollection<T?>, ICollection
    {
        public int Count
        {
            get
            {
                int result = 0;
                LinkNodeLinkNext<T>? cache = peekNode;
                while (cache != null)
                {
                    cache = cache.Next as LinkNodeLinkNext<T>;
                    result++;
                }
                return result;
            }
        }

        public bool IsSynchronized => false;

        public object? SyncRoot => null;

        public void CopyTo(Array array, int index)
        {
            if (array != null)
            {
                foreach (T? item in this)
                {
                    array.SetValue(item, index++);
                }
            }
        }

        public IEnumerator<T?> GetEnumerator()
        {
            if (peekNode != null)
            {
                foreach (var item in peekNode)
                {
                    yield return item;
                }
            }
            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public T? Peek()
        {
            return peekNode != null ? peekNode.Value : default;
        }

        public T? Pop()
        {
            LinkNodeLinkNext<T>? cache = peekNode;
            peekNode = cache?.Next as LinkNodeLinkNext<T>;
            return cache != null ? cache.Value : default;
        }

        public void Push(T? obj)
        {
            LinkNodeLinkNext<T> cache = new LinkNodeLinkNext<T>(obj);
            cache.Next = peekNode;
            peekNode = cache;
        }

        private LinkNodeLinkNext<T>? peekNode;
    }
}
