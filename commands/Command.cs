using System;
using System.Collections.Generic;

namespace BTM
{
    interface ICommand
    {
        IExecutor Execute(string input);
    }

    interface IHelpable : ICommand
    {
        string HelpKeyword { get; }
        string Help { get; }
    }

    abstract class CommandBase : ICommand
    {
        protected List<CommandBase> subcommands;

        public CommandBase(List<CommandBase> subcommands)
        {
            this.subcommands = subcommands;
        }

        public CommandBase() : this(new List<CommandBase>())
        { }
        
        public abstract bool Check(string input);

        public abstract string Process(string input);

        public virtual IExecutor Execute(string input)
        {
            input = Process(input);

            if (subcommands.Count == 0 || input == null) return null;

            input = input.Trim();

            foreach (CommandBase command in subcommands)
            {
                if (command.Check(input))
                {
                    return command.Execute(input);
                }
            }
            
            Console.Error.WriteLine($"Unknown command syntax near: \n`{input}`\nFor help see `help`");
            return null;
        }
    }

    interface IExecutor
    {
        void Do();
        void Undo();
    }

    abstract class ExecutorFactory : CommandBase
    {
        public override bool Check(string input) => input == "";

        public override string Process(string input) => input;
    }
}
