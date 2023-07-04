using System;
using System.Collections;
using System.Collections.Generic;

namespace BTM
{
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

        public IIterator<BTMBase> GetForwardIterator(int startOffset = 0) =>
            new VectorIterator(this, false, startOffset);

        public IIterator<BTMBase> GetBackwardIterator(int startOffset = 0) =>
            new VectorIterator(this, true, startOffset);

        public IIterator<BTMBase> First() => new VectorIterator(this);
        public IIterator<BTMBase> Last() => new VectorIterator(this, true);
        public IIterator<BTMBase> GetEnumerator() => First();
        IEnumerator<BTMBase> IEnumerable<BTMBase>.GetEnumerator() => First();
        IEnumerator IEnumerable.GetEnumerator() => First();

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
}
