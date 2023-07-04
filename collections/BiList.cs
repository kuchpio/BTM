using System;
using System.Collections;
using System.Collections.Generic;

namespace BTM
{
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

            public BiListNode Next { get => next; set => next = value; }
            public BiListNode Prev { get => prev; set => prev = value; }
            public BTMBase Value => value;
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

        public IIterator<BTMBase> GetForwardIterator(int startOffset = 0) =>
            new BiListIterator(this, false, startOffset);

        public IIterator<BTMBase> GetBackwardIterator(int startOffset = 0) =>
            new BiListIterator(this, true, startOffset);

        public IIterator<BTMBase> First() => new BiListIterator(this);
        public IIterator<BTMBase> Last() => new BiListIterator(this, true);
        public IIterator<BTMBase> GetEnumerator() => First();
        IEnumerator<BTMBase> IEnumerable<BTMBase>.GetEnumerator() => First();
        IEnumerator IEnumerable.GetEnumerator() => First();

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
}
