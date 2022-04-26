using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RW_NodeTree.Tools
{
    public interface ILinkNode<T>
    {
        T Data { get; set; }
        void DropNode();
    }
    public interface ILinkNodeHasNext<T> : ILinkNode<T>, IEnumerable<T>
    {
        ILinkNode<T> Next { get; set; }
        bool InsertNext(ILinkNode<T> next);
    }
    public interface ILinkNodeHasPerv<T> : ILinkNode<T>, IEnumerable<T>
    {
        ILinkNode<T> Perv { get; set; }
        bool InsertPerv(ILinkNode<T> next);
    }

    public class LinkNodeLinkNext<T> : ILinkNodeHasNext<T>
    {
        public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
        {
            public Enumerator(ILinkNodeHasNext<T> head) : this()
            {
                _head = head;
            }

            private ILinkNodeHasNext<T> _head;

            private ILinkNodeHasNext<T> _currentElement;

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
                _currentElement = _currentElement?.Next as ILinkNodeHasNext<T> ?? _head;
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

        public ILinkNode<T> Next 
        { 
            get => next; 
            set
            {
                ILinkNodeHasPerv<T> cachedNext = next as ILinkNodeHasPerv<T>;
                next = value;
                if (cachedNext != null)
                {
                    cachedNext.Perv = null;
                }
                cachedNext = value as ILinkNodeHasPerv<T>;
                if (cachedNext != null)
                {
                    ILinkNodeHasNext<T> cachedPerv = cachedNext.Perv as ILinkNodeHasNext<T>;
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
            ILinkNodeHasPerv<T> next = Next as ILinkNodeHasPerv<T>;
            if (next != null)
            {
                next.Perv = null;
            }
            Next = null;
        }

        public bool InsertNext(ILinkNode<T> next)
        {
            if(next != null)
            {
                next.DropNode();
                ILinkNodeHasNext<T> cachedPerv = next as ILinkNodeHasNext<T>;
                if (cachedPerv != null)
                {
                    cachedPerv.Next = Next;
                }
                ILinkNodeHasPerv<T> cachedNext = next as ILinkNodeHasPerv<T>;
                if (cachedNext != null)
                {
                    cachedNext.Perv = this;
                }
                cachedNext = Next as ILinkNodeHasPerv<T>;
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

        private ILinkNode<T> next = null;

        private T data;
    }
    public class LinkNodeLinkPerv<T> : ILinkNodeHasPerv<T>
    {
        public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
        {
            public Enumerator(ILinkNodeHasPerv<T> head) : this()
            {
                _head = head;
            }

            private ILinkNodeHasPerv<T> _head;

            private ILinkNodeHasPerv<T> _currentElement;

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
                _currentElement = _currentElement?.Perv as ILinkNodeHasPerv<T> ?? _head;
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

        public ILinkNode<T> Perv
        {
            get => perv;
            set
            {
                ILinkNodeHasNext<T> cachedPerv = perv as ILinkNodeHasNext<T>;
                perv = value;
                if (cachedPerv != null)
                {
                    cachedPerv.Next = null;
                }
                cachedPerv = value as ILinkNodeHasNext<T>;
                if (cachedPerv != null)
                {
                    ILinkNodeHasPerv<T> cachedNext = cachedPerv.Next as ILinkNodeHasPerv<T>;
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
            ILinkNodeHasNext<T> perv = Perv as ILinkNodeHasNext<T>;
            if (perv != null)
            {
                perv.Next = null;
            }
            Perv = null;
        }

        public bool InsertPerv(ILinkNode<T> perv)
        {
            if (perv != null)
            {
                perv.DropNode();
                ILinkNodeHasPerv<T> cachedNext = perv as ILinkNodeHasPerv<T>;
                if (cachedNext != null)
                {
                    cachedNext.Perv = Perv;
                }
                ILinkNodeHasNext<T> cachedPerv = perv as ILinkNodeHasNext<T>;
                if (cachedPerv != null)
                {
                    cachedPerv.Next = this;
                }
                cachedPerv = Perv as ILinkNodeHasNext<T>;
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

        private ILinkNode<T> perv = null;

        private T data;
    }
    public class LinkNodeLinkNextAndPerv<T> : ILinkNodeHasNext<T> , ILinkNodeHasPerv<T>
    {
        public LinkNodeLinkNextAndPerv(T data)
        {
            Data = data;
        }

        public ILinkNode<T> Next
        {
            get => next;
            set
            {
                ILinkNodeHasPerv<T> cachedNext = next as ILinkNodeHasPerv<T>;
                next = value;
                if (cachedNext != null)
                {
                    cachedNext.Perv = null;
                }
                cachedNext = value as ILinkNodeHasPerv<T>;
                if (cachedNext != null)
                {
                    ILinkNodeHasNext<T> cachedPerv = cachedNext.Perv as ILinkNodeHasNext<T>;
                    if (cachedPerv != null)
                    {
                        cachedPerv.Next = null;
                    }
                    cachedNext.Perv = this;
                }
            }
        }
        public ILinkNode<T> Perv
        {
            get => perv;
            set
            {
                ILinkNodeHasNext<T> cachedPerv = perv as ILinkNodeHasNext<T>;
                perv = value;
                if (cachedPerv != null)
                {
                    cachedPerv.Next = null;
                }
                cachedPerv = value as ILinkNodeHasNext<T>;
                if (cachedPerv != null)
                {
                    ILinkNodeHasPerv<T> cachedNext = cachedPerv.Next as ILinkNodeHasPerv<T>;
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
            ILinkNodeHasPerv<T> next = Next as ILinkNodeHasPerv<T>;
            if (next != null)
            {
                next.Perv = Perv;
            }
            ILinkNodeHasNext<T> perv = Perv as ILinkNodeHasNext<T>;
            if (perv != null)
            {
                perv.Next = Next;
            }
            Next = null;
            Perv = null;
        }

        public bool InsertNext(ILinkNode<T> next)
        {
            if (next != null)
            {
                next.DropNode();
                ILinkNodeHasNext<T> cachedPerv = next as ILinkNodeHasNext<T>;
                if (cachedPerv != null)
                {
                    cachedPerv.Next = Next;
                }
                ILinkNodeHasPerv<T> cachedNext = next as ILinkNodeHasPerv<T>;
                if (cachedNext != null)
                {
                    cachedNext.Perv = this;
                }
                cachedNext = Next as ILinkNodeHasPerv<T>;
                if (cachedNext != null)
                {
                    cachedNext.Perv = next;
                }
                Next = next;
                return true;

            }
            return false;
        }


        public bool InsertPerv(ILinkNode<T> perv)
        {
            if (perv != null)
            {
                perv.DropNode();
                ILinkNodeHasPerv<T> cachedNext = perv as ILinkNodeHasPerv<T>;
                if (cachedNext != null)
                {
                    cachedNext.Perv = Perv;
                }
                ILinkNodeHasNext<T> cachedPerv = perv as ILinkNodeHasNext<T>;
                if (cachedPerv != null)
                {
                    cachedPerv.Next = this;
                }
                cachedPerv = Perv as ILinkNodeHasNext<T>;
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

        private ILinkNode<T> next = null;

        private ILinkNode<T> perv = null;

        private T data;
    }
}
