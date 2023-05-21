namespace BTM
{
    interface ICommandExecutor
    {
        void Execute();
    }

    abstract class ExecutorReturner : CommandBase
    {
        public override bool Check(string input) => input == "";

        public override string Process(string input) => input;
    }

    class FindExecutorReturner<BTMBase> : ExecutorReturner where BTMBase : IBTMBase
    {
        private NamedCollection<BTMBase> collection;
        private All<BTMBase> filter;

        public FindExecutorReturner(NamedCollection<BTMBase> collection, All<BTMBase> filter = null)
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
        private NamedCollection<BTMBase> collection;
        private All<BTMBase> filter;

        public FindExecutor(NamedCollection<BTMBase> collection, All<BTMBase> filter = null)
        {
            this.collection = collection;
            this.filter = filter ?? new All<BTMBase>(new List<IPredicate<BTMBase>>());
        }
        
        public void Execute()
        {
            CollectionUtils.ForEach(collection.Collection.First(), new ActionIf<BTMBase>(new Print<BTMBase>(), filter));
        }

        public override string ToString()
        {
            return filter.Count == 0 ? $"list {collection}" : $"find {collection} {filter}";
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
            string buildLogs = builder.ToString();
            return new AddExecutor<BTMBase>(collection, builder.Result(), buildLogs);
        }
    }

    class AddExecutor<BTMBase> : ICommandExecutor where BTMBase : IBTMBase
    {
        private IBTMCollection<BTMBase> collection;
        private BTMBase newObject;
        private string buildLogs;

        public AddExecutor(IBTMCollection<BTMBase> collection, BTMBase newObject, string buildLogs)
        {
            this.collection = collection;
            this.newObject = newObject;
            this.buildLogs = buildLogs;
        }
        
        public void Execute()
        {
            collection.Add(newObject);
        }

        public override string ToString()
        {
            return buildLogs;
        }
    }

    class EditExecutorReturner<BTMBase> : ExecutorReturner where BTMBase : class, IBTMBase
    {
        private NamedCollection<BTMBase> collection;
        private All<BTMBase> filter;
        private ActionSequence<BTMBase> actionSequence;

        public EditExecutorReturner(NamedCollection<BTMBase> collection, All<BTMBase> filter, ActionSequence<BTMBase> actionSequence)
        {
            this.collection = collection;
            this.filter = filter;
            this.actionSequence = actionSequence;
        }

        public override ICommandExecutor Execute(string input)
        {
            return new EditExecutor<BTMBase>(collection, actionSequence.Clone(), filter.Clone());
        }
    }

    class EditExecutor<BTMBase> : ICommandExecutor where BTMBase : class, IBTMBase
    {
        private NamedCollection<BTMBase> collection;
        private ActionSequence<BTMBase> actionSequence;
        private All<BTMBase> filter;

        public EditExecutor(NamedCollection<BTMBase> collection, ActionSequence<BTMBase> actionSequence, All<BTMBase> filter)
        {
            this.collection = collection;
            this.actionSequence = actionSequence;
            this.filter = filter;
        }
        
        public void Execute()
        {
            actionSequence.Eval(CollectionUtils.Find(collection.Collection.First(), filter));
        }

        public override string ToString()
        {
            return $"edit {collection} {filter}\n{actionSequence}\ndone";
        }
    }

    class DeleteExecutorReturner<BTMBase> : ExecutorReturner where BTMBase : IBTMBase
    {
        private NamedCollection<BTMBase> collection;
        private All<BTMBase> filter;

        public DeleteExecutorReturner(NamedCollection<BTMBase> collection, All<BTMBase> filter)
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
        private NamedCollection<BTMBase> collection;
        private All<BTMBase> filter;

        public DeleteExecutor(NamedCollection<BTMBase> collection, All<BTMBase> filter)
        {
            this.collection = collection;
            this.filter = filter;
        }
        
        public void Execute()
        {
            collection.Collection.RemoveIfFirst(filter);
        }

        public override string ToString()
        {
            return $"delete {collection} {filter}";
        }
    }
}