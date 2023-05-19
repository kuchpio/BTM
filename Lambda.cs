
namespace BTM
{
    interface IPredicate<BTMBase> where BTMBase : IBTMBase
    {
        bool Eval(BTMBase item);
    }

    interface IComparer<BTMBase> where BTMBase : IBTMBase
    {
        int Eval(BTMBase item1, BTMBase item2);
    }

    interface IAction<BTMBase> where BTMBase : IBTMBase
    {
        void Eval(BTMBase item);
    }

    class True<BTMBase> : IPredicate<BTMBase> where BTMBase : IBTMBase
    {
        public bool Eval(BTMBase item)
        {
            return true;
        }
    }

    class False<BTMBase> : IPredicate<BTMBase> where BTMBase : IBTMBase
    {
        public bool Eval(BTMBase item)
        {
            return false;
        }
    }

    class All<BTMBase> : IPredicate<BTMBase> where BTMBase : IBTMBase
    {
        private List<IPredicate<BTMBase>> predicates;

        public All()
        {
            predicates = new List<IPredicate<BTMBase>>();
        }
        
        public All(List<IPredicate<BTMBase>> predicates)
        {
            this.predicates = predicates;
        }

        public void Add(IPredicate<BTMBase> predicate)
        {
            predicates.Add(predicate);
        }

        public void Clear()
        {
            predicates.Clear();
        }
        
        public bool Eval(BTMBase item)
        {
            foreach (IPredicate<BTMBase> predicate in predicates)
                if (!predicate.Eval(item)) return false;

            return true;
        }
    }

    class Any<BTMBase> : IPredicate<BTMBase> where BTMBase : IBTMBase
    {
        private List<IPredicate<BTMBase>> predicates;

        public Any()
        {
            predicates = new List<IPredicate<BTMBase>>();
        }
        
        public Any(List<IPredicate<BTMBase>> predicates)
        {
            this.predicates = predicates;
        }

        public void Add(IPredicate<BTMBase> predicate)
        {
            predicates.Add(predicate);
        }

        public void Clear()
        {
            predicates.Clear();
        }

        public bool Eval(BTMBase item)
        {
            foreach (IPredicate<BTMBase> predicate in predicates)
                if (predicate.Eval(item)) return true;

            return false;
        }
    }

    class ActionSequence<BTMBase> : IAction<BTMBase> where BTMBase : IBTMBase
    {
        private List<IAction<BTMBase>> actions;

        public ActionSequence()
        {
            actions = new List<IAction<BTMBase>>();
        }

        public ActionSequence(List<IAction<BTMBase>> actions)
        {
            this.actions = actions;
        }

        public void Add(IAction<BTMBase> action)
        {
            actions.Add(action);
        }

        public void Clear()
        {
            actions.Clear();
        }

        public void Eval(BTMBase item)
        {
            foreach(IAction<BTMBase> action in actions)
                action.Eval(item);
        }
    }

    class ActionIf<BTMBase> : IAction<BTMBase> where BTMBase : IBTMBase
    {
        private IAction<BTMBase> action;
        private IPredicate<BTMBase> predicate;

        public ActionIf(IAction<BTMBase> action, IPredicate<BTMBase> predicate)
        {
            this.action = action;
            this.predicate = predicate;
        }

        public void Eval(BTMBase item)
        {
            if (!predicate.Eval(item)) return;

            action.Eval(item);
        }
    }

    class Print<BTMBase> : IAction<BTMBase> where BTMBase : IBTMBase
    {
        public void Eval(BTMBase item)
        {
            Console.WriteLine(item);
        }
    }
}