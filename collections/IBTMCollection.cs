using System.Collections.Generic;

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
}
