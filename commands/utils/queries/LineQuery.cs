using System.Collections.Generic;

namespace BTM
{
    class LineQuery : CollectionQuery<ILine>, IBuildableCollectionQuery<ILine, ILineBuilder>
    {
        public LineQuery() : base(BTM.GetInstance().Lines, "line", "`numberDec`: numeric, `numberHex`: string, `commonName`: string")
        { }

        public IEnumerable<CommandBase> CreateBuilderAdders(List<CommandBase> subcommands, ILineBuilder builder)
        {
            return new List<CommandBase>()
            {
                new NumberDecBuilderAdder(subcommands, builder),
                new NumberHexBuilderAdder(subcommands, builder),
                new CommonNameBuilderAdder(subcommands, builder)
            };
        }

        public override IEnumerable<CommandBase> CreateFilterAdders(List<CommandBase> subcommands)
        {
            return new List<CommandBase>()
            {
                new NumberDecFilterAdder(subcommands, Filter.Predicates),
                new NumberHexFilterAdder(subcommands, Filter.Predicates),
                new CommonNameFilterAdder(subcommands, Filter.Predicates)
            };
        }

        public override IEnumerable<CommandBase> CreateSetterAdders(List<CommandBase> subcommands)
        {
            return new List<CommandBase>()
            {
                new NumberDecSetterAdder(subcommands, EditActions.Actions),
                new NumberHexSetterAdder(subcommands, EditActions.Actions),
                new CommonNameSetterAdder(subcommands, EditActions.Actions)
            };
        }

        private class NumberDecFilterAdder : FieldFilterAdder<ILine, int>
        {
            public NumberDecFilterAdder(List<CommandBase> subcommands, ICollection<IPredicate<ILine>> predicates) :
                base(subcommands, predicates, "numberdec")
            { }

            public override int GetValue(ILine item) => item.NumberDec;
        }

        private class NumberDecBuilderAdder : FieldBuilderAdder<ILine, int, ILineBuilder>
        {
            public NumberDecBuilderAdder(List<CommandBase> subcommands, ILineBuilder builder) :
                base(subcommands, builder, "numberdec")
            { }

            public override void BuildValue(ILineBuilder builder, int value) => builder.AddNumberDec(value);
        }

        private class NumberDecSetterAdder : FieldSetterAdder<ILine, int>
        {
            public NumberDecSetterAdder(List<CommandBase> subcommands, ICollection<IAction<ILine>> actions) :
                base(subcommands, actions, "numberdec")
            { }

            public override void SetValue(ILine item, int value) => item.NumberDec = value;
        }

        private class NumberHexFilterAdder : FieldFilterAdder<ILine, ParsableString>
        {
            public NumberHexFilterAdder(List<CommandBase> subcommands, ICollection<IPredicate<ILine>> predicates) :
                base(subcommands, predicates, "numberhex")
            { }

            public override ParsableString GetValue(ILine item) => item.NumberHex;
        }

        private class NumberHexBuilderAdder : FieldBuilderAdder<ILine, ParsableString, ILineBuilder>
        {
            public NumberHexBuilderAdder(List<CommandBase> subcommands, ILineBuilder builder) :
                base(subcommands, builder, "numberhex")
            { }

            public override void BuildValue(ILineBuilder builder, ParsableString value) => builder.AddNumberHex(value);
        }

        private class NumberHexSetterAdder : FieldSetterAdder<ILine, ParsableString>
        {
            public NumberHexSetterAdder(List<CommandBase> subcommands, ICollection<IAction<ILine>> actions) :
                base(subcommands, actions, "numberhex")
            { }

            public override void SetValue(ILine item, ParsableString value) => item.NumberHex = value;
        }

        private class CommonNameFilterAdder : FieldFilterAdder<ILine, ParsableString>
        {
            public CommonNameFilterAdder(List<CommandBase> subcommands, ICollection<IPredicate<ILine>> predicates) :
                base(subcommands, predicates, "commonname")
            { }

            public override ParsableString GetValue(ILine item) => item.CommonName;
        }

        private class CommonNameBuilderAdder : FieldBuilderAdder<ILine, ParsableString, ILineBuilder>
        {
            public CommonNameBuilderAdder(List<CommandBase> subcommands, ILineBuilder builder) :
                base(subcommands, builder, "commonname")
            { }

            public override void BuildValue(ILineBuilder builder, ParsableString value) => builder.AddCommonName(value);
        }

        private class CommonNameSetterAdder : FieldSetterAdder<ILine, ParsableString>
        {
            public CommonNameSetterAdder(List<CommandBase> subcommands, ICollection<IAction<ILine>> actions) :
                base(subcommands, actions, "commonname")
            { }

            public override void SetValue(ILine item, ParsableString value) => item.CommonName = value;
        }
    }
}
