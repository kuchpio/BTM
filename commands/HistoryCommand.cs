using System;
using System.Collections.Generic;

namespace BTM
{
    class HistoryCommand : KeywordConsumer, IHelpable
    {
        private List<IExecutor> history;

        public HistoryCommand(List<IExecutor> history) : base("history")
        {
            this.history = history;
        }

        public string HelpKeyword => "history";

        public string Help => 
@"USAGE: history

Shows list of commands that were executed. Only queueable (q)
commands are stored in history.
";

        public override void Action()
        {
            Console.WriteLine(string.Join("\n- - - - - - - - - - - - - - -\n", history));
        }
    }

    class UndoCommand : KeywordConsumer, IHelpable
    {
        private Terminal terminal;

        public UndoCommand(Terminal terminal) : base("undo")
        {
            this.terminal = terminal;
        }

        public string HelpKeyword => "undo";

        public string Help => 
@"USAGE: undo

Reverses previously executed queueable (q) command. 
";

        public override void Action() => terminal.Undo();
    }

    class RedoCommand : KeywordConsumer, IHelpable
    {
        private Terminal terminal;

        public RedoCommand(Terminal terminal) : base("redo")
        {
            this.terminal = terminal;
        }

        public string HelpKeyword => "redo";

        public string Help => 
@"USAGE: redo

Executes previously undone queueable (q) command. 
";

        public override void Action() => terminal.Redo();
    }
}
