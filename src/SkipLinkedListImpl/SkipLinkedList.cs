namespace SkipLinkedListImpl
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    public class SkipLinkedList<T> : ICollection<T>, System.Collections.ICollection, IReadOnlyCollection<T>
    {
        // This LinkedList is a doubly-Linked circular list.
        internal SkipLinkedListNode<T> head;
        internal int count;
        internal int version;
        private Object _syncRoot;

        public SkipLinkedList()
        {
        }

        public SkipLinkedList(IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }

            foreach (T item in collection)
            {
                AddLast(item);
            }
        }

        public int Count
        {
            get { return count; }
        }

        public SkipLinkedListNode<T> First
        {
            get { return head; }
        }

        public SkipLinkedListNode<T> Last
        {
            get { return head == null ? null : head.prev; }
        }

        bool ICollection<T>.IsReadOnly
        {
            get { return false; }
        }

        void ICollection<T>.Add(T value)
        {
            AddLast(value);
        }

        public SkipLinkedListNode<T> AddAfter(SkipLinkedListNode<T> node, T value)
        {
            ValidateNode(node);
            SkipLinkedListNode<T> result = new SkipLinkedListNode<T>(node.list, value);
            InternalInsertNodeBefore(node.next, result);
            return result;
        }

        public void AddAfter(SkipLinkedListNode<T> node, SkipLinkedListNode<T> newNode)
        {
            ValidateNode(node);
            ValidateNewNode(newNode);
            InternalInsertNodeBefore(node.next, newNode);
            newNode.list = this;
        }

        public SkipLinkedListNode<T> AddBefore(SkipLinkedListNode<T> node, T value)
        {
            ValidateNode(node);
            SkipLinkedListNode<T> result = new SkipLinkedListNode<T>(node.list, value);
            InternalInsertNodeBefore(node, result);
            if (node == head)
            {
                head = result;
            }
            return result;
        }

        public void AddBefore(SkipLinkedListNode<T> node, SkipLinkedListNode<T> newNode)
        {
            ValidateNode(node);
            ValidateNewNode(newNode);
            InternalInsertNodeBefore(node, newNode);
            newNode.list = this;
            if (node == head)
            {
                head = newNode;
            }
        }

        public SkipLinkedListNode<T> AddFirst(T value)
        {
            SkipLinkedListNode<T> result = new SkipLinkedListNode<T>(this, value);
            if (head == null)
            {
                InternalInsertNodeToEmptyList(result);
            }
            else
            {
                InternalInsertNodeBefore(head, result);
                head = result;
            }
            return result;
        }

        public void AddFirst(SkipLinkedListNode<T> node)
        {
            ValidateNewNode(node);

            if (head == null)
            {
                InternalInsertNodeToEmptyList(node);
            }
            else
            {
                InternalInsertNodeBefore(head, node);
                head = node;
            }
            node.list = this;
        }

        public SkipLinkedListNode<T> AddLast(T value)
        {
            SkipLinkedListNode<T> result = new SkipLinkedListNode<T>(this, value);
            if (head == null)
            {
                InternalInsertNodeToEmptyList(result);
            }
            else
            {
                InternalInsertNodeBefore(head, result);
            }
            return result;
        }

        public void AddLast(SkipLinkedListNode<T> node)
        {
            ValidateNewNode(node);

            if (head == null)
            {
                InternalInsertNodeToEmptyList(node);
            }
            else
            {
                InternalInsertNodeBefore(head, node);
            }
            node.list = this;
        }

        public void Clear()
        {
            SkipLinkedListNode<T> current = head;
            while (current != null)
            {
                SkipLinkedListNode<T> temp = current;
                current = current.Next;   // use Next the instead of "next", otherwise it will loop forever
                temp.Invalidate();
            }

            head = null;
            count = 0;
            version++;
        }

        public void CopyTo(T[] array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            if (index < 0 || index > array.Length)
            {
                throw new ArgumentOutOfRangeException("index", $"IndexOutOfRange. Index = {index}");
            }

            if (array.Length - index < Count)
            {
                throw new ArgumentException("Arg_InsufficientSpace");
            }

            SkipLinkedListNode<T> node = head;
            if (node != null)
            {
                do
                {
                    array[index++] = node.item;
                    node = node.next;
                } while (node != head);
            }
        }

        public bool Contains(T value)
        {
            return Find(value) != null;
        }

        public SkipLinkedListNode<T> Find(T value)
        {
            SkipLinkedListNode<T> node = head;
            EqualityComparer<T> c = EqualityComparer<T>.Default;
            if (node != null)
            {
                if (value != null)
                {
                    do
                    {
                        if (c.Equals(node.item, value))
                        {
                            return node;
                        }
                        node = node.next;
                    } while (node != head);
                }
                else
                {
                    do
                    {
                        if (node.item == null)
                        {
                            return node;
                        }
                        node = node.next;
                    } while (node != head);
                }
            }
            return null;
        }

        public SkipLinkedListNode<T> FindLast(T value)
        {
            if (head == null) return null;

            SkipLinkedListNode<T> last = head.prev;
            SkipLinkedListNode<T> node = last;
            EqualityComparer<T> c = EqualityComparer<T>.Default;
            if (node != null)
            {
                if (value != null)
                {
                    do
                    {
                        if (c.Equals(node.item, value))
                        {
                            return node;
                        }

                        node = node.prev;
                    } while (node != last);
                }
                else
                {
                    do
                    {
                        if (node.item == null)
                        {
                            return node;
                        }
                        node = node.prev;
                    } while (node != last);
                }
            }
            return null;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Remove(T value)
        {
            SkipLinkedListNode<T> node = Find(value);
            if (node != null)
            {
                InternalRemoveNode(node);
                return true;
            }
            return false;
        }

        public void Remove(SkipLinkedListNode<T> node)
        {
            ValidateNode(node);
            InternalRemoveNode(node);
        }

        public void RemoveFirst()
        {
            if (head == null) { throw new InvalidOperationException("SkipLinkedListIsEmpty"); }
            InternalRemoveNode(head);
        }

        public void RemoveLast()
        {
            if (head == null) { throw new InvalidOperationException("SkipLinkedListIsEmpty"); }
            InternalRemoveNode(head.prev);
        }

        private void InternalInsertNodeBefore(SkipLinkedListNode<T> node, SkipLinkedListNode<T> newNode)
        {
            newNode.next = node;
            newNode.prev = node.prev;
            node.prev.next = newNode;
            node.prev = newNode;
            version++;
            count++;
        }

        private void InternalInsertNodeToEmptyList(SkipLinkedListNode<T> newNode)
        {
            Debug.Assert(head == null && count == 0, "LinkedList must be empty when this method is called!");
            newNode.next = newNode;
            newNode.prev = newNode;
            head = newNode;
            version++;
            count++;
        }

        internal void InternalRemoveNode(SkipLinkedListNode<T> node)
        {
            Debug.Assert(node.list == this, "Deleting the node from another list!");
            Debug.Assert(head != null, "This method shouldn't be called on empty list!");
            if (node.next == node)
            {
                Debug.Assert(count == 1 && head == node, "this should only be true for a list with only one node");
                head = null;
            }
            else
            {
                node.next.prev = node.prev;
                node.prev.next = node.next;
                if (head == node)
                {
                    head = node.next;
                }
            }
            node.Invalidate();
            count--;
            version++;
        }

        internal void ValidateNewNode(SkipLinkedListNode<T> node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            if (node.list != null)
            {
                throw new InvalidOperationException("SkipLinkedListNodeIsAttached");
            }
        }

        internal void ValidateNode(SkipLinkedListNode<T> node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            if (node.list != this)
            {
                throw new InvalidOperationException("ExternalSkipLinkedListNode");
            }
        }

        bool System.Collections.ICollection.IsSynchronized
        {
            get { return false; }
        }

        object System.Collections.ICollection.SyncRoot
        {
            get
            {
                if (_syncRoot == null)
                {
                    System.Threading.Interlocked.CompareExchange<Object>(ref _syncRoot, new Object(), null);
                }
                return _syncRoot;
            }
        }

        void System.Collections.ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            if (array.Rank != 1)
            {
                throw new ArgumentException("Arg_MultiRank");
            }

            if (array.GetLowerBound(0) != 0)
            {
                throw new ArgumentException("Arg_NonZeroLowerBound");
            }

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index", $"IndexOutOfRange. Index = {index}");
            }

            if (array.Length - index < Count)
            {
                throw new ArgumentException("Arg_InsufficientSpace");
            }

            T[] tArray = array as T[];
            if (tArray != null)
            {
                CopyTo(tArray, index);
            }
            else
            {
                //
                // Catch the obvious case assignment will fail.
                // We can found all possible problems by doing the check though.
                // For example, if the element type of the Array is derived from T,
                // we can't figure out if we can successfully copy the element beforehand.
                //
                Type targetType = array.GetType().GetElementType();
                Type sourceType = typeof(T);
                if (!(targetType.IsAssignableFrom(sourceType) || sourceType.IsAssignableFrom(targetType)))
                {
                    throw new ArgumentException("Invalid_Array_Type");
                }

                object[] objects = array as object[];
                if (objects == null)
                {
                    throw new ArgumentException("Invalid_Array_Type");
                }
                SkipLinkedListNode<T> node = head;
                try
                {
                    if (node != null)
                    {
                        do
                        {
                            objects[index++] = node.item;
                            node = node.next;
                        } while (node != head);
                    }
                }
                catch (ArrayTypeMismatchException)
                {
                    throw new ArgumentException("Invalid_Array_Type");
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public struct Enumerator : IEnumerator<T>, System.Collections.IEnumerator
        {
            private SkipLinkedList<T> list;
            private SkipLinkedListNode<T> node;
            private int version;
            private T current;
            private int index;

            internal Enumerator(SkipLinkedList<T> list)
            {
                this.list = list;
                version = list.version;
                node = list.head;
                current = default(T);
                index = 0;
            }

            public T Current
            {
                get { return current; }
            }

            object System.Collections.IEnumerator.Current
            {
                get
                {
                    if (index == 0 || (index == list.Count + 1))
                    {
                        throw new InvalidOperationException("InvalidOperation_EnumOpCantHappen");
                    }

                    return current;
                }
            }

            public bool MoveNext()
            {
                if (node == null)
                {
                    index = list.Count + 1;
                    return false;
                }

                ++index;
                current = node.item;
                node = node.next;
                if (node == list.head)
                {
                    node = null;
                }
                return true;
            }

            void System.Collections.IEnumerator.Reset()
            {
                current = default(T);
                node = list.head;
                index = 0;
            }

            public void Dispose()
            {
            }
        }

    }

    public sealed class SkipLinkedListNode<T>
    {
        internal SkipLinkedList<T> list;
        internal SkipLinkedListNode<T> next;
        internal SkipLinkedListNode<T> prev;
        internal T item;

        public SkipLinkedListNode(T value)
        {
            this.item = value;
        }

        internal SkipLinkedListNode(SkipLinkedList<T> list, T value)
        {
            this.list = list;
            this.item = value;
        }

        public SkipLinkedList<T> List
        {
            get { return list; }
        }

        public SkipLinkedListNode<T> Next
        {
            get { return next == null || next == list.head ? null : next; }
        }

        public SkipLinkedListNode<T> Previous
        {
            get { return prev == null || this == list.head ? null : prev; }
        }

        public T Value
        {
            get { return item; }
            set { item = value; }
        }

        internal void Invalidate()
        {
            list = null;
            next = null;
            prev = null;
        }
    }
}
