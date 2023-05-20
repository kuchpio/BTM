
namespace BTM 
{
    class Terminal
    {
        private bool work;
        private CommandExecutor commandExecutor;

        public Terminal()
        {
            work = true;
            commandExecutor = new CommandExecutor(this);
        }

        public void Run()
        {
            while (work)
            {
                Console.Write("BTM> ");
                string userInput = Console.ReadLine();

                if (commandExecutor.Check(userInput))
                    commandExecutor.Execute(userInput).Execute();
            }
        }

        public void Close()
        {
            work = false;
        }
    }

    class CommandExecutor : CommandBase
    {
        public CommandExecutor(Terminal terminal) : 
            base(new List<CommandBase>() { 
                new ListCommand(), 
                new FindCommand(), 
                new AddCommand(),
                new EditCommand(),
                new DeleteCommand(), 
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
                subcommands.Add(new FindExecutorReturner<ILine>(collection));
            }
        }

        private class DisplayStop : StopSelector
        {
            public DisplayStop()
            {
                subcommands.Add(new FindExecutorReturner<IStop>(collection));
            }
        }

        private class DisplayBytebus : BytebusSelector
        {
            public DisplayBytebus()
            {
                subcommands.Add(new FindExecutorReturner<IBytebus>(collection));
            }
        }

        private class DisplayTram : TramSelector
        {
            public DisplayTram()
            {
                subcommands.Add(new FindExecutorReturner<ITram>(collection));
            }
        }

        private class DisplayVehicle : VehicleSelector
        {
            public DisplayVehicle()
            {
                subcommands.Add(new FindExecutorReturner<IVehicle>(collection));
            }
        }

        private class DisplayDriver : DriverSelector
        {
            public DisplayDriver()
            {
                subcommands.Add(new FindExecutorReturner<IDriver>(collection));
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
                subcommands.Add(new FindExecutorReturner<ILine>(collection, collectionFilter));
            }
        }

        private class DisplayFilteredStop : StopSelector
        {
            public DisplayFilteredStop()
            {
                subcommands.Add(new IdFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new NameFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new TypeFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new FindExecutorReturner<IStop>(collection, collectionFilter));
            }
        }

        private class DisplayFilteredBytebus : BytebusSelector
        {
            public DisplayFilteredBytebus()
            {
                subcommands.Add(new IdFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new EngineFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new FindExecutorReturner<IBytebus>(collection, collectionFilter));
            }
        }

        private class DisplayFilteredTram : TramSelector
        {
            public DisplayFilteredTram()
            {
                subcommands.Add(new IdFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new CarsNumberFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new FindExecutorReturner<ITram>(collection, collectionFilter));
            }
        }

        private class DisplayFilteredVehicle : VehicleSelector
        {
            public DisplayFilteredVehicle()
            {
                subcommands.Add(new IdFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new FindExecutorReturner<IVehicle>(collection, collectionFilter));
            }
        }

        private class DisplayFilteredDriver : DriverSelector
        {
            public DisplayFilteredDriver()
            {
                subcommands.Add(new NameFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new SurnameFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new SeniorityFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new FindExecutorReturner<IDriver>(collection, collectionFilter));
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
                            new BuilderDescriptor(collection, new LineBaseBuilder()), 
                            "`numberDec`: numeric, `numberHex`: string, `commonName`: string"
                        ), "base"
                    ));

                subcommands.Add(
                    new KeywordConsumer(
                        new ConsoleMessageWriter(
                            new BuilderDescriptor(collection, new LineTextBuilder()), 
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
                            new BuilderDescriptor(collection, new StopBaseBuilder()), 
                            "`id`: numeric, `name`: string, `type`: string"
                        ), "base"
                    ));

                subcommands.Add(
                    new KeywordConsumer(
                        new ConsoleMessageWriter(
                            new BuilderDescriptor(collection, new StopTextBuilder()), 
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
                            new BuilderDescriptor(collection, new BytebusBaseBuilder()), 
                            "`id`: numeric, `engine`: string"
                        ), "base"
                    ));

                subcommands.Add(
                    new KeywordConsumer(
                        new ConsoleMessageWriter(
                            new BuilderDescriptor(collection, new BytebusTextBuilder()), 
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
                            new BuilderDescriptor(collection, new TramBaseBuilder()), 
                            "`id`: numeric, `carsNumber`: numeric"
                        ), "base"
                    ));

                subcommands.Add(
                    new KeywordConsumer(
                        new ConsoleMessageWriter(
                            new BuilderDescriptor(collection, new TramTextBuilder()), 
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
                            new BuilderDescriptor(collection, new DriverBaseBuilder()), 
                            "`name`: string, `surname`: string, `seniority`: numeric"
                        ), "base"
                    ));

                subcommands.Add(
                    new KeywordConsumer(
                        new ConsoleMessageWriter(
                            new BuilderDescriptor(collection, new DriverTextBuilder()), 
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
            public EditConfirmer(IBTMCollection<BTMBase> collection, All<BTMBase> filters, ActionSequence<BTMBase> actionSequence) : 
                base(new ConsoleMessageWriter(new List<CommandBase>() {
                    new EditExecutorReturner<BTMBase>(collection, actionSequence, filters)
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
                    new EditDescriptor(collection, collectionFilter, new ActionSequence<ILine>()), 
                    new ConsoleMessageWriter(new List<CommandBase>(), "Conditions do not specify one record uniquely."), 
                    collection, 
                    collectionFilter
                ));
            }

            private class EditDescriptor : ConsoleMessageWriter
            {
                public EditDescriptor(IBTMCollection<ILine> collection, All<ILine> filter, ActionSequence<ILine> setActions) : 
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
                    new EditDescriptor(collection, collectionFilter, new ActionSequence<IStop>()), 
                    new ConsoleMessageWriter(new List<CommandBase>(), "Conditions do not specify one record uniquely."), 
                    collection, 
                    collectionFilter
                ));
            }

            private class EditDescriptor : ConsoleMessageWriter
            {
                public EditDescriptor(IBTMCollection<IStop> collection, All<IStop> filter, ActionSequence<IStop> setActions) : 
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
                    new EditDescriptor(collection, collectionFilter, new ActionSequence<IBytebus>()), 
                    new ConsoleMessageWriter(new List<CommandBase>(), "Conditions do not specify one record uniquely."), 
                    collection, 
                    collectionFilter
                ));
            }

            private class EditDescriptor : ConsoleMessageWriter
            {
                public EditDescriptor(IBTMCollection<IBytebus> collection, All<IBytebus> filter, ActionSequence<IBytebus> setActions) : 
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
                    new EditDescriptor(collection, collectionFilter, new ActionSequence<ITram>()), 
                    new ConsoleMessageWriter(new List<CommandBase>(), "Conditions do not specify one record uniquely."), 
                    collection, 
                    collectionFilter
                ));
            }

            private class EditDescriptor : ConsoleMessageWriter
            {
                public EditDescriptor(IBTMCollection<ITram> collection, All<ITram> filter, ActionSequence<ITram> setActions) : 
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
                    new EditDescriptor(collection, collectionFilter, new ActionSequence<IVehicle>()), 
                    new ConsoleMessageWriter(new List<CommandBase>(), "Conditions do not specify one record uniquely."), 
                    collection, 
                    collectionFilter
                ));
            }

            private class EditDescriptor : ConsoleMessageWriter
            {
                public EditDescriptor(IBTMCollection<IVehicle> collection, All<IVehicle> filter, ActionSequence<IVehicle> setActions) : 
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
                    new EditDescriptor(collection, collectionFilter, new ActionSequence<IDriver>()), 
                    new ConsoleMessageWriter(new List<CommandBase>(), "Conditions do not specify one record uniquely."), 
                    collection, 
                    collectionFilter
                ));
            }

            private class EditDescriptor : ConsoleMessageWriter
            {
                public EditDescriptor(IBTMCollection<IDriver> collection, All<IDriver> filter, ActionSequence<IDriver> setActions) : 
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
            public Deleter(IBTMCollection<BTMBase> collection, All<BTMBase> filter) : base(new List<CommandBase>() {
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
                    new Deleter<ILine>(collection, collectionFilter), 
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
                    new Deleter<IStop>(collection, collectionFilter), 
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
                    new Deleter<IBytebus>(collection, collectionFilter), 
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
                    new Deleter<ITram>(collection, collectionFilter), 
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
                    new Deleter<IDriver>(collection, collectionFilter), 
                    new ConsoleMessageWriter("Conditions do not specify one record uniquely."), 
                    collection, 
                    collectionFilter
                ));
            }
        }
    }
}