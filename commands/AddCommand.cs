using System.Collections.Generic;

namespace BTM
{
    class AddCommand : KeywordConsumer, IHelpable
    {
        public AddCommand() : base(new List<CommandBase>()
        {
            new AddToCollection<ILine, ILineBuilder>(new LineQuery(), new LineBuilderLogger(new LineBaseBuilder()), new LineBuilderLogger(new LineTextBuilder())),
            new AddToCollection<IStop, IStopBuilder>(new StopQuery(), new StopBuilderLogger(new StopBaseBuilder()), new StopBuilderLogger(new StopTextBuilder())),
            new AddToCollection<IBytebus, IBytebusBuilder>(new BytebusQuery(), new BytebusBuilderLogger(new BytebusBaseBuilder()), new BytebusBuilderLogger(new BytebusTextBuilder())),
            new AddToCollection<ITram, ITramBuilder>(new TramQuery(), new TramBuilderLogger(new TramBaseBuilder()), new TramBuilderLogger(new TramTextBuilder())),
            new AddToCollection<IDriver, IDriverBuilder>(new DriverQuery(), new DriverBuilderLogger(new DriverBaseBuilder()), new DriverBuilderLogger(new DriverTextBuilder())),
        }, "add")
        { }

        public string HelpKeyword => "add";

        public string Help =>
@"USAGE: add <collection> (base|secondary)
       <attribute>=<value>
          :
       <attribute>=<value>
       (done|exit)

collection: line|stop|bytebus|tram|driver

Creates and adds new entity to specified collection. Second
argument specifies representation of the entity:
    base        - Objects with fields
    secondary   - Objects encoded as strings

After being called, command becomes interactive and in each line
accepts an input in format `<attribute>=<value>` that allows
setting values of attributes of newly created entity. To finish
and save the entity to it's corresponding collection pass `done`, 
to discard the entity pass `exit`. 
";

        private class AddToCollection<BTMBase, BTMBaseBuilder> : CollectionSelector<BTMBase>
            where BTMBase : IBTMBase
            where BTMBaseBuilder : IBTMBuilder<BTMBase>
        {
            public AddToCollection(IBuildableCollectionQuery<BTMBase, BTMBaseBuilder> query, BTMBaseBuilder baseBuilder, BTMBaseBuilder secondaryBuilder) :
                base(query)
            {
                subcommands.Add(new KeywordConsumer(
                    new ConsoleLineWriter(
                        new BuilderDescriber(query, baseBuilder),
                        $"[{query.FieldDescription}]"
                    ),
                    "base"
                ));

                subcommands.Add(new KeywordConsumer(
                    new ConsoleLineWriter(
                        new BuilderDescriber(query, secondaryBuilder),
                        $"[{query.FieldDescription}]"
                    ),
                    "secondary"
                ));
            }

            private class BuilderDescriber : CommandBase
            {
                private IBTMBuilder<BTMBase> builder;

                public BuilderDescriber(IBuildableCollectionQuery<BTMBase, BTMBaseBuilder> query, BTMBaseBuilder builder)
                {
                    this.builder = builder;

                    List<CommandBase> readerSubcommands = new List<CommandBase>()
                    {
                        new KeywordConsumer(
                            new ConsoleLineWriter(
                                new AddExecutorFactory<BTMBase>(query, builder),
                                "SUCCESS"
                            ),
                            "done"
                        ),
                        new KeywordConsumer("exit"),
                    };
                    readerSubcommands.AddRange(query.CreateBuilderAdders(subcommands, builder));
                    readerSubcommands.Add(new ConsoleLineWriter(subcommands, $"Previous line contains an error."));

                    subcommands.Add(new ConsoleLineReader(readerSubcommands));
                }

                public override bool Check(string input) => true;

                public override string Process(string input)
                {
                    builder.Reset();
                    return input;
                }
            }
        }

        private class AddExecutorFactory<BTMBase> : ExecutorFactory where BTMBase : IBTMBase
        {
            private ICollectionQuery<BTMBase> query;
            private IBTMBuilder<BTMBase> builder;

            public AddExecutorFactory(ICollectionQuery<BTMBase> query, IBTMBuilder<BTMBase> builder)
            {
                this.query = query;
                this.builder = builder;
            }

            public override IExecutor Execute(string input)
            {
                string commandString = builder.ToString();
                return new AddExecutor(query.Collection, builder.Result(), commandString);
            }

            private class AddExecutor : IExecutor
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
        }
    }
}
