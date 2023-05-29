using System.Collections.Generic;

namespace BTM
{
    interface IExecutor
    {
        void Do();
        void Undo();
    }

    abstract class ExecutorFactory : CommandBase
    {
        public override bool Check(string input) => input == "";

        public override string Process(string input) => input;
    }

    class FindExecutorFactory<BTMBase> : ExecutorFactory where BTMBase : IBTMBase
    {
        private IBTMCollection<BTMBase> collection;
        private All<BTMBase> filter;
        private string collectionName;

        public FindExecutorFactory(IBTMCollection<BTMBase> collection, All<BTMBase> filter, string collectionName)
        {
            this.collection = collection;
            this.filter = filter;
            this.collectionName = collectionName;
        }

        public override IExecutor Execute(string input)
        {
            return new FindExecutor<BTMBase>(
                collection, 
                filter.Clone(), 
                filter.Count == 0 ? $"list {collectionName}" : $"find {collectionName} {filter}"
            );
        }
    }

    class FindExecutor<BTMBase> : IExecutor where BTMBase : IBTMBase
    {
        private IBTMCollection<BTMBase> collection;
        private All<BTMBase> filter;
        private string commandString;

        public FindExecutor(IBTMCollection<BTMBase> collection, All<BTMBase> filter, string commandString)
        {
            this.collection = collection;
            this.filter = filter;
            this.commandString = commandString;
        }
        
        public void Do()
        {
            CollectionUtils.ForEach(collection.First(), new ActionIf<BTMBase>(new Print<BTMBase>(), filter));
        }

        public void Undo()
        {
            // TODO: Console.SetCursorPosition, etc...
        }

        public override string ToString() => commandString;
    }

    class AddExecutorFactory<BTMBase> : ExecutorFactory where BTMBase : IBTMBase
    {
        private IBTMCollection<BTMBase> collection;
        private IBTMBuilder<BTMBase> builder;

        public AddExecutorFactory(IBTMCollection<BTMBase> collection, IBTMBuilder<BTMBase> builder)
        {
            this.collection = collection;
            this.builder = builder;
        }

        public override IExecutor Execute(string input)
        {
            string commandString = builder.ToString();
            return new AddExecutor<BTMBase>(collection, builder.Result(), commandString);
        }
    }

    class AddExecutor<BTMBase> : IExecutor where BTMBase : IBTMBase
    {
        private IBTMCollection<BTMBase> collection;
        private BTMBase addedObject;
        private string commandString;

        public AddExecutor(IBTMCollection<BTMBase> collection, BTMBase addedObject, string commandString)
        {
            this.collection = collection;
            this.addedObject = addedObject;
            this.commandString = commandString;
        }
        
        public void Do()
        {
            collection.Add(addedObject);
        }

        public void Undo()
        {
            collection.Remove(addedObject);
        }

        public override string ToString() => commandString;
    }

    class EditExecutorFactory<BTMBase> : ExecutorFactory where BTMBase : class, IBTMBase, IRestoreable<BTMBase>
    {
        private IBTMCollection<BTMBase> collection;
        private All<BTMBase> filter;
        private ActionSequence<BTMBase> actionSequence;
        private string collectionName;

        public EditExecutorFactory(IBTMCollection<BTMBase> collection, All<BTMBase> filter, ActionSequence<BTMBase> actionSequence, string collectionName)
        {
            this.collection = collection;
            this.filter = filter;
            this.actionSequence = actionSequence;
            this.collectionName = collectionName;
        }

        public override IExecutor Execute(string input)
        {
            return new EditExecutor<BTMBase>(
                collection, 
                filter, 
                actionSequence.Clone(), 
                $"edit {collectionName} {filter}\n{actionSequence}\ndone"
            );
        }
    }

    class EditExecutor<BTMBase> : IExecutor where BTMBase : class, IBTMBase, IRestoreable<BTMBase>
    {
        private IBTMCollection<BTMBase> collection;
        private All<BTMBase> filter;
        private ActionSequence<BTMBase> actionSequence;
        private BTMBase editedObject, objectBackup;
        private string commandString;

        public EditExecutor(IBTMCollection<BTMBase> collection, All<BTMBase> filter, ActionSequence<BTMBase> actionSequence, string commandString)
        {
            this.collection = collection;
            this.filter = filter;
            this.actionSequence = actionSequence;
            this.commandString = commandString;
        }
        
        public void Do()
        {
            editedObject = CollectionUtils.Find(collection.First(), filter);
            objectBackup = editedObject.Clone();
            actionSequence.Eval(editedObject);
        }

        public void Undo()
        {
            editedObject.CopyFrom(objectBackup);
        }

        public override string ToString() => commandString;
    }

    class DeleteExecutorFactory<BTMBase> : ExecutorFactory where BTMBase : class, IBTMBase
    {
        private IBTMCollection<BTMBase> collection;
        private All<BTMBase> filter;
        private string collectionName;

        public DeleteExecutorFactory(IBTMCollection<BTMBase> collection, All<BTMBase> filter, string collectionName)
        {
            this.collection = collection;
            this.filter = filter;
            this.collectionName = collectionName;
        }

        public override IExecutor Execute(string input)
        {
            return new DeleteExecutor<BTMBase>(
                collection, 
                filter, 
                $"delete {collectionName} {filter}"
            );
        }
    }

    class DeleteExecutor<BTMBase> : IExecutor where BTMBase : class, IBTMBase
    {
        private IBTMCollection<BTMBase> collection;
        private All<BTMBase> filter;
        private BTMBase deletedObject;
        private string commandString;
        private int index;

        public DeleteExecutor(IBTMCollection<BTMBase> collection, All<BTMBase> filter, string commandString)
        {
            this.collection = collection;
            this.filter = filter;
            this.commandString = commandString;
        }
        
        public void Do()
        {
            deletedObject = CollectionUtils.Find<BTMBase>(collection.First(), filter);
            this.index = collection.Remove(deletedObject);
        }

        public void Undo()
        {
            collection.Add(this.index, deletedObject);
        }

        public override string ToString() => commandString;
    }
}