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
                Console.ForegroundColor = executeImmediately ? ConsoleColor.Green : ConsoleColor.Yellow;
                Console.Write("BTM > ");
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

        private class ExitCommand : KeywordConsumer
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
    }
}
