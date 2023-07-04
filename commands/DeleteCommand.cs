using System.Collections.Generic;
using static BTM.CollectionUtils;

namespace BTM
{
    class DeleteCommand : KeywordConsumer
    {
        public DeleteCommand() :
            base(new List<CommandBase>() {
                new DeleteFromCollection<ILine>(new LineQuery()),
                new DeleteFromCollection<IStop>(new StopQuery()),
                new DeleteFromCollection<IBytebus>(new BytebusQuery()),
                new DeleteFromCollection<ITram>(new TramQuery()),
                new DeleteFromCollection<IVehicle>(new VehicleQuery()),
                new DeleteFromCollection<IDriver>(new DriverQuery()),
            }, "delete")
        { }

        private class DeleteFromCollection<BTMBase> : CollectionSelector<BTMBase>
            where BTMBase : class, IBTMBase
        {
            public DeleteFromCollection(ICollectionQuery<BTMBase> query) :
                base(query)
            {
                subcommands.AddRange(query.CreateFilterAdders(subcommands));
                subcommands.Add(new RecordNumberVerifier<BTMBase>(
                    1,
                    new DeleteExecutorFactory<BTMBase>(query),
                    new ConsoleLineWriter("Conditions do not specify one record uniquely."),
                    query
                ));
            }
        }

        private class DeleteExecutorFactory<BTMBase> : ExecutorFactory where BTMBase : class, IBTMBase
        {
            private ICollectionQuery<BTMBase> query;

            public DeleteExecutorFactory(ICollectionQuery<BTMBase> query)
            {
                this.query = query;
            }

            public override IExecutor Execute(string input)
            {
                return new DeleteExecutor(query.Collection, query.Filter, query.CollectionName);
            }

            private class DeleteExecutor : IExecutor
            {
                private IBTMCollection<BTMBase> collection;
                private All<BTMBase> filter;
                private BTMBase deletedObject;
                private string commandString;
                private int index;

                public DeleteExecutor(IBTMCollection<BTMBase> collection, All<BTMBase> filter, string collectionName)
                {
                    this.collection = collection;
                    this.filter = new All<BTMBase>(new List<IPredicate<BTMBase>>(filter.Predicates));
                    this.commandString = $"delete {collectionName} {string.Join(" ", filter.Predicates)}";
                }

                public void Do()
                {
                    deletedObject = Find<BTMBase>(collection.First(), filter);
                    this.index = collection.Remove(deletedObject);
                }

                public void Undo()
                {
                    collection.Add(this.index, deletedObject);
                }

                public override string ToString() => commandString;
            }
        }
    }
}
