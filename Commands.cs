
namespace BTM 
{
    class Terminal
    {
        private bool work;
        private CommandRunner commandExecutor;
        private List<ICommandExecutor> queue;

        public Terminal()
        {
            work = true;
            commandExecutor = new CommandRunner(this);
            queue = new List<ICommandExecutor>();
        }

        public void Run()
        {
            while (work)
            {
                Console.Write("BTM> ");
                string userInput = Console.ReadLine();

                if (commandExecutor.Check(userInput))
                {
                    ICommandExecutor executor = commandExecutor.Execute(userInput);
                    if (executor != null)
                    {
                        queue.Add(executor);
                    }
                }
                    
            }
        }

        public void Close()
        {
            work = false;
        }

        public void ClearCommandQueue()
        {
            queue.Clear();
        }

        public void ExecuteCommandQueue()
        {
            foreach (ICommandExecutor executor in queue)
                executor.Execute();

            ClearCommandQueue();
        }

        public string CommandQueueToString()
        {
            return string.Join("\n", queue);       
        }
    }

    class QueueCommand : KeywordConsumer
    {
        public QueueCommand(Terminal terminal) : base(new List<CommandBase>() {
            new QueueDismiss(terminal), 
            new QueueCommit(terminal), 
            new QueuePrint(terminal)
        }, "queue")
        { }

        private class QueueDismiss : KeywordConsumer
        {
            private Terminal terminal;
            
            public QueueDismiss(Terminal terminal) : base("dismiss")
            { 
                this.terminal = terminal;
            }

            public override void Action()
            {
                terminal.ClearCommandQueue();
            }
        }

        private class QueueCommit : KeywordConsumer
        {
            private Terminal terminal;
            
            public QueueCommit(Terminal terminal) : base("commit")
            { 
                this.terminal = terminal;
            }

            public override void Action()
            {
                terminal.ExecuteCommandQueue();
            }
        }

        private class QueuePrint : KeywordConsumer
        {
            private Terminal terminal;
            
            public QueuePrint(Terminal terminal) : base("print")
            { 
                this.terminal = terminal;
            }

            public override void Action()
            {
                Console.WriteLine(terminal.CommandQueueToString());
            }
        }
    }

    class CommandRunner : CommandBase
    {
        public CommandRunner(Terminal terminal) : 
            base(new List<CommandBase>() { 
                new ListCommand(), 
                new FindCommand(), 
                new AddCommand(),
                new EditCommand(),
                new DeleteCommand(), 
                new QueueCommand(terminal),
                new ExitCommand(terminal) 
            })
        { }

        public override bool Check(string input)
        {
            return input != null;
        }

        public override string Process(string input)
        {
            return input;
        }
    }

    class ExitCommand : KeywordConsumer
    {
        private Terminal terminal;

        public ExitCommand(Terminal terminal) : base("exit")
        {
            this.terminal = terminal;
        }

        public override void Action()
        {
            terminal.Close();
        }
    }

    class ListCommand : KeywordConsumer 
    {
        public ListCommand() : base(new List<CommandBase>()
        {
            new DisplayLine(),
            new DisplayStop(),
            new DisplayBytebus(),
            new DisplayTram(),
            new DisplayVehicle(),
            new DisplayDriver()
        }, "list")
        { }

        private class DisplayLine : LineSelector
        {
            public DisplayLine()
            {
                subcommands.Add(new FindExecutorReturner<ILine>(new NamedCollection<ILine>(collection, collectionName)));
            }
        }

        private class DisplayStop : StopSelector
        {
            public DisplayStop()
            {
                subcommands.Add(new FindExecutorReturner<IStop>(new NamedCollection<IStop>(collection, collectionName)));
            }
        }

        private class DisplayBytebus : BytebusSelector
        {
            public DisplayBytebus()
            {
                subcommands.Add(new FindExecutorReturner<IBytebus>(new NamedCollection<IBytebus>(collection, collectionName)));
            }
        }

        private class DisplayTram : TramSelector
        {
            public DisplayTram()
            {
                subcommands.Add(new FindExecutorReturner<ITram>(new NamedCollection<ITram>(collection, collectionName)));
            }
        }

        private class DisplayVehicle : VehicleSelector
        {
            public DisplayVehicle()
            {
                subcommands.Add(new FindExecutorReturner<IVehicle>(new NamedCollection<IVehicle>(collection, collectionName)));
            }
        }

        private class DisplayDriver : DriverSelector
        {
            public DisplayDriver()
            {
                subcommands.Add(new FindExecutorReturner<IDriver>(new NamedCollection<IDriver>(collection, collectionName)));
            }
        }
    }

    class FindCommand : KeywordConsumer
    {
        public FindCommand() : base(new List<CommandBase>() 
        { 
            new DisplayFilteredLine(), 
            new DisplayFilteredStop(),
            new DisplayFilteredBytebus(), 
            new DisplayFilteredTram(), 
            new DisplayFilteredVehicle(),
            new DisplayFilteredDriver()
        }, "find")
        { }

        private class DisplayFilteredLine : LineSelector
        {
            public DisplayFilteredLine()
            {
                subcommands.Add(new NumberDecFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new NumberHexFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new CommonNameFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new FindExecutorReturner<ILine>(new NamedCollection<ILine>(collection, collectionName), collectionFilter));
            }
        }

        private class DisplayFilteredStop : StopSelector
        {
            public DisplayFilteredStop()
            {
                subcommands.Add(new IdFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new NameFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new TypeFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new FindExecutorReturner<IStop>(new NamedCollection<IStop>(collection, collectionName), collectionFilter));
            }
        }

        private class DisplayFilteredBytebus : BytebusSelector
        {
            public DisplayFilteredBytebus()
            {
                subcommands.Add(new IdFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new EngineFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new FindExecutorReturner<IBytebus>(new NamedCollection<IBytebus>(collection, collectionName), collectionFilter));
            }
        }

        private class DisplayFilteredTram : TramSelector
        {
            public DisplayFilteredTram()
            {
                subcommands.Add(new IdFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new CarsNumberFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new FindExecutorReturner<ITram>(new NamedCollection<ITram>(collection, collectionName), collectionFilter));
            }
        }

        private class DisplayFilteredVehicle : VehicleSelector
        {
            public DisplayFilteredVehicle()
            {
                subcommands.Add(new IdFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new FindExecutorReturner<IVehicle>(new NamedCollection<IVehicle>(collection, collectionName), collectionFilter));
            }
        }

        private class DisplayFilteredDriver : DriverSelector
        {
            public DisplayFilteredDriver()
            {
                subcommands.Add(new NameFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new SurnameFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new SeniorityFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new FindExecutorReturner<IDriver>(new NamedCollection<IDriver>(collection, collectionName), collectionFilter));
            }
        }
    }

    class AddCommand : KeywordConsumer
    {
        public AddCommand() : base(new List<CommandBase>()
        {
            new AddLine(), 
            new AddStop(), 
            new AddBytebus(), 
            new AddTram(), 
            new AddDriver()
        }, "add")
        { }

        private class BuilderResetter<BTMBase> : CommandBase where BTMBase : IBTMBase
        {
            private IBTMBuilder<BTMBase> builder;
            
            public BuilderResetter(IBTMBuilder<BTMBase> builder)
            {
                this.builder = builder;
            }

            public override bool Check(string input) => true;

            public override string Process(string input)
            {
                builder.Reset();
                return input;
            }
        }

        private class BuildConfirmer<BTMBase> : KeywordConsumer where BTMBase : IBTMBase
        {
            public BuildConfirmer(IBTMCollection<BTMBase> collection, IBTMBuilder<BTMBase> builder) : 
                base(new ConsoleMessageWriter(new List<CommandBase>() 
                        { new AddExecutorReturner<BTMBase>(collection, builder) }, 
                        "Object registered for addition."
                    ), "done")
            { }
        }

        private class BuildDiscarder : KeywordConsumer
        {
            public BuildDiscarder() :
                base(new ConsoleMessageWriter(new List<CommandBase>(), "Object creation abandoned."), "exit")
            { }
        }

        private class AddLine : LineSelector
        {
            public AddLine()
            {
                subcommands.Add(
                    new KeywordConsumer(
                        new ConsoleMessageWriter(
                            new BuilderDescriptor(collection, new LineBuilderLogger(new LineBaseBuilder())), 
                            "`numberDec`: numeric, `numberHex`: string, `commonName`: string"
                        ), "base"
                    ));

                subcommands.Add(
                    new KeywordConsumer(
                        new ConsoleMessageWriter(
                            new BuilderDescriptor(collection, new LineBuilderLogger(new LineTextBuilder())), 
                            "`numberDec`: numeric, `numberHex`: string, `commonName`: string"
                        ), "secondary"
                    ));
            }

            private class BuilderDescriptor : BuilderResetter<ILine>
            {
                public BuilderDescriptor(IBTMCollection<ILine> collection, ILineBuilder builder) : base(builder)
                {
                    subcommands.Add(
                        new ConsoleLineReader(new List<CommandBase>() {
                            new BuildConfirmer<ILine>(collection, builder), 
                            new BuildDiscarder(), 
                            new NumberDecBuilderAdder(subcommands, builder), 
                            new NumberHexBuilderAdder(subcommands, builder), 
                            new CommonNameBuilderAdder(subcommands, builder),
                            new ConsoleMessageWriter(subcommands, $"Previous line contains an error.")
                        })
                    );
                }
            }
        }

        private class AddStop : StopSelector
        {
            public AddStop()
            {
                subcommands.Add(
                    new KeywordConsumer(
                        new ConsoleMessageWriter(
                            new BuilderDescriptor(collection, new StopBuilderLogger(new StopBaseBuilder())), 
                            "`id`: numeric, `name`: string, `type`: string"
                        ), "base"
                    ));

                subcommands.Add(
                    new KeywordConsumer(
                        new ConsoleMessageWriter(
                            new BuilderDescriptor(collection, new StopBuilderLogger(new StopTextBuilder())), 
                            "`id`: numeric, `name`: string, `type`: string"
                        ), "secondary"
                    ));
            }

            private class BuilderDescriptor : BuilderResetter<IStop>
            {
                public BuilderDescriptor(IBTMCollection<IStop> collection, IStopBuilder builder) : base(builder)
                {
                    subcommands.Add(
                        new ConsoleLineReader(new List<CommandBase>() {
                            new BuildConfirmer<IStop>(collection, builder), 
                            new BuildDiscarder(), 
                            new IdBuilderAdder(subcommands, builder), 
                            new NameBuilderAdder(subcommands, builder), 
                            new TypeBuilderAdder(subcommands, builder),
                            new ConsoleMessageWriter(subcommands, $"Previous line contains an error.")
                        })
                    );
                }
            }
        }

        private class AddBytebus : BytebusSelector
        {
            public AddBytebus()
            {
                subcommands.Add(
                    new KeywordConsumer(
                        new ConsoleMessageWriter(
                            new BuilderDescriptor(collection, new BytebusBuilderLogger(new BytebusBaseBuilder())), 
                            "`id`: numeric, `engine`: string"
                        ), "base"
                    ));

                subcommands.Add(
                    new KeywordConsumer(
                        new ConsoleMessageWriter(
                            new BuilderDescriptor(collection, new BytebusBuilderLogger(new BytebusTextBuilder())), 
                            "`id`: numeric, `engine`: string"
                        ), "secondary"
                    ));
            }

            private class BuilderDescriptor : BuilderResetter<IBytebus>
            {
                public BuilderDescriptor(IBTMCollection<IBytebus> collection, IBytebusBuilder builder) : base(builder)
                {
                    subcommands.Add(
                        new ConsoleLineReader(new List<CommandBase>() {
                            new BuildConfirmer<IBytebus>(collection, builder), 
                            new BuildDiscarder(), 
                            new IdBuilderAdder(subcommands, builder), 
                            new EngineBuilderAdder(subcommands, builder),
                            new ConsoleMessageWriter(subcommands, $"Previous line contains an error.")
                        })
                    );
                }
            }
        }

        private class AddTram : TramSelector
        {
            public AddTram()
            {
                subcommands.Add(
                    new KeywordConsumer(
                        new ConsoleMessageWriter(
                            new BuilderDescriptor(collection, new TramBuilderLogger(new TramBaseBuilder())), 
                            "`id`: numeric, `carsNumber`: numeric"
                        ), "base"
                    ));

                subcommands.Add(
                    new KeywordConsumer(
                        new ConsoleMessageWriter(
                            new BuilderDescriptor(collection, new TramBuilderLogger(new TramTextBuilder())), 
                            "`id`: numeric, `carsNumber`: numeric"
                        ), "secondary"
                    ));
            }

            private class BuilderDescriptor : BuilderResetter<ITram>
            {
                public BuilderDescriptor(IBTMCollection<ITram> collection, ITramBuilder builder) : base(builder)
                {
                    subcommands.Add(
                        new ConsoleLineReader(new List<CommandBase>() {
                            new BuildConfirmer<ITram>(collection, builder), 
                            new BuildDiscarder(), 
                            new IdBuilderAdder(subcommands, builder), 
                            new CarsNumberBuilderAdder(subcommands, builder),
                            new ConsoleMessageWriter(subcommands, $"Previous line contains an error.")
                        })
                    );
                }
            }
        }

        private class AddDriver : DriverSelector
        {
            public AddDriver()
            {
                subcommands.Add(
                    new KeywordConsumer(
                        new ConsoleMessageWriter(
                            new BuilderDescriptor(collection, new DriverBuilderLogger(new DriverBaseBuilder())), 
                            "`name`: string, `surname`: string, `seniority`: numeric"
                        ), "base"
                    ));

                subcommands.Add(
                    new KeywordConsumer(
                        new ConsoleMessageWriter(
                            new BuilderDescriptor(collection, new DriverBuilderLogger(new DriverTextBuilder())), 
                            "`name`: string, `surname`: string, `seniority`: numeric"
                        ), "secondary"
                    ));
            }

            private class BuilderDescriptor : BuilderResetter<IDriver>
            {
                public BuilderDescriptor(IBTMCollection<IDriver> collection, IDriverBuilder builder) : base(builder)
                {
                    subcommands.Add(
                        new ConsoleLineReader(new List<CommandBase>() {
                            new BuildConfirmer<IDriver>(collection, builder), 
                            new BuildDiscarder(), 
                            new NameBuilderAdder(subcommands, builder), 
                            new SurnameBuilderAdder(subcommands, builder), 
                            new SeniorityBuilderAdder(subcommands, builder),
                            new ConsoleMessageWriter(subcommands, $"Previous line contains an error.")
                        })
                    );
                }
            }
        }
    }

    class EditCommand : KeywordConsumer
    {
        public EditCommand() : base(new List<CommandBase>()
        {
            new EditLine(),
            new EditStop(),
            new EditBytebus(),
            new EditTram(), 
            new EditVehicle(), 
            new EditDriver()
        }, "edit")
        { }

        private class EditConfirmer<BTMBase> : KeywordConsumer where BTMBase : class, IBTMBase
        {
            public EditConfirmer(NamedCollection<BTMBase> collection, All<BTMBase> filters, ActionSequence<BTMBase> actionSequence) : 
                base(new ConsoleMessageWriter(new List<CommandBase>() {
                    new EditExecutorReturner<BTMBase>(collection, filters, actionSequence)
                }, "Object succesfully registered for edit."), "done")
            { }
        }

        private class EditDiscarder : KeywordConsumer
        {
            public EditDiscarder() : 
                base(new ConsoleMessageWriter("No object registered for edit."), "exit")
            { }
        }

        private class EditLine : LineSelector
        {
            public EditLine()
            {
                subcommands.Add(new NumberDecFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new NumberHexFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new CommonNameFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new RecordNumberVerifier<ILine>(
                    1, 
                    new EditDescriptor(new NamedCollection<ILine>(collection, collectionName), collectionFilter, new ActionSequence<ILine>()), 
                    new ConsoleMessageWriter(new List<CommandBase>(), "Conditions do not specify one record uniquely."), 
                    collection, 
                    collectionFilter
                ));
            }

            private class EditDescriptor : ConsoleMessageWriter
            {
                public EditDescriptor(NamedCollection<ILine> collection, All<ILine> filter, ActionSequence<ILine> setActions) : 
                    base(new List<CommandBase>(), "`numberDec`: numeric, `numberHex`: string, `commonName`: string")
                {   
                    subcommands.Add(
                        new ConsoleLineReader(new List<CommandBase>() {
                            new EditConfirmer<ILine>(collection, filter, setActions),
                            new EditDiscarder(), 
                            new NumberDecSetterAdder(subcommands, setActions), 
                            new NumberHexSetterAdder(subcommands, setActions), 
                            new CommonNameSetterAdder(subcommands, setActions),
                            new ConsoleMessageWriter(subcommands, $"Previous line contains an error.")
                        }));
                }
            }
        }

        private class EditStop : StopSelector
        {
            public EditStop()
            {
                subcommands.Add(new IdFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new NameFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new TypeFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new RecordNumberVerifier<IStop>(
                    1, 
                    new EditDescriptor(new NamedCollection<IStop>(collection, collectionName), collectionFilter, new ActionSequence<IStop>()), 
                    new ConsoleMessageWriter(new List<CommandBase>(), "Conditions do not specify one record uniquely."), 
                    collection, 
                    collectionFilter
                ));
            }

            private class EditDescriptor : ConsoleMessageWriter
            {
                public EditDescriptor(NamedCollection<IStop> collection, All<IStop> filter, ActionSequence<IStop> setActions) : 
                    base(new List<CommandBase>(), "`id`: numeric, `name`: string, `type`: string")
                {   
                    subcommands.Add(
                        new ConsoleLineReader(new List<CommandBase>() {
                            new EditConfirmer<IStop>(collection, filter, setActions),
                            new EditDiscarder(), 
                            new IdSetterAdder(subcommands, setActions), 
                            new NameSetterAdder(subcommands, setActions), 
                            new TypeSetterAdder(subcommands, setActions),
                            new ConsoleMessageWriter(subcommands, $"Previous line contains an error.")
                        }));
                }
            }
        }

        private class EditBytebus : BytebusSelector
        {
            public EditBytebus()
            {
                subcommands.Add(new IdFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new EngineFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new RecordNumberVerifier<IBytebus>(
                    1, 
                    new EditDescriptor(new NamedCollection<IBytebus>(collection, collectionName), collectionFilter, new ActionSequence<IBytebus>()), 
                    new ConsoleMessageWriter(new List<CommandBase>(), "Conditions do not specify one record uniquely."), 
                    collection, 
                    collectionFilter
                ));
            }

            private class EditDescriptor : ConsoleMessageWriter
            {
                public EditDescriptor(NamedCollection<IBytebus> collection, All<IBytebus> filter, ActionSequence<IBytebus> setActions) : 
                    base(new List<CommandBase>(), "`id`: numeric, `engine`: string")
                {   
                    subcommands.Add(
                        new ConsoleLineReader(new List<CommandBase>() {
                            new EditConfirmer<IBytebus>(collection, filter, setActions),
                            new EditDiscarder(), 
                            new IdSetterAdder(subcommands, setActions), 
                            new EngineSetterAdder(subcommands, setActions), 
                            new ConsoleMessageWriter(subcommands, $"Previous line contains an error.")
                        }));
                }
            }
        }

        private class EditTram : TramSelector
        {
            public EditTram()
            {
                subcommands.Add(new IdFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new CarsNumberFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new RecordNumberVerifier<ITram>(
                    1, 
                    new EditDescriptor(new NamedCollection<ITram>(collection, collectionName), collectionFilter, new ActionSequence<ITram>()), 
                    new ConsoleMessageWriter(new List<CommandBase>(), "Conditions do not specify one record uniquely."), 
                    collection, 
                    collectionFilter
                ));
            }

            private class EditDescriptor : ConsoleMessageWriter
            {
                public EditDescriptor(NamedCollection<ITram> collection, All<ITram> filter, ActionSequence<ITram> setActions) : 
                    base(new List<CommandBase>(), "`id`: numeric, `carsNumber`: numeric")
                {   
                    subcommands.Add(
                        new ConsoleLineReader(new List<CommandBase>() {
                            new EditConfirmer<ITram>(collection, filter, setActions),
                            new EditDiscarder(), 
                            new IdSetterAdder(subcommands, setActions), 
                            new CarsNumberSetterAdder(subcommands, setActions), 
                            new ConsoleMessageWriter(subcommands, $"Previous line contains an error.")
                        }));
                }
            }
        }

        private class EditVehicle : VehicleSelector
        {
            public EditVehicle()
            {
                subcommands.Add(new IdFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new RecordNumberVerifier<IVehicle>(
                    1, 
                    new EditDescriptor(new NamedCollection<IVehicle>(collection, collectionName), collectionFilter, new ActionSequence<IVehicle>()), 
                    new ConsoleMessageWriter(new List<CommandBase>(), "Conditions do not specify one record uniquely."), 
                    collection, 
                    collectionFilter
                ));
            }

            private class EditDescriptor : ConsoleMessageWriter
            {
                public EditDescriptor(NamedCollection<IVehicle> collection, All<IVehicle> filter, ActionSequence<IVehicle> setActions) : 
                    base(new List<CommandBase>(), "`id`: numeric")
                {   
                    subcommands.Add(
                        new ConsoleLineReader(new List<CommandBase>() {
                            new EditConfirmer<IVehicle>(collection, filter, setActions),
                            new EditDiscarder(), 
                            new IdSetterAdder(subcommands, setActions), 
                            new ConsoleMessageWriter(subcommands, $"Previous line contains an error.")
                        }));
                }
            }
        }

        private class EditDriver : DriverSelector
        {
            public EditDriver()
            {
                subcommands.Add(new NameFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new SurnameFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new SeniorityFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new RecordNumberVerifier<IDriver>(
                    1, 
                    new EditDescriptor(new NamedCollection<IDriver>(collection, collectionName), collectionFilter, new ActionSequence<IDriver>()), 
                    new ConsoleMessageWriter(new List<CommandBase>(), "Conditions do not specify one record uniquely."), 
                    collection, 
                    collectionFilter
                ));
            }

            private class EditDescriptor : ConsoleMessageWriter
            {
                public EditDescriptor(NamedCollection<IDriver> collection, All<IDriver> filter, ActionSequence<IDriver> setActions) : 
                    base(new List<CommandBase>(), "`name`: string, `surname`: string, `seniority`: numeric")
                {   
                    subcommands.Add(
                        new ConsoleLineReader(new List<CommandBase>() {
                            new EditConfirmer<IDriver>(collection, filter, setActions),
                            new EditDiscarder(), 
                            new NameSetterAdder(subcommands, setActions), 
                            new SurnameSetterAdder(subcommands, setActions), 
                            new SenioritySetterAdder(subcommands, setActions), 
                            new ConsoleMessageWriter(subcommands, $"Previous line contains an error.")
                        }));
                }
            }
        }
    }

    class DeleteCommand : KeywordConsumer
    {
        public DeleteCommand() : 
            base(new List<CommandBase>() {
                new DeleteLine(),
                new DeleteStop(), 
                new DeleteBytebus(), 
                new DeleteTram(), 
                new DeleteDriver()
            }, "delete")
        { }

        private class Deleter<BTMBase> : ConsoleMessageWriter where BTMBase : IBTMBase
        {
            public Deleter(NamedCollection<BTMBase> collection, All<BTMBase> filter) : base(new List<CommandBase>() {
                new DeleteExecutorReturner<BTMBase>(collection, filter)
            }, "Object succesfully registered for deletion.")
            { }
        }

        private class DeleteLine : LineSelector
        {
            public DeleteLine()
            {
                subcommands.Add(new NumberDecFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new NumberHexFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new CommonNameFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new RecordNumberVerifier<ILine>(
                    1, 
                    new Deleter<ILine>(new NamedCollection<ILine>(collection, collectionName), collectionFilter), 
                    new ConsoleMessageWriter("Conditions do not specify one record uniquely."), 
                    collection, 
                    collectionFilter
                ));
            }
        }

        private class DeleteStop : StopSelector
        {
            public DeleteStop()
            {
                subcommands.Add(new IdFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new NameFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new TypeFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new RecordNumberVerifier<IStop>(
                    1, 
                    new Deleter<IStop>(new NamedCollection<IStop>(collection, collectionName), collectionFilter), 
                    new ConsoleMessageWriter("Conditions do not specify one record uniquely."), 
                    collection, 
                    collectionFilter
                ));
            }
        }

        private class DeleteBytebus : BytebusSelector
        {
            public DeleteBytebus()
            {
                subcommands.Add(new IdFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new EngineFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new RecordNumberVerifier<IBytebus>(
                    1, 
                    new Deleter<IBytebus>(new NamedCollection<IBytebus>(collection, collectionName), collectionFilter), 
                    new ConsoleMessageWriter("Conditions do not specify one record uniquely."), 
                    collection, 
                    collectionFilter
                ));
            }
        }

        private class DeleteTram : TramSelector
        {
            public DeleteTram()
            {
                subcommands.Add(new IdFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new CarsNumberFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new RecordNumberVerifier<ITram>(
                    1, 
                    new Deleter<ITram>(new NamedCollection<ITram>(collection, collectionName), collectionFilter), 
                    new ConsoleMessageWriter("Conditions do not specify one record uniquely."), 
                    collection, 
                    collectionFilter
                ));
            }
        }

        private class DeleteDriver : DriverSelector
        {
            public DeleteDriver()
            {
                subcommands.Add(new NameFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new SurnameFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new SeniorityFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new RecordNumberVerifier<IDriver>(
                    1, 
                    new Deleter<IDriver>(new NamedCollection<IDriver>(collection, collectionName), collectionFilter), 
                    new ConsoleMessageWriter("Conditions do not specify one record uniquely."), 
                    collection, 
                    collectionFilter
                ));
            }
        }
    }
}