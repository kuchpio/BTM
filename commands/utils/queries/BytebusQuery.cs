using System.Collections.Generic;

namespace BTM
{
    class BytebusQuery : CollectionQuery<IBytebus>, IBuildableCollectionQuery<IBytebus, IBytebusBuilder>
    {
        public BytebusQuery() : base(BTM.GetInstance().Buses, "bytebus", "`id`: numeric, `engine`: string")
        { }

        public IEnumerable<CommandBase> CreateBuilderAdders(List<CommandBase> subcommands, IBytebusBuilder builder)
        {
            return new List<CommandBase>()
            {
                new IdBuilderAdder(subcommands, builder),
                new EngineBuilderAdder(subcommands, builder),
            };
        }

        public override IEnumerable<CommandBase> CreateFilterAdders(List<CommandBase> subcommands)
        {
            return new List<CommandBase>()
            {
                new IdFilterAdder(subcommands, Filter.Predicates),
                new EngineFilterAdder(subcommands, Filter.Predicates),
            };
        }

        public override IEnumerable<CommandBase> CreateSetterAdders(List<CommandBase> subcommands)
        {
            return new List<CommandBase>()
            {
                new IdSetterAdder(subcommands, EditActions.Actions),
                new EngineSetterAdder(subcommands, EditActions.Actions),
            };
        }

        private class IdFilterAdder : FieldFilterAdder<IBytebus, int>
        {
            public IdFilterAdder(List<CommandBase> subcommands, ICollection<IPredicate<IBytebus>> predicates) :
                base(subcommands, predicates, "id")
            { }

            public override int GetValue(IBytebus item) => item.Id;
        }

        private class IdBuilderAdder : FieldBuilderAdder<IBytebus, int, IBytebusBuilder>
        {
            public IdBuilderAdder(List<CommandBase> subcommands, IBytebusBuilder builder) :
                base(subcommands, builder, "id")
            { }

            public override void BuildValue(IBytebusBuilder builder, int value) => builder.AddId(value);
        }

        private class IdSetterAdder : FieldSetterAdder<IBytebus, int>
        {
            public IdSetterAdder(List<CommandBase> subcommands, ICollection<IAction<IBytebus>> actions) :
                base(subcommands, actions, "id")
            { }

            public override void SetValue(IBytebus item, int value) => item.Id = value;
        }

        private class EngineFilterAdder : FieldFilterAdder<IBytebus, ParsableString>
        {
            public EngineFilterAdder(List<CommandBase> subcommands, ICollection<IPredicate<IBytebus>> predicates) :
                base(subcommands, predicates, "engine")
            { }

            public override ParsableString GetValue(IBytebus item) => item.EngineClass;
        }

        private class EngineBuilderAdder : FieldBuilderAdder<IBytebus, ParsableString, IBytebusBuilder>
        {
            public EngineBuilderAdder(List<CommandBase> subcommands, IBytebusBuilder builder) :
                base(subcommands, builder, "engine")
            { }

            public override void BuildValue(IBytebusBuilder builder, ParsableString value) => builder.AddEngine(value);
        }

        private class EngineSetterAdder : FieldSetterAdder<IBytebus, ParsableString>
        {
            public EngineSetterAdder(List<CommandBase> subcommands, ICollection<IAction<IBytebus>> actions) :
                base(subcommands, actions, "engine")
            { }

            public override void SetValue(IBytebus item, ParsableString value) => item.EngineClass = value;
        }
    }
}
