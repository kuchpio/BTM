using System;
using System.Collections.Generic;

namespace BTM
{
    class HistoryCommand : KeywordConsumer
    {
        private List<IExecutor> history;

        public HistoryCommand(List<IExecutor> history) : base("history")
        {
            this.history = history;
        }

        public override void Action()
        {
            Console.WriteLine(string.Join("\n- - - - - - - - - - - - - - -\n", history));
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
}
