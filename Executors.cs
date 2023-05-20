namespace BTM
{
    interface ICommandExecutor
    {
        void Execute();
    }

    class EmptyExecutor : ICommandExecutor
    {
        public void Execute()
        { }
    }

    abstract class ExecutorReturner : CommandBase
    {
        public override bool Check(string input) => input == "";

        public override string Process(string input) => input;
    }

    class FindExecutorReturner<BTMBase> : ExecutorReturner where BTMBase : IBTMBase
    {
        private IBTMCollection<BTMBase> collection;
        private All<BTMBase> filter;

        public FindExecutorReturner(IBTMCollection<BTMBase> collection, All<BTMBase> filter = null)
        {
            this.collection = collection;
            this.filter = filter;
        }

        public override ICommandExecutor Execute(string input)
        {
            return new FindExecutor<BTMBase>(collection, filter == null ? null : filter.Clone());
        }
    }

    class FindExecutor<BTMBase> : ICommandExecutor where BTMBase : IBTMBase
    {
        private IBTMCollection<BTMBase> collection;
        private IAction<BTMBase> printIfFilters;

        public FindExecutor(IBTMCollection<BTMBase> collection, All<BTMBase> filter = null)
        {
            this.collection = collection;
            filter = filter ?? new All<BTMBase>(new List<IPredicate<BTMBase>>() { new True<BTMBase>() });
            printIfFilters = new ActionIf<BTMBase>(new Print<BTMBase>(), filter);
        }
        
        public void Execute()
        {
            CollectionUtils.ForEach(collection.First(), printIfFilters);
        }
    }

    class AddExecutorReturner<BTMBase> : ExecutorReturner where BTMBase : IBTMBase
    {
        private IBTMCollection<BTMBase> collection;
        private IBTMBuilder<BTMBase> builder;

        public AddExecutorReturner(IBTMCollection<BTMBase> collection, IBTMBuilder<BTMBase> builder)
        {
            this.collection = collection;
            this.builder = builder;
        }

        public override ICommandExecutor Execute(string input)
        {
            return new AddExecutor<BTMBase>(collection, builder.Result());
        }
    }

    class AddExecutor<BTMBase> : ICommandExecutor where BTMBase : IBTMBase
    {
        private IBTMCollection<BTMBase> collection;
        private BTMBase newObject;

        public AddExecutor(IBTMCollection<BTMBase> collection, BTMBase newObject)
        {
            this.collection = collection;
            this.newObject = newObject;
        }
        
        public void Execute()
        {
            collection.Add(newObject);
        }
    }

    class EditExecutorReturner<BTMBase> : ExecutorReturner where BTMBase : class, IBTMBase
    {
        private IBTMCollection<BTMBase> collection;
        private ActionSequence<BTMBase> actionSequence;
        private All<BTMBase> filter;

        public EditExecutorReturner(IBTMCollection<BTMBase> collection, ActionSequence<BTMBase> actionSequence, All<BTMBase> filter)
        {
            this.collection = collection;
            this.actionSequence = actionSequence;
            this.filter = filter;
        }

        public override ICommandExecutor Execute(string input)
        {
            return new EditExecutor<BTMBase>(collection, actionSequence.Clone(), filter.Clone());
        }
    }

    class EditExecutor<BTMBase> : ICommandExecutor where BTMBase : class, IBTMBase
    {
        private IBTMCollection<BTMBase> collection;
        private ActionSequence<BTMBase> actionSequence;
        private All<BTMBase> filter;

        public EditExecutor(IBTMCollection<BTMBase> collection, ActionSequence<BTMBase> actionSequence, All<BTMBase> filter)
        {
            this.collection = collection;
            this.actionSequence = actionSequence;
            this.filter = filter;
        }
        
        public void Execute()
        {
            actionSequence.Eval(CollectionUtils.Find(collection.First(), filter));
        }
    }

    class DeleteExecutorReturner<BTMBase> : ExecutorReturner where BTMBase : IBTMBase
    {
        private IBTMCollection<BTMBase> collection;
        private All<BTMBase> filter;

        public DeleteExecutorReturner(IBTMCollection<BTMBase> collection, All<BTMBase> filter)
        {
            this.collection = collection;
            this.filter = filter;
        }

        public override ICommandExecutor Execute(string input)
        {
            return new DeleteExecutor<BTMBase>(collection, filter.Clone());
        }
    }

    class DeleteExecutor<BTMBase> : ICommandExecutor where BTMBase : IBTMBase
    {
        private IBTMCollection<BTMBase> collection;
        private All<BTMBase> filter;

        public DeleteExecutor(IBTMCollection<BTMBase> collection, All<BTMBase> filter)
        {
            this.collection = collection;
            this.filter = filter;
        }
        
        public void Execute()
        {
            collection.RemoveIfFirst(filter);
        }
    }
}