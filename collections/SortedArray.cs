using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BTM
{
    class SortedArray<BTMBase> : IBTMCollection<BTMBase> where BTMBase : IBTMBase
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

        public IIterator<BTMBase> GetForwardIterator(int startOffset = 0) =>
            new SortedArrayIterator(this, false, startOffset);

        public IIterator<BTMBase> GetBackwardIterator(int startOffset = 0) =>
            new SortedArrayIterator(this, true, startOffset);

        public IIterator<BTMBase> First() => new SortedArrayIterator(this);
        public IIterator<BTMBase> Last() => new SortedArrayIterator(this, true);
        public IIterator<BTMBase> GetEnumerator() => First();
        IEnumerator<BTMBase> IEnumerable<BTMBase>.GetEnumerator() => First();
        IEnumerator IEnumerable.GetEnumerator() => First();

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
}
