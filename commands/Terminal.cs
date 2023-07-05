using System;
using System.Collections.Generic;

namespace BTM
{
    class Terminal
    {
        private bool work;
        private CommandBase commandRunner;
        private List<IExecutor> history, queue;
        private int historyIndex;
        private List<CommandBase> commands;

        public Terminal()
        {
            work = true;
            history = new List<IExecutor>();
            queue = new List<IExecutor>();
            historyIndex = 0;
            commands = new List<CommandBase>() 
            {
                new ListCommand(),
                new FindCommand(),
                new AddCommand(),
                new EditCommand(),
                new DeleteCommand(),
                new QueueCommand(this),
                new HistoryCommand(this.History),
                new UndoCommand(this),
                new RedoCommand(this),
                new ExportCommand(this.History),
                new LoadCommand(this),
                new DescCommand(), 
                new ExitCommand(this)
            };
            commands.Add(new HelpCommand(commands));
            commandRunner = new CommandRunner(commands);
        }

        public void Run(bool executeImmediately)
        {
            IExecutor executor;
            while (work)
            {
                Console.ForegroundColor = executeImmediately ? ConsoleColor.Green : ConsoleColor.Yellow;
                Console.Write($"BTM ({(executeImmediately ? 'i' : 'q')}) > ");
                Console.ResetColor();
                string input = Console.ReadLine();

                if (input == null) return;

                if ((executor = commandRunner.Execute(input)) != null)
                {
                    queue.Add(executor);
                    history.RemoveRange(historyIndex, history.Count - historyIndex);
                }

                if (executeImmediately) ExecuteQueue();
            }
        }

        public void Close()
        {
            work = false;
        }

        public void ExecuteQueue()
        {
            foreach (IExecutor enqueued in queue)
            {
                enqueued.Do();
                history.Add(enqueued);
                historyIndex++;
            }
            queue.Clear();
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

        private class CommandRunner : CommandBase
        {
            public CommandRunner(List<CommandBase> subcommands) :
                base(subcommands)
            { }

            public override bool Check(string input) => input != null;

            public override string Process(string input) => input;
        }

        private class HelpCommand : KeywordConsumer, IHelpable
        {
            public HelpCommand(List<CommandBase> commands) : 
                base("help")
            {
                foreach (ICommand command in commands)
                    if (command is IHelpable) 
                        subcommands.Add(new HelpDisplayer(command as IHelpable));

                subcommands.Add(new HelpDisplayer(this));
            }

            public string HelpKeyword => "";

            public string Help => 
@"USAGE: help <command>

command: 
    desc        - show available attributes for entity
    list        - show all existing entities
    find        - filter existing entities
    add         - (q) add new entity
    delete      - (q) delete existing entity
    edit        - (q) edit existing entity

    queue       - manage queue
    history     - show history
    export      - export command history to file
    load        - load commands from file
    undo        - undo
    redo        - redo

    exit        - closes terminal

Terminal can run in one of two modes: *immediate* or *queue*.

In queue mode commands marked with (q) are stored in queue after
being called. They are executed and stored in history only after 
calling *queue commit*. 

In immediate mode enqueued commands are executed immediately and
removed from the queue. 
";

            private class HelpDisplayer : KeywordConsumer
            {
                private IHelpable command;

                public HelpDisplayer(IHelpable command) : 
                    base(command.HelpKeyword)
                {
                    this.command = command;
                }

                public override void Action()
                {
                    Console.WriteLine(command.Help);
                }
            }
        }

        private class ExitCommand : KeywordConsumer, IHelpable
        {
            private Terminal terminal;

            public ExitCommand(Terminal terminal) : base("exit")
            {
                this.terminal = terminal;
            }

            public string HelpKeyword => "exit";

            public string Help => 
@"USAGE: exit

Closes BTM terminal.
";

            public override void Action()
            {
                terminal.Close();
            }
        }
    }
}
