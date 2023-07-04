using System;
using System.Collections.Generic;
using static BTM.CollectionUtils;

namespace BTM
{
    class KeywordConsumer : CommandBase
    {
        private string keyword;

        public KeywordConsumer(string keyword) : this(new List<CommandBase>(), keyword)
        { }
        public KeywordConsumer(CommandBase subcommand, string keyword) : this(new List<CommandBase>() { subcommand }, keyword)
        { }
        public KeywordConsumer(List<CommandBase> subcommands, string keyword) : base(subcommands)
        {
            this.keyword = keyword;
        }

        public override bool Check(string input)
        {
            return input.StartsWith(keyword) && (input.Length == keyword.Length || input[keyword.Length] == ' ');
        }

        public override string Process(string input)
        {
            Action();

            return input.Remove(0, keyword.Length);
        }

        public virtual void Action()
        { }
    }

    class CollectionSelector<BTMBase> : KeywordConsumer where BTMBase : IBTMBase
    {
        private ICollectionQuery<BTMBase> query;

        public CollectionSelector(List<CommandBase> subcommands, ICollectionQuery<BTMBase> query) : 
            base(subcommands, query.CollectionName)
        {
            this.query = query;
        }

        public CollectionSelector(CommandBase subcommand, ICollectionQuery<BTMBase> query) : 
            this(new List<CommandBase>() { subcommand }, query) 
        { }

        public CollectionSelector(ICollectionQuery<BTMBase> query) : 
            this(new List<CommandBase>(), query)
        { }

        public override void Action()
        {
            query.Filter.Predicates.Clear();
            query.EditActions.Actions.Clear();
        }
    }

    class ConsoleLineReader : CommandBase
    {
        public ConsoleLineReader(List<CommandBase> subcommands) : base(subcommands)
        { }

        public ConsoleLineReader(CommandBase subcommand) : base(new List<CommandBase>() { subcommand })
        { }

        public override bool Check(string input) => true;

        public override string Process(string input)
        {
            return Console.ReadLine();
        }
    }

    class ConsoleLineWriter : CommandBase
    {
        private string message;

        public ConsoleLineWriter(List<CommandBase> subcommands, string message) :
            base(subcommands)
        {
            this.message = message;
        }

        public ConsoleLineWriter(CommandBase subcommand, string message) : this(new List<CommandBase>() { subcommand }, message)
        { }

        public ConsoleLineWriter(string message) : this(new List<CommandBase>(), message)
        { }

        public override bool Check(string input) => true;

        public override string Process(string input)
        {
            Console.WriteLine(message);
            return input;
        }
    }

    class RecordNumberVerifier<BTMBase> : CommandBase where BTMBase : IBTMBase
    {
        private ICollectionQuery<BTMBase> selector;
        private List<CommandBase> happySubcommands, sadSubcommands;
        private int number;

        public RecordNumberVerifier(int number, List<CommandBase> happySubcommands, List<CommandBase> sadSubcommands, ICollectionQuery<BTMBase> selector)
        {
            this.number = number;
            this.selector = selector;
            this.happySubcommands = happySubcommands;
            this.sadSubcommands = sadSubcommands;
        }

        public RecordNumberVerifier(int number, CommandBase happySubcommand, CommandBase sadSubcommand, ICollectionQuery<BTMBase> selector) :
            this(number, new List<CommandBase>() { happySubcommand }, new List<CommandBase>() { sadSubcommand }, selector)
        { }

        public override bool Check(string input) => input == "";

        public override string Process(string input)
        {
            subcommands = CountIf(selector.Collection.First(), selector.Filter) == number ? happySubcommands : sadSubcommands;
            return input;
        }
    }

    class FilteredCollectionPrinter<BTMBase> : CommandBase where BTMBase : IBTMBase
    {
        private ICollectionQuery<BTMBase> selector;

        public FilteredCollectionPrinter(ICollectionQuery<BTMBase> selector)
        {
            this.selector = selector;
        }

        public override bool Check(string input) => input == "";

        public override string Process(string input)
        {
            ForEach(selector.Collection.First(), new ActionIf<BTMBase>(new Print<BTMBase>(), selector.Filter));
            return input;
        }
    }
}
