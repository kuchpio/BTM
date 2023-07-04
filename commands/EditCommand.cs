using System.Collections.Generic;
using static BTM.CollectionUtils;

namespace BTM
{
    class EditCommand : KeywordConsumer
    {
        public EditCommand() : base(new List<CommandBase>()
        {
            new EditFromCollection<ILine>(new LineQuery()),
            new EditFromCollection<IStop>(new StopQuery()),
            new EditFromCollection<IBytebus>(new BytebusQuery()),
            new EditFromCollection<ITram>(new TramQuery()),
            new EditFromCollection<IVehicle>(new VehicleQuery()),
            new EditFromCollection<IDriver>(new DriverQuery()),
        }, "edit")
        { }

        private class EditFromCollection<BTMBase> : CollectionSelector<BTMBase>
            where BTMBase : class, IBTMBase, IRestoreable<BTMBase>
        {
            public EditFromCollection(ICollectionQuery<BTMBase> query) :
                base(query)
            {
                subcommands.AddRange(query.CreateFilterAdders(subcommands));
                subcommands.Add(new RecordNumberVerifier<BTMBase>(
                    1,
                    new EditDescriber(query),
                    new ConsoleLineWriter("Conditions do not specify one record uniquely."),
                    query
                ));
            }

            private class EditDescriber : ConsoleLineWriter
            {
                public EditDescriber(ICollectionQuery<BTMBase> query) :
                    base($"[{query.FieldDescription}]")
                {
                    List<CommandBase> readerSubcommands = new List<CommandBase>()
                    {
                        new KeywordConsumer(
                            new ConsoleLineWriter(
                                new EditExecutorFactory<BTMBase>(query),
                                "SUCCESS"
                            ),
                            "done"
                        ),
                        new KeywordConsumer("exit")
                    };
                    readerSubcommands.AddRange(query.CreateSetterAdders(subcommands));
                    readerSubcommands.Add(new ConsoleLineWriter(subcommands, $"Previous line contains an error."));

                    subcommands.Add(new ConsoleLineReader(readerSubcommands));
                }
            }
        }

        private class EditExecutorFactory<BTMBase> : ExecutorFactory where BTMBase : class, IBTMBase, IRestoreable<BTMBase>
        {
            private ICollectionQuery<BTMBase> query;

            public EditExecutorFactory(ICollectionQuery<BTMBase> query)
            {
                this.query = query;
            }

            public override IExecutor Execute(string input)
            {
                return new EditExecutor(query.Collection, query.Filter, query.EditActions, query.CollectionName);
            }

            private class EditExecutor : IExecutor
            {
                private IBTMCollection<BTMBase> collection;
                private All<BTMBase> filter;
                private ActionSequence<BTMBase> actionSequence;
                private BTMBase editedObject, objectBackup;
                private string commandString;

                public EditExecutor(IBTMCollection<BTMBase> collection, All<BTMBase> filter, ActionSequence<BTMBase> actionSequence, string collectionName)
                {
                    this.collection = collection;
                    this.filter = new All<BTMBase>(new List<IPredicate<BTMBase>>(filter.Predicates));
                    this.actionSequence = new ActionSequence<BTMBase>(new List<IAction<BTMBase>>(actionSequence.Actions));
                    this.commandString = $"edit {collectionName} {string.Join("\n", filter.Predicates)}\n{string.Join("\n", actionSequence.Actions)}\ndone";
                }

                public void Do()
                {
                    editedObject = Find(collection.First(), filter);
                    objectBackup = editedObject.Clone();
                    actionSequence.Eval(editedObject);
                }

                public void Undo()
                {
                    editedObject.CopyFrom(objectBackup);
                }

                public override string ToString() => commandString;
            }
        }
    }
}
