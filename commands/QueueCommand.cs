using System;
using System.Collections.Generic;

namespace BTM
{
    class QueueCommand : KeywordConsumer, IHelpable
    {
        public QueueCommand(Terminal terminal) : base(new List<CommandBase>() {
            new QueueDismiss(terminal.Queue), 
            new QueueCommit(terminal), 
            new QueuePrint(terminal.Queue), 
            new ExportCommand(terminal.Queue), 
            new LoadCommand(terminal)
        }, "queue")
        { }

        public string HelpKeyword => "queue";

        public string Help => 
@"USAGE: queue <subcommand>

subcommand:
    commit      - executes all commands stored in queue
    dismiss     - clears the queue without executing commands
    print       - shows commands stored in queue

    export      - exports commands stored in queue to file
    load        - loads commands from file to queue

Command is stored in queue only when it is queueable (q) and
BTM terminal runs in *queue* mode. 

Command *queue export* (resp. *queue load*) works similar to 
*export* (resp. *load*), but it exports commands from queue
(resp. loads command to queue) instead of history. 
";

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
