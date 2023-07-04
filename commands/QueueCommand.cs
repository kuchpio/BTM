using System;
using System.Collections.Generic;

namespace BTM
{
    class QueueCommand : KeywordConsumer
    {
        public QueueCommand(Terminal terminal) : base(new List<CommandBase>() {
            new QueueDismiss(terminal.Queue), 
            new QueueCommit(terminal), 
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
            private Terminal terminal;
            
            public QueueCommit(Terminal terminal) : base("commit")
            { 
                this.terminal = terminal;
            }

            public override void Action() => terminal.ExecuteQueue();
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
}
