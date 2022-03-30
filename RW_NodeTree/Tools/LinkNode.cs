using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStructor
{
    public interface LinkNode<T>
    {
        T Data { get; set; }
        void DropNode();
    }
    public interface LinkNodeHasNext<T> : LinkNode<T>, IEnumerable<T>
    {
        LinkNode<T> Next { get; set; }
        bool InsertNext(LinkNode<T> next);
    }
    public interface LinkNodeHasPerv<T> : LinkNode<T>, IEnumerable<T>
    {
        LinkNode<T> Perv { get; set; }
        bool InsertPerv(LinkNode<T> next);
    }

    public class LinkNodeLinkNext<T> : LinkNodeHasNext<T>
    {
        public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
        {
            public Enumerator(LinkNodeHasNext<T> head) : this()
            {
                _head = head;
            }

            private LinkNodeHasNext<T> _head;

            private LinkNodeHasNext<T> _currentElement;

            public T Current
            {
                get
                {
                    if (_currentElement == null)
                    {
                        return default(T);
                    }
                    return _currentElement.Data;
                }
            }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                _currentElement = null;
                _head = null;
            }

            public bool MoveNext()
            {
                bool isNullBefore = _currentElement == null;
                _currentElement = _currentElement?.Next as LinkNodeHasNext<T> ?? _head;
                return _currentElement != null && (_currentElement != _head || isNullBefore);
            }

            public void Reset()
            {
                _currentElement = null;
            }
        }
        public LinkNodeLinkNext(T data)
        {
            Data = data;
        }

        public LinkNode<T> Next 
        { 
            get => next; 
            set
            {
                LinkNodeHasPerv<T> cachedNext = next as LinkNodeHasPerv<T>;
                next = value;
                if (cachedNext != null)
                {
                    cachedNext.Perv = null;
                }
                cachedNext = value as LinkNodeHasPerv<T>;
                if (cachedNext != null)
                {
                    LinkNodeHasNext<T> cachedPerv = cachedNext.Perv as LinkNodeHasNext<T>;
                    if (cachedPerv != null)
                    {
                        cachedPerv.Next = null;
                    }
                    cachedNext.Perv = this;
                }
            }
        }
        public T Data { get => data; set => data = value; }

        public void DropNode()
        {
            LinkNodeHasPerv<T> next = Next as LinkNodeHasPerv<T>;
            if (next != null)
            {
                next.Perv = null;
            }
            Next = null;
        }

        public bool InsertNext(LinkNode<T> next)
        {
            if(next != null)
            {
                next.DropNode();
                LinkNodeHasNext<T> cachedPerv = next as LinkNodeHasNext<T>;
                if (cachedPerv != null)
                {
                    cachedPerv.Next = Next;
                }
                LinkNodeHasPerv<T> cachedNext = next as LinkNodeHasPerv<T>;
                if (cachedNext != null)
                {
                    cachedNext.Perv = this;
                }
                cachedNext = Next as LinkNodeHasPerv<T>;
                if (cachedNext != null)
                {
                    cachedNext.Perv = next;
                }
                Next = next;
                return true;

            }
            return false;
        }

        public IEnumerator<T> GetEnumerator()
        {
            LinkNodeLinkNext<T>.Enumerator result = new LinkNodeLinkNext<T>.Enumerator(this);
            return result;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private LinkNode<T> next = null;

        private T data;
    }
    public class LinkNodeLinkPerv<T> : LinkNodeHasPerv<T>
    {
        public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
        {
            public Enumerator(LinkNodeHasPerv<T> head) : this()
            {
                _head = head;
            }

            private LinkNodeHasPerv<T> _head;

            private LinkNodeHasPerv<T> _currentElement;

            public T Current
            {
                get
                {
                    if (_currentElement == null)
                    {
                        return default(T);
                    }
                    return _currentElement.Data;
                }
            }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                _currentElement = null;
                _head = null;
            }

            public bool MoveNext()
            {
                bool isNullBefore = _currentElement == null;
                _currentElement = _currentElement?.Perv as LinkNodeHasPerv<T> ?? _head;
                return _currentElement != null && (_currentElement != _head || isNullBefore);
            }

            public void Reset()
            {
                _currentElement = null;
            }
        }
        public LinkNodeLinkPerv(T data)
        {
            Data = data;
        }

        public LinkNode<T> Perv
        {
            get => perv;
            set
            {
                LinkNodeHasNext<T> cachedPerv = perv as LinkNodeHasNext<T>;
                perv = value;
                if (cachedPerv != null)
                {
                    cachedPerv.Next = null;
                }
                cachedPerv = value as LinkNodeHasNext<T>;
                if (cachedPerv != null)
                {
                    LinkNodeHasPerv<T> cachedNext = cachedPerv.Next as LinkNodeHasPerv<T>;
                    if (cachedNext != null)
                    {
                        cachedNext.Perv = null;
                    }
                    cachedPerv.Next = this;
                }
            }
        }
        public T Data { get => data; set => data = value; }

        public void DropNode()
        {
            LinkNodeHasNext<T> perv = Perv as LinkNodeHasNext<T>;
            if (perv != null)
            {
                perv.Next = null;
            }
            Perv = null;
        }

        public bool InsertPerv(LinkNode<T> perv)
        {
            if (perv != null)
            {
                perv.DropNode();
                LinkNodeHasPerv<T> cachedNext = perv as LinkNodeHasPerv<T>;
                if (cachedNext != null)
                {
                    cachedNext.Perv = Perv;
                }
                LinkNodeHasNext<T> cachedPerv = perv as LinkNodeHasNext<T>;
                if (cachedPerv != null)
                {
                    cachedPerv.Next = this;
                }
                cachedPerv = Perv as LinkNodeHasNext<T>;
                if (cachedPerv != null)
                {
                    cachedPerv.Next = perv;
                }
                Perv = perv;
                return true;

            }
            return false;
        }

        public IEnumerator<T> GetEnumerator()
        {
            LinkNodeLinkPerv<T>.Enumerator result = new LinkNodeLinkPerv<T>.Enumerator(this);
            return result;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private LinkNode<T> perv = null;

        private T data;
    }
    public class LinkNodeLinkNextAndPerv<T> : LinkNodeHasNext<T> , LinkNodeHasPerv<T>
    {
        public LinkNodeLinkNextAndPerv(T data)
        {
            Data = data;
        }

        public LinkNode<T> Next
        {
            get => next;
            set
            {
                LinkNodeHasPerv<T> cachedNext = next as LinkNodeHasPerv<T>;
                next = value;
                if (cachedNext != null)
                {
                    cachedNext.Perv = null;
                }
                cachedNext = value as LinkNodeHasPerv<T>;
                if (cachedNext != null)
                {
                    LinkNodeHasNext<T> cachedPerv = cachedNext.Perv as LinkNodeHasNext<T>;
                    if (cachedPerv != null)
                    {
                        cachedPerv.Next = null;
                    }
                    cachedNext.Perv = this;
                }
            }
        }
        public LinkNode<T> Perv
        {
            get => perv;
            set
            {
                LinkNodeHasNext<T> cachedPerv = perv as LinkNodeHasNext<T>;
                perv = value;
                if (cachedPerv != null)
                {
                    cachedPerv.Next = null;
                }
                cachedPerv = value as LinkNodeHasNext<T>;
                if (cachedPerv != null)
                {
                    LinkNodeHasPerv<T> cachedNext = cachedPerv.Next as LinkNodeHasPerv<T>;
                    if (cachedNext != null)
                    {
                        cachedNext.Perv = null;
                    }
                    cachedPerv.Next = this;
                }
            }
        }
        public T Data { get => data; set => data = value; }

        public void DropNode()
        {
            LinkNodeHasPerv<T> next = Next as LinkNodeHasPerv<T>;
            if (next != null)
            {
                next.Perv = Perv;
            }
            LinkNodeHasNext<T> perv = Perv as LinkNodeHasNext<T>;
            if (perv != null)
            {
                perv.Next = Next;
            }
            Next = null;
            Perv = null;
        }

        public bool InsertNext(LinkNode<T> next)
        {
            if (next != null)
            {
                next.DropNode();
                LinkNodeHasNext<T> cachedPerv = next as LinkNodeHasNext<T>;
                if (cachedPerv != null)
                {
                    cachedPerv.Next = Next;
                }
                LinkNodeHasPerv<T> cachedNext = next as LinkNodeHasPerv<T>;
                if (cachedNext != null)
                {
                    cachedNext.Perv = this;
                }
                cachedNext = Next as LinkNodeHasPerv<T>;
                if (cachedNext != null)
                {
                    cachedNext.Perv = next;
                }
                Next = next;
                return true;

            }
            return false;
        }


        public bool InsertPerv(LinkNode<T> perv)
        {
            if (perv != null)
            {
                perv.DropNode();
                LinkNodeHasPerv<T> cachedNext = perv as LinkNodeHasPerv<T>;
                if (cachedNext != null)
                {
                    cachedNext.Perv = Perv;
                }
                LinkNodeHasNext<T> cachedPerv = perv as LinkNodeHasNext<T>;
                if (cachedPerv != null)
                {
                    cachedPerv.Next = this;
                }
                cachedPerv = Perv as LinkNodeHasNext<T>;
                if (cachedPerv != null)
                {
                    cachedPerv.Next = perv;
                }
                Perv = perv;
                return true;

            }
            return false;
        }
        public IEnumerator<T> GetEnumerator()
        {
            LinkNodeLinkNext<T>.Enumerator result = new LinkNodeLinkNext<T>.Enumerator(this);
            return result;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private LinkNode<T> next = null;

        private LinkNode<T> perv = null;

        private T data;
    }
}
