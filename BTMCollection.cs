using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTM
{
    interface IBTMCollection<BTMBase> : IEnumerable<BTMBase> where BTMBase : IBTMBase
    {
        void Add(BTMBase btmObject);
        void Add(int index, BTMBase btmObject);
        void Add(IIterator<BTMBase> iter);
        int Remove(BTMBase btmObject);
        void RemoveLast();
        void Clear();
        IIterator<BTMBase> First();
        IIterator<BTMBase> Last();
        IIterator<BTMBase> GetForwardIterator(int startOffset = 0);
        IIterator<BTMBase> GetBackwardIterator(int startOffset = 0);
        new IIterator<BTMBase> GetEnumerator();
    }

    interface IIterator<BTMBase> : IEnumerator<BTMBase> where BTMBase : IBTMBase
    {
        new void Reset();
        new bool MoveNext();
        new BTMBase Current { get; }
    }

    class BiList<BTMBase> : IBTMCollection<BTMBase> where BTMBase : IBTMBase
    {
        private class BiListNode
        {
            private BTMBase value;
            private BiListNode next, prev;

            public BiListNode(BTMBase value, BiListNode next = null, BiListNode prev = null)
            {
                this.value = value;
                this.next = next;
                this.prev = prev;
            }

            public BiListNode Next
            {
                get { return next; }
                set { next = value; }
            }

            public BiListNode Prev
            {
                get { return prev; }
                set { prev = value; }
            }

            public BTMBase Value
            {
                get { return value; }
            }
        }

        private BiListNode head, tail;

        public BiList()
        {
            head = tail = null;
        }

        public void Add(BTMBase btmObject)
        {
            AddBack(btmObject);
        }

        private void AddBack(BTMBase btmObject)
        {
            if (head == null)
            {
                tail = head = new BiListNode(btmObject);
            }
            else
            {
                tail.Next = new BiListNode(btmObject, null, tail);
                tail = tail.Next;
            }
        }

        public void Add(int index, BTMBase btmObject)
        {
            BiListNode currentNode = head;
            for (int i = 0; i < index; i++)
            {
                if (currentNode == null) return;
                currentNode = currentNode.Next;
            }
            if (currentNode == null) AddBack(btmObject);
            BiListNode newNode = new BiListNode(btmObject, currentNode, currentNode.Prev);
            currentNode.Prev = newNode;
            if (newNode.Prev == null)
            {
                head = newNode;
            }
            else
            {
                newNode.Prev.Next = newNode;
            }
        }

        public void Add(IIterator<BTMBase> iterator)
        {
            while (iterator.MoveNext()) Add(iterator.Current);
        }

        public int Remove(BTMBase btmObject)
        {
            int index = 0;
            for (BiListNode currentNode = head; currentNode != null; currentNode = currentNode.Next)
            {
                if (Object.ReferenceEquals(btmObject, currentNode.Value))
                {
                    if (currentNode.Prev != null)
                        currentNode.Prev.Next = currentNode.Next;

                    if (currentNode.Next != null)
                        currentNode.Next.Prev = currentNode.Prev;

                    return index;
                }
                index++;
            }
            return -1;
        }

        public void RemoveLast()
        {
            if (head == null) return;

            tail = tail.Prev;
            if (tail != null) tail.Next = null;
        }

        public void Clear()
        {
            head = tail = null;
        }

        public IIterator<BTMBase> GetForwardIterator(int startOffset = 0)
        {
            return new BiListIterator(this, false, startOffset);
        }

        public IIterator<BTMBase> GetBackwardIterator(int startOffset = 0)
        {
            return new BiListIterator(this, true, startOffset);
        }

        public IIterator<BTMBase> First()
        {
            return new BiListIterator(this);
        }

        public IIterator<BTMBase> Last()
        {
            return new BiListIterator(this, true);
        }

        public IIterator<BTMBase> GetEnumerator()
        {
            return First();
        }

        IEnumerator<BTMBase> IEnumerable<BTMBase>.GetEnumerator()
        {
            return First();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return First();
        }

        public void RemoveIfFirst(IPredicate<BTMBase> predicate)
        {
            for (BiListNode currentNode = head; currentNode != null; currentNode = currentNode.Next)
            {
                if (predicate.Eval(currentNode.Value))
                {
                    if (currentNode.Prev != null)
                        currentNode.Prev.Next = currentNode.Next;

                    if (currentNode.Next != null)
                        currentNode.Next.Prev = currentNode.Prev;

                    return;
                }
            }
        }

        private class BiListIterator : IIterator<BTMBase>
        {
            private BiListNode currentNode, startNode;
            private bool reverse, started;

            public BiListIterator(BiList<BTMBase> biList, bool reverse = false, int startOffset = 0)
            {
                startNode = reverse ? biList.tail : biList.head;
                currentNode = null;
                this.reverse = reverse;
                started = false;

                // Apply offset
                for (int i = 0; i < startOffset; i++) MoveNext();

                if (currentNode != null)
                {
                    startNode = reverse ? currentNode.Prev : currentNode.Next;
                    started = false;
                }
            }

            public BTMBase Current => currentNode.Value;

            object IEnumerator.Current => currentNode.Value;

            public void Dispose()
            { }

            public bool MoveNext()
            {
                if (!started)
                {
                    currentNode = startNode;
                    started = true;
                }
                else if (currentNode != null)
                {
                    currentNode = reverse ? currentNode.Prev : currentNode.Next;
                }
                
                return currentNode != null;
            }

            public void Reset()
            {
                currentNode = null;
                started = false;
            }
        }
    }

    class Vector<BTMBase> : IBTMCollection<BTMBase> where BTMBase : IBTMBase
    {
        private BTMBase[] data;
        private int size;
        private int capacity;

        public Vector() : this(8)
        { }

        public Vector(int capacity)
        {
            data = new BTMBase[capacity];
            size = 0;
            this.capacity = capacity;
        }

        private void Resize(int newCapacity)
        {
            BTMBase[] newDataSpace = new BTMBase[newCapacity];

            for (int i = 0; i < size; i++)
            {
                newDataSpace[i] = data[i];
            }

            data = newDataSpace;
            capacity = newCapacity;
        }

        public void Add(BTMBase btmObject)
        {
            AddBack(btmObject);
        }

        public void Add(int index, BTMBase btmObject)
        {
            AddBack(data[size - 1]);
            for (int i = size - 1; i > index; i--)
            {
                data[i] = data[i - 1];
            }
            data[index] = btmObject;
        }

        private void AddBack(BTMBase btmObject)
        {
            if (size == capacity) Resize(2 * capacity);

            data[size++] = btmObject;
        }

        public void Add(IIterator<BTMBase> iterator)
        {
            while (iterator.MoveNext()) Add(iterator.Current);
        }

        public int Remove(BTMBase btmObject)
        {
            int index = -1;
            for (int i = 0; i < size; i++)
            {
                if (index >= 0)
                {
                    data[i - 1] = data[i];
                }
                else
                {
                    if (Object.ReferenceEquals(btmObject, data[i])) index = i;
                }    
            }
            if (index >= 0) size--;
            return index;
        }

        public void RemoveLast()
        {
            if (size > 0) size--;
        }

        public void Clear()
        {
            size = 0;
        }

        public IIterator<BTMBase> First()
        {
            return new VectorIterator(this);
        }

        public IIterator<BTMBase> Last()
        {
            return new VectorIterator(this, true);
        }

        public IIterator<BTMBase> GetForwardIterator(int startOffset)
        {
            return new VectorIterator(this, false, startOffset);
        }

        public IIterator<BTMBase> GetBackwardIterator(int startOffset)
        {
            return new VectorIterator(this, true, startOffset);
        }

        public IIterator<BTMBase> GetEnumerator()
        {
            return First();
        }

        IEnumerator<BTMBase> IEnumerable<BTMBase>.GetEnumerator()
        {
            return First();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return First();
        }

        private class VectorIterator : IIterator<BTMBase>
        {
            private int currentIndex, startIndex, size;
            private BTMBase[] data;
            private bool reverse;

            public VectorIterator(Vector<BTMBase> vector, bool reverse = false, int startOffset = 0)
            {
                size = vector.size;
                startIndex = reverse ? size - 1 - startOffset : startOffset;
                currentIndex = reverse ? startIndex + 1 : startIndex - 1;
                data = vector.data;
                this.reverse = reverse;
            }

            public BTMBase Current => data[currentIndex];

            object IEnumerator.Current => data[currentIndex];

            public void Dispose()
            { }

            public bool MoveNext()
            {
                if (reverse)
                {
                    currentIndex--;

                    return currentIndex >= 0;
                }
                else
                {
                    currentIndex++;

                    return currentIndex < size;
                }
            }

            public void Reset()
            {
                currentIndex = reverse ? startIndex + 1 : startIndex - 1;
            }
        }
    }

    class SortedArray<BTMBase> : IBTMCollection<BTMBase> where BTMBase: IBTMBase
    {
        private List<BTMBase> array;
        IComparer<BTMBase> comparerFunction; // #1 - #2

        public SortedArray(IComparer<BTMBase> comparerFunction)
        {
            array = new List<BTMBase>();
            this.comparerFunction = comparerFunction;
        }

        private int FindIndexOf(BTMBase item)
        {
            int a = 0, b = array.Count - 1;

            while (b > a)
            {
                int c = (a + b) / 2;
                if (comparerFunction.Eval(array[c], item) < 0)
                {
                    a = c + 1;
                }
                else
                {
                    b = c;
                }
            }

            return b;
        }

        public void Add(BTMBase item)
        {
            if (array.Count == 0)
            {
                array.Add(item);
                return;
            }

            int i = FindIndexOf(item);
            if (i == array.Count - 1 && comparerFunction.Eval(array.Last(), item) < 0)
            {
                array.Add(item);
                return;
            }

            array.Add(array.Last());
            for (int j = array.Count - 2; j > i; j--)
            {
                array[j] = array[j - 1];
            }

            array[i] = item;
        }

        public void Add(int index, BTMBase btmObject)
        {
            if ((index > 0 && comparerFunction.Eval(array[index - 1], btmObject) > 0) || comparerFunction.Eval(array[index], btmObject) < 0)
                return;

            array.Add(array.Last());
            for (int j = array.Count - 2; j > index; j--)
            {
                array[j] = array[j - 1];
            }

            array[index] = btmObject;
        }

        public void Add(IIterator<BTMBase> iterator)
        {
            while (iterator.MoveNext()) Add(iterator.Current);
        }

        public int Remove(BTMBase item)
        {
            if (array.Count == 0) return -1;
            
            int i = FindIndexOf(item);
            if (comparerFunction.Eval(array[i], item) != 0)
                return -1;

            for (int j = i; j <= array.Count - 2; j++)
            {
                array[j] = array[j + 1];
            }

            array.RemoveAt(array.Count - 1);
            return i;
        }

        public void RemoveLast()
        {
            array.RemoveAt(array.Count - 1);
        }

        public void Clear()
        {
            array.Clear();
        }

        public IIterator<BTMBase> Find(BTMBase item)
        {
            int i = FindIndexOf(item);
            if (array.Count == 0 || comparerFunction.Eval(array[i], item) != 0)
                return null;

            return GetForwardIterator(i);
        }

        public IIterator<BTMBase> First()
        {
            return new SortedArrayIterator(this);
        }

        public IIterator<BTMBase> Last()
        {
            return new SortedArrayIterator(this, true);
        }

        public IIterator<BTMBase> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public IIterator<BTMBase> GetForwardIterator(int startOffset)
        {
            return new SortedArrayIterator(this, false, startOffset);
        }

        public IIterator<BTMBase> GetBackwardIterator(int startOffset)
        {
            return new SortedArrayIterator(this, true, startOffset);
        }

        IEnumerator<BTMBase> IEnumerable<BTMBase>.GetEnumerator()
        {
            return First();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return First();
        }

        private class SortedArrayIterator : IIterator<BTMBase>
        {
            private int currentIndex, startIndex;
            private List<BTMBase> array;
            private bool reverse;

            public SortedArrayIterator(SortedArray<BTMBase> sortedArray, bool reverse = false, int startOffset = 0)
            {
                array = sortedArray.array;
                startIndex = reverse ? array.Count - 1 - startOffset : startOffset;
                currentIndex = reverse ? startIndex + 1 : startIndex - 1;
                this.reverse = reverse;
            }

            public BTMBase Current => array[currentIndex];

            object IEnumerator.Current => array[currentIndex];

            public void Dispose()
            { }

            public bool MoveNext()
            {
                if (reverse)
                {
                    currentIndex--;

                    return currentIndex >= 0;
                }
                else
                {
                    currentIndex++;

                    return currentIndex < array.Count;
                }
            }

            public void Reset()
            {
                currentIndex = reverse ? startIndex + 1 : startIndex - 1;
            }
        }
    }

    static class CollectionUtils
    {
        public static BTMBase Find<BTMBase>(IIterator<BTMBase> iterator, IPredicate<BTMBase> predicate) where BTMBase : class, IBTMBase
        {
            while (iterator.MoveNext())
            {
                if (predicate.Eval(iterator.Current))
                    return iterator.Current;
            }

            return null;
        }

        public static void ForEach<BTMBase>(IIterator<BTMBase> iterator, IAction<BTMBase> action) where BTMBase : IBTMBase
        {
            while (iterator.MoveNext())
            {
                action.Eval(iterator.Current);
            }
        }

        public static int CountIf<BTMBase>(IIterator<BTMBase> iterator, IPredicate<BTMBase> predicate) where BTMBase : IBTMBase
        {
            int counter = 0;

            while (iterator.MoveNext())
            {
                if (predicate.Eval(iterator.Current))
                    counter++;
            }

            return counter;
        }

        public static string ToString<BTMBase>(IIterator<BTMBase> iterator, string sep = ", ") where BTMBase : IBTMBase
        {
            List<string> shortStrings = new List<string>();
            ForEach(iterator, new StringConcat<BTMBase>(shortStrings));
            return string.Join(sep, shortStrings);
        }

        private class StringConcat<BTMBase> : IAction<BTMBase> where BTMBase : IBTMBase
        {
            List<string> shortStrings = new List<string>();

            public StringConcat(List<string> shortStrings)
            {
                this.shortStrings = shortStrings;
            }

            public void Eval(BTMBase item)
            {
                shortStrings.Add(item.ToShortString());
            }
        }
    }
}
