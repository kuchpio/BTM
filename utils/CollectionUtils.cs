using System.Collections.Generic;

namespace BTM
{
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
            while (iterator.MoveNext()) action.Eval(iterator.Current);
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
            ForEach(iterator, new AddToShortStringList<BTMBase>(shortStrings));
            return string.Join(sep, shortStrings);
        }

        private class AddToShortStringList<BTMBase> : IAction<BTMBase> where BTMBase : IBTMBase
        {
            List<string> shortStrings = new List<string>();

            public AddToShortStringList(List<string> shortStrings)
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
