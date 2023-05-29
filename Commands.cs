using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace BTM 
{
    class Terminal
    {
        private bool work;
        private CommandBase commandRunner;
        private List<IExecutor> history, queue;
        private int historyIndex;

        public Terminal()
        {
            work = true;
            history = new List<IExecutor>();
            queue = new List<IExecutor>();
            historyIndex = 0;
            commandRunner = new CommandRunner(this);
        }

        public void Run(bool executeImmediately)
        {
            IExecutor executor;
            while (work)
            {
                Console.Write("BTM> ");
                string input = Console.ReadLine();

                if (input == null) return;

                if ((executor = commandRunner.Execute(input)) != null)
                {
                    queue.Add(executor);
                    history.RemoveRange(historyIndex, history.Count - historyIndex);
                }

                if (executeImmediately)
                {
                    foreach (IExecutor enqueued in queue)
                    {
                        enqueued.Do();
                        history.Add(enqueued);
                        historyIndex++;
                    }
                    queue.Clear();
                }
            }
        }

        public void Close()
        {
            work = false;
        }

        public List<IExecutor> History => history;
        public List<IExecutor> Queue => queue;

        public void Undo()
        {
            if (historyIndex <= 0) return;
            history[--historyIndex].Undo();
        }

        public void Redo()
        {
            if (historyIndex >= History.Count) return;
            history[historyIndex++].Do();
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
                new HistoryCommand(terminal.History),
                new UndoCommand(terminal), 
                new RedoCommand(terminal), 
                new ExportCommand(terminal.History),
                new LoadCommand(terminal), 
                new ExitCommand(terminal) 
            })
        { }

        public override bool Check(string input) => input != null;

        public override string Process(string input) => input;
    }

    class QueueCommand : KeywordConsumer
    {
        public QueueCommand(Terminal terminal) : base(new List<CommandBase>() {
            new QueueDismiss(terminal.Queue), 
            new QueueCommit(terminal.Queue), 
            new QueuePrint(terminal.Queue), 
            new ExportCommand(terminal.Queue), 
            new LoadCommand(terminal)
        }, "queue")
        { }

        private class QueueDismiss : KeywordConsumer
        {
            private List<IExecutor> queue;
            
            public QueueDismiss(List<IExecutor> queue) : base("dismiss")
            { 
                this.queue = queue;
            }

            public override void Action() => queue.Clear();
        }

        private class QueueCommit : KeywordConsumer
        {
            private List<IExecutor> queue;
            
            public QueueCommit(List<IExecutor> queue) : base("commit")
            { 
                this.queue = queue;
            }

            public override void Action()
            {
                foreach (IExecutor executor in queue)
                    executor.Do();

                queue.Clear();
            }
        }

        private class QueuePrint : KeywordConsumer
        {
            private List<IExecutor> queue;
            
            public QueuePrint(List<IExecutor> queue) : base("print")
            { 
                this.queue = queue;
            }

            public override void Action()
            {
                Console.WriteLine(string.Join("\n", queue));
            }
        }
    }

    class HistoryCommand : KeywordConsumer
    {
        private List<IExecutor> history;
        
        public HistoryCommand(List<IExecutor> history) : base("history")
        { 
            this.history = history;
        }

        public override void Action()
        {
            Console.WriteLine(string.Join("\n", history));
        }
    }

    class UndoCommand : KeywordConsumer
    {
        private Terminal terminal;

        public UndoCommand(Terminal terminal) : base("undo")
        {
            this.terminal = terminal;
        }

        public override void Action() => terminal.Undo();
    }

    class RedoCommand : KeywordConsumer
    {
        private Terminal terminal;

        public RedoCommand(Terminal terminal) : base("redo")
        {
            this.terminal = terminal;
        }

        public override void Action() => terminal.Redo();
    }

    class ExportCommand : KeywordConsumer
    {
        public ExportCommand(List<IExecutor> executors) : base(new List<CommandBase>() {
            new TextExporter(executors), 
            new XMLExporter(executors),
        }, "export")
        { }

        private class TextExporter : CommandBase
        {
            private List<IExecutor> executors;

            public TextExporter(List<IExecutor> executors)
            {
                this.executors = executors;
            }

            public override bool Check(string input)
            {
                int spaceIndex = input.IndexOf(' ');
                return spaceIndex > 0 && input.Substring(spaceIndex).Trim().StartsWith("plaintext");
            }

            public override string Process(string input)
            {
                string filename = input.Substring(0, input.IndexOf(' '));
                File.WriteAllText(filename, ExecutorsToString());
                return "";
            }

            private string ExecutorsToString()
            {
                return string.Join("\n", executors);
            }
        }

        private class XMLExporter : CommandBase
        {
            private List<IExecutor> executors;

            public XMLExporter(List<IExecutor> executors)
            {
                this.executors = executors;
            }

            public override bool Check(string input)
            {
                int spaceIndex = input.IndexOf(' ');
                if (input.Length > 0 && spaceIndex < 0) return true;
                return spaceIndex > 0 && input.Substring(spaceIndex).Trim().StartsWith("xml");
            }

            public override string Process(string input)
            {
                int spaceIndex = input.IndexOf(' ');
                string filename = spaceIndex < 0 ? input : input.Substring(0, spaceIndex);
                File.WriteAllText(filename, ExecutorsToString());
                return "";
            }

            private string ExecutorsToString()
            {
                return "<?xml version='1.0' encoding='utf-8'?>\n<queue>\n\t<command>\n" + 
                    string.Join(
                        "\n\t</command>\n\t<command>\n", 
                        executors.Select((IExecutor executor, int _) => 
                            "\t\t<line content='" + 
                            string.Join(
                                "'/>\n\t\t<line content='", 
                                executor.ToString().Split('\n', StringSplitOptions.RemoveEmptyEntries)
                            ) + 
                            "'/>"
                        )) + 
                    "\n\t</command>\n</queue>";
            }
        }
    }

    class LoadCommand : KeywordConsumer
    {
        public LoadCommand(Terminal terminal) : base(new FileLoader(terminal), "load")
        { }

        private class FileLoader : CommandBase
        {
            private Terminal terminal;
            
            public FileLoader(Terminal terminal)
            { 
                this.terminal = terminal;
            }

            public override bool Check(string input)
            {
                return input != "";
            }

            public override string Process(string input)
            {
                string text;
                try 
                {
                    text = File.ReadAllText(input);
                }
                catch
                {
                    Console.Error.Write($"Could not read file {input}.");
                    return "";
                }

                if (text.Trim().StartsWith("<?xml"))
                {
                    List<string> lines = new List<string>();
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(text);
                    foreach (XmlNode node in doc.SelectNodes("//line"))
                    {
                        lines.Add(node.Attributes.GetNamedItem("content").InnerText);
                    }
                    text = string.Join("\n", lines);
                }

                TextReader stdin = Console.In;
                TextWriter stdout = Console.Out;

                using (TextReader filestream = new StringReader(text))
                using (TextWriter writer = new StringWriter())
                {
                    Console.SetIn(filestream);
                    Console.SetOut(writer);

                    terminal.Run(false);
                }

                Console.SetIn(stdin);
                Console.SetOut(stdout);
                
                return "";
            }
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
                // subcommands.Add(new FindExecutorFactory<ILine>(collection, collectionFilter, collectionName));
                subcommands.Add(new FilteredCollectionPrinter<ILine>(collection, collectionFilter, collectionName));
            }
        }

        private class DisplayStop : StopSelector
        {
            public DisplayStop()
            {
                // subcommands.Add(new FindExecutorFactory<IStop>(collection, collectionFilter, collectionName));
                subcommands.Add(new FilteredCollectionPrinter<IStop>(collection, collectionFilter, collectionName));
            }
        }

        private class DisplayBytebus : BytebusSelector
        {
            public DisplayBytebus()
            {
                // subcommands.Add(new FindExecutorFactory<IBytebus>(collection, collectionFilter, collectionName));
                subcommands.Add(new FilteredCollectionPrinter<IBytebus>(collection, collectionFilter, collectionName));
            }
        }

        private class DisplayTram : TramSelector
        {
            public DisplayTram()
            {
                // subcommands.Add(new FindExecutorFactory<ITram>(collection, collectionFilter, collectionName));
                subcommands.Add(new FilteredCollectionPrinter<ITram>(collection, collectionFilter, collectionName));
            }
        }

        private class DisplayVehicle : VehicleSelector
        {
            public DisplayVehicle()
            {
                // subcommands.Add(new FindExecutorFactory<IVehicle>(collection, collectionFilter, collectionName));
                subcommands.Add(new FilteredCollectionPrinter<IVehicle>(collection, collectionFilter, collectionName));
            }
        }

        private class DisplayDriver : DriverSelector
        {
            public DisplayDriver()
            {
                // subcommands.Add(new FindExecutorFactory<IDriver>(collection, collectionFilter, collectionName));
                subcommands.Add(new FilteredCollectionPrinter<IDriver>(collection, collectionFilter, collectionName));
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
                // subcommands.Add(new FindExecutorFactory<ILine>(collection, collectionFilter, collectionName));
                subcommands.Add(new FilteredCollectionPrinter<ILine>(collection, collectionFilter, collectionName));
            }
        }

        private class DisplayFilteredStop : StopSelector
        {
            public DisplayFilteredStop()
            {
                subcommands.Add(new IdFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new NameFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new TypeFilterAdder(subcommands, collectionFilter));
                // subcommands.Add(new FindExecutorFactory<IStop>(collection, collectionFilter, collectionName));
                subcommands.Add(new FilteredCollectionPrinter<IStop>(collection, collectionFilter, collectionName));
            }
        }

        private class DisplayFilteredBytebus : BytebusSelector
        {
            public DisplayFilteredBytebus()
            {
                subcommands.Add(new IdFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new EngineFilterAdder(subcommands, collectionFilter));
                // subcommands.Add(new FindExecutorFactory<IBytebus>(collection, collectionFilter, collectionName));
                subcommands.Add(new FilteredCollectionPrinter<IBytebus>(collection, collectionFilter, collectionName));
            }
        }

        private class DisplayFilteredTram : TramSelector
        {
            public DisplayFilteredTram()
            {
                subcommands.Add(new IdFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new CarsNumberFilterAdder(subcommands, collectionFilter));
                // subcommands.Add(new FindExecutorFactory<ITram>(collection, collectionFilter, collectionName));
                subcommands.Add(new FilteredCollectionPrinter<ITram>(collection, collectionFilter, collectionName));
            }
        }

        private class DisplayFilteredVehicle : VehicleSelector
        {
            public DisplayFilteredVehicle()
            {
                subcommands.Add(new IdFilterAdder(subcommands, collectionFilter));
                // subcommands.Add(new FindExecutorFactory<IVehicle>(collection, collectionFilter, collectionName));
                subcommands.Add(new FilteredCollectionPrinter<IVehicle>(collection, collectionFilter, collectionName));
            }
        }

        private class DisplayFilteredDriver : DriverSelector
        {
            public DisplayFilteredDriver()
            {
                subcommands.Add(new NameFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new SurnameFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new SeniorityFilterAdder(subcommands, collectionFilter));
                // subcommands.Add(new FindExecutorFactory<IDriver>(collection, collectionFilter, collectionName));
                subcommands.Add(new FilteredCollectionPrinter<IDriver>(collection, collectionFilter, collectionName));
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

        private interface IBuilderAddersFactory<BTMBase, BTMBaseBuilder> 
            where BTMBase : IBTMBase
            where BTMBaseBuilder : IBTMBuilder<BTMBase>
        {
            IEnumerable<CommandBase> Create(List<CommandBase> subcommands, BTMBaseBuilder builder);
        }

        private class AddDecider<BTMBase, BTMBaseBuilder, BuilderAddersFactory> : KeywordConsumer 
            where BTMBase : IBTMBase
            where BTMBaseBuilder : IBTMBuilder<BTMBase>
            where BuilderAddersFactory : IBuilderAddersFactory<BTMBase, BTMBaseBuilder>, new()
        {
            public AddDecider(IBTMCollection<BTMBase> collection, BTMBaseBuilder builder, string keyword, string fieldsDescription) : 
                base(new ConsoleLineWriter(
                        new BuilderDescriber(collection, builder), 
                        fieldsDescription
                ), keyword)
            { }

            private class BuilderDescriber : CommandBase
            {
                private IBTMBuilder<BTMBase> builder;
                
                public BuilderDescriber(IBTMCollection<BTMBase> collection, BTMBaseBuilder builder)
                {
                    this.builder = builder;
                    List<CommandBase> readerSubcommands = new List<CommandBase>();
                    readerSubcommands.Add(new KeywordConsumer(new ConsoleLineWriter(
                        new AddExecutorFactory<BTMBase>(collection, builder), 
                        "Object registered for addition."
                    ), "done"));
                    readerSubcommands.Add(new KeywordConsumer(new ConsoleLineWriter("Object creation abandoned."), "exit"));
                    readerSubcommands.AddRange(new BuilderAddersFactory().Create(subcommands, builder));
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

        private class AddLine : LineSelector
        {
            public AddLine()
            {
                subcommands.Add(new AddDecider<ILine, ILineBuilder, BuilderAddersFactory>(
                    collection, 
                    new LineBuilderLogger(new LineBaseBuilder()), 
                    "base", 
                    "`numberDec`: numeric, `numberHex`: string, `commonName`: string"
                ));

                subcommands.Add(new AddDecider<ILine, ILineBuilder, BuilderAddersFactory>(
                    collection, 
                    new LineBuilderLogger(new LineTextBuilder()), 
                    "secondary", 
                    "`numberDec`: numeric, `numberHex`: string, `commonName`: string"
                ));
            }

            private class BuilderAddersFactory : IBuilderAddersFactory<ILine, ILineBuilder>
            {
                public IEnumerable<CommandBase> Create(List<CommandBase> subcommands, ILineBuilder builder)
                {
                    return new List<CommandBase>()
                    {
                        new NumberDecBuilderAdder(subcommands, builder), 
                        new NumberHexBuilderAdder(subcommands, builder), 
                        new CommonNameBuilderAdder(subcommands, builder)
                    };
                }
            }
        }

        private class AddStop : StopSelector
        {
            public AddStop()
            {
                subcommands.Add(new AddDecider<IStop, IStopBuilder, BuilderAddersFactory>(
                    collection, 
                    new StopBuilderLogger(new StopBaseBuilder()), 
                    "base", 
                    "`id`: numeric, `name`: string, `type`: string"
                ));

                subcommands.Add(new AddDecider<IStop, IStopBuilder, BuilderAddersFactory>(
                    collection, 
                    new StopBuilderLogger(new StopTextBuilder()), 
                    "secondary", 
                    "`id`: numeric, `name`: string, `type`: string"
                ));
            }

            private class BuilderAddersFactory : IBuilderAddersFactory<IStop, IStopBuilder>
            {
                public IEnumerable<CommandBase> Create(List<CommandBase> subcommands, IStopBuilder builder)
                {
                    return new List<CommandBase>()
                    {
                        new IdBuilderAdder(subcommands, builder), 
                        new NameBuilderAdder(subcommands, builder), 
                        new TypeBuilderAdder(subcommands, builder)
                    };
                }
            }
        }

        private class AddBytebus : BytebusSelector
        {
            public AddBytebus()
            {
                subcommands.Add(new AddDecider<IBytebus, IBytebusBuilder, BuilderAddersFactory>(
                    collection, 
                    new BytebusBuilderLogger(new BytebusBaseBuilder()), 
                    "base", 
                    "`id`: numeric, `engine`: string"
                ));

                subcommands.Add(new AddDecider<IBytebus, IBytebusBuilder, BuilderAddersFactory>(
                    collection, 
                    new BytebusBuilderLogger(new BytebusTextBuilder()), 
                    "secondary", 
                    "`id`: numeric, `engine`: string"
                ));
            }

            private class BuilderAddersFactory : IBuilderAddersFactory<IBytebus, IBytebusBuilder>
            {
                public IEnumerable<CommandBase> Create(List<CommandBase> subcommands, IBytebusBuilder builder)
                {
                    return new List<CommandBase>()
                    {
                        new IdBuilderAdder(subcommands, builder), 
                        new EngineBuilderAdder(subcommands, builder)
                    };
                }
            }
        }

        private class AddTram : TramSelector
        {
            public AddTram()
            {
                subcommands.Add(new AddDecider<ITram, ITramBuilder, BuilderAddersFactory>(
                    collection, 
                    new TramBuilderLogger(new TramBaseBuilder()), 
                    "base", 
                    "`id`: numeric, `carsNumber`: numeric"
                ));

                subcommands.Add(new AddDecider<ITram, ITramBuilder, BuilderAddersFactory>(
                    collection, 
                    new TramBuilderLogger(new TramTextBuilder()), 
                    "secondary", 
                    "`id`: numeric, `carsNumber`: numeric"
                ));
            }

            private class BuilderAddersFactory : IBuilderAddersFactory<ITram, ITramBuilder>
            {
                public IEnumerable<CommandBase> Create(List<CommandBase> subcommands, ITramBuilder builder)
                {
                    return new List<CommandBase>()
                    {
                        new IdBuilderAdder(subcommands, builder), 
                        new CarsNumberBuilderAdder(subcommands, builder),
                    };
                }
            }
        }

        private class AddDriver : DriverSelector
        {
            public AddDriver()
            {
                subcommands.Add(new AddDecider<IDriver, IDriverBuilder, BuilderAddersFactory>(
                    collection, 
                    new DriverBuilderLogger(new DriverBaseBuilder()), 
                    "base", 
                    "`name`: string, `surname`: string, `seniority`: numeric"
                ));

                subcommands.Add(new AddDecider<IDriver, IDriverBuilder, BuilderAddersFactory>(
                    collection, 
                    new DriverBuilderLogger(new DriverTextBuilder()), 
                    "secondary", 
                    "`name`: string, `surname`: string, `seniority`: numeric"
                ));
            }
            
            private class BuilderAddersFactory : IBuilderAddersFactory<IDriver, IDriverBuilder>
            {
                public IEnumerable<CommandBase> Create(List<CommandBase> subcommands, IDriverBuilder builder)
                {
                    return new List<CommandBase>()
                    {
                        new NameBuilderAdder(subcommands, builder), 
                        new SurnameBuilderAdder(subcommands, builder), 
                        new SeniorityBuilderAdder(subcommands, builder),
                    };
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

        private interface ISetterAddersFactory<BTMBase> where BTMBase : class, IBTMBase
        {
            IEnumerable<CommandBase> Create(List<CommandBase> subcommands, ActionSequence<BTMBase> setActions);
        }

        private class EditDecider<BTMBase, SetterAddersFactory, CopyAction> : RecordNumberVerifier<BTMBase> 
            where BTMBase : class, IBTMBase
            where SetterAddersFactory : ISetterAddersFactory<BTMBase>, new()
            where CopyAction : IBinaryAction<BTMBase>, new()
        {
            public EditDecider(IBTMCollection<BTMBase> collection, All<BTMBase> filter, ActionSequence<BTMBase> setActions, string collectionName, string fieldsDescription) : 
                base(
                    1, 
                    new EditDescriber(collection, filter, setActions, collectionName, fieldsDescription), 
                    new ConsoleLineWriter("Conditions do not specify one record uniquely."), 
                    collection, 
                    filter
                )
            { }

            private class EditDescriber : ConsoleLineWriter
            {
                public EditDescriber(IBTMCollection<BTMBase> collection, All<BTMBase> filter, ActionSequence<BTMBase> setActions, string collectionName, string fieldsDescription) : 
                    base($"[{fieldsDescription}]")
                {   
                    List<CommandBase> readerSubcommands = new List<CommandBase>();
                    readerSubcommands.Add(new KeywordConsumer(new ConsoleLineWriter(
                        new EditExecutorFactory<BTMBase>(collection, filter, setActions, collectionName, new CopyAction()), 
                        "Object succesfully registered for edit."
                    ), "done"));
                    readerSubcommands.Add(new KeywordConsumer(new ConsoleLineWriter("No object registered for edit."), "exit"));
                    readerSubcommands.AddRange(new SetterAddersFactory().Create(subcommands, setActions));
                    readerSubcommands.Add(new ConsoleLineWriter(subcommands, $"Previous line contains an error."));
                    subcommands.Add(new ConsoleLineReader(readerSubcommands));
                }
            }
        }

        private class EditLine : LineSelector
        {
            public EditLine()
            {
                subcommands.Add(new NumberDecFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new NumberHexFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new CommonNameFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new EditDecider<ILine, SetterAddersFactory, CopyAction>(
                    collection, 
                    collectionFilter, 
                    setActions, 
                    collectionName, 
                    "`numberDec`: numeric, `numberHex`: string, `commonName`: string"
                ));
            }

            private class SetterAddersFactory : ISetterAddersFactory<ILine>
            {
                public IEnumerable<CommandBase> Create(List<CommandBase> subcommands, ActionSequence<ILine> setActions)
                {
                    return new List<CommandBase>() {
                        new NumberDecSetterAdder(subcommands, setActions), 
                        new NumberHexSetterAdder(subcommands, setActions), 
                        new CommonNameSetterAdder(subcommands, setActions)
                    };
                }
            }

            private class CopyAction : IBinaryAction<ILine>
            {
                public void Eval(ILine src, ILine dest)
                {
                    dest.CopyFrom(src);
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
                subcommands.Add(new EditDecider<IStop, SetterAddersFactory, CopyAction>(
                    collection, 
                    collectionFilter, 
                    setActions, 
                    collectionName, 
                    "`id`: numeric, `name`: string, `type`: string"
                ));
            }

            private class SetterAddersFactory : ISetterAddersFactory<IStop>
            {
                public IEnumerable<CommandBase> Create(List<CommandBase> subcommands, ActionSequence<IStop> setActions)
                {
                    return new List<CommandBase>() {
                        new IdSetterAdder(subcommands, setActions), 
                        new NameSetterAdder(subcommands, setActions), 
                        new TypeSetterAdder(subcommands, setActions)
                    };
                }
            }

            private class CopyAction : IBinaryAction<IStop>
            {
                public void Eval(IStop src, IStop dest)
                {
                    dest.CopyFrom(src);
                }
            }
        }

        private class EditBytebus : BytebusSelector
        {
            public EditBytebus()
            {
                subcommands.Add(new IdFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new EngineFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new EditDecider<IBytebus, SetterAddersFactory, CopyAction>(
                    collection, 
                    collectionFilter, 
                    setActions, 
                    collectionName, 
                    "`id`: numeric, `engine`: string"
                ));
            }

            private class SetterAddersFactory : ISetterAddersFactory<IBytebus>
            {
                public IEnumerable<CommandBase> Create(List<CommandBase> subcommands, ActionSequence<IBytebus> setActions)
                {
                    return new List<CommandBase>() {
                        new IdSetterAdder(subcommands, setActions), 
                        new EngineSetterAdder(subcommands, setActions)
                    };
                }
            }

            private class CopyAction : IBinaryAction<IBytebus>
            {
                public void Eval(IBytebus src, IBytebus dest)
                {
                    dest.CopyFrom(src);
                }
            }
        }

        private class EditTram : TramSelector
        {
            public EditTram()
            {
                subcommands.Add(new IdFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new CarsNumberFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new EditDecider<ITram, SetterAddersFactory, CopyAction>(
                    collection, 
                    collectionFilter, 
                    setActions, 
                    collectionName, 
                    "`id`: numeric, `carsNumber`: numeric"
                ));
            }

            private class SetterAddersFactory : ISetterAddersFactory<ITram>
            {
                public IEnumerable<CommandBase> Create(List<CommandBase> subcommands, ActionSequence<ITram> setActions)
                {
                    return new List<CommandBase>() {
                        new IdSetterAdder(subcommands, setActions), 
                        new CarsNumberSetterAdder(subcommands, setActions)
                    };
                }
            }

            private class CopyAction : IBinaryAction<ITram>
            {
                public void Eval(ITram src, ITram dest)
                {
                    dest.CopyFrom(src);
                }
            }
        }

        private class EditVehicle : VehicleSelector
        {
            public EditVehicle()
            {
                subcommands.Add(new IdFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new EditDecider<IVehicle, SetterAddersFactory, CopyAction>(
                    collection, 
                    collectionFilter, 
                    setActions, 
                    collectionName, 
                    "`id`: numeric"
                ));
            }

            private class SetterAddersFactory : ISetterAddersFactory<IVehicle>
            {
                public IEnumerable<CommandBase> Create(List<CommandBase> subcommands, ActionSequence<IVehicle> setActions)
                {
                    return new List<CommandBase>() {
                        new IdSetterAdder(subcommands, setActions), 
                    };
                }
            }

            private class CopyAction : IBinaryAction<IVehicle>
            {
                public void Eval(IVehicle src, IVehicle dest)
                {
                    dest.CopyFrom(src);
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
                subcommands.Add(new EditDecider<IDriver, SetterAddersFactory, CopyAction>(
                    collection, 
                    collectionFilter, 
                    setActions, 
                    collectionName, 
                    "`name`: string, `surname`: string, `seniority`: numeric"
                ));
            }

            private class SetterAddersFactory : ISetterAddersFactory<IDriver>
            {
                public IEnumerable<CommandBase> Create(List<CommandBase> subcommands, ActionSequence<IDriver> setActions)
                {
                    return new List<CommandBase>() {
                        new NameSetterAdder(subcommands, setActions), 
                        new SurnameSetterAdder(subcommands, setActions), 
                        new SenioritySetterAdder(subcommands, setActions)
                    };
                }
            }

            private class CopyAction : IBinaryAction<IDriver>
            {
                public void Eval(IDriver src, IDriver dest)
                {
                    dest.CopyFrom(src);
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

        private class DeleteDecider<BTMBase> : RecordNumberVerifier<BTMBase> where BTMBase : class, IBTMBase
        {
            public DeleteDecider(IBTMCollection<BTMBase> collection, All<BTMBase> filter, string collectionName) :
                base(
                    1, 
                    new ConsoleLineWriter(
                        new DeleteExecutorFactory<BTMBase>(collection, filter, collectionName), 
                        "Object succesfully registered for deletion."
                    ),
                    new ConsoleLineWriter("Conditions do not specify one record uniquely."), 
                    collection, 
                    filter
                )
            { }
        }

        private class DeleteLine : LineSelector
        {
            public DeleteLine()
            {
                subcommands.Add(new NumberDecFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new NumberHexFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new CommonNameFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new DeleteDecider<ILine>(collection, collectionFilter, collectionName));
            }
        }

        private class DeleteStop : StopSelector
        {
            public DeleteStop()
            {
                subcommands.Add(new IdFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new NameFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new TypeFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new DeleteDecider<IStop>(collection, collectionFilter, collectionName));
            }
        }

        private class DeleteBytebus : BytebusSelector
        {
            public DeleteBytebus()
            {
                subcommands.Add(new IdFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new EngineFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new DeleteDecider<IBytebus>(collection, collectionFilter, collectionName));
            }
        }

        private class DeleteTram : TramSelector
        {
            public DeleteTram()
            {
                subcommands.Add(new IdFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new CarsNumberFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new DeleteDecider<ITram>(collection, collectionFilter, collectionName));
            }
        }

        private class DeleteDriver : DriverSelector
        {
            public DeleteDriver()
            {
                subcommands.Add(new NameFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new SurnameFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new SeniorityFilterAdder(subcommands, collectionFilter));
                subcommands.Add(new DeleteDecider<IDriver>(collection, collectionFilter, collectionName));
            }
        }
    }
}