using System.Collections.Generic;

namespace BTM
{
    class StopQuery : CollectionQuery<IStop>, IBuildableCollectionQuery<IStop, IStopBuilder>
    {
        public StopQuery() : base(BTM.GetInstance().Stops, "stop", "`id`: numeric, `name`: string, `type`: string")
        { }

        public IEnumerable<CommandBase> CreateBuilderAdders(List<CommandBase> subcommands, IStopBuilder builder)
        {
            return new List<CommandBase>()
            {
                new IdBuilderAdder(subcommands, builder),
                new NameBuilderAdder(subcommands, builder),
                new TypeBuilderAdder(subcommands, builder)
            };
        }

        public override IEnumerable<CommandBase> CreateFilterAdders(List<CommandBase> subcommands)
        {
            return new List<CommandBase>()
            {
                new IdFilterAdder(subcommands, Filter.Predicates),
                new NameFilterAdder(subcommands, Filter.Predicates),
                new TypeFilterAdder(subcommands, Filter.Predicates)
            };
        }

        public override IEnumerable<CommandBase> CreateSetterAdders(List<CommandBase> subcommands)
        {
            return new List<CommandBase>()
            {
                new IdSetterAdder(subcommands, EditActions.Actions),
                new NameSetterAdder(subcommands, EditActions.Actions),
                new TypeSetterAdder(subcommands, EditActions.Actions)
            };
        }

        private class IdFilterAdder : FieldFilterAdder<IStop, int>
        {
            public IdFilterAdder(List<CommandBase> subcommands, ICollection<IPredicate<IStop>> predicates) :
                base(subcommands, predicates, "id")
            { }

            public override int GetValue(IStop item) => item.Id;
        }

        private class IdBuilderAdder : FieldBuilderAdder<IStop, int, IStopBuilder>
        {
            public IdBuilderAdder(List<CommandBase> subcommands, IStopBuilder builder) :
                base(subcommands, builder, "id")
            { }

            public override void BuildValue(IStopBuilder builder, int value) => builder.AddId(value);
        }

        private class IdSetterAdder : FieldSetterAdder<IStop, int>
        {
            public IdSetterAdder(List<CommandBase> subcommands, ICollection<IAction<IStop>> actions) :
                base(subcommands, actions, "id")
            { }

            public override void SetValue(IStop item, int value) => item.Id = value;
        }

        private class NameFilterAdder : FieldFilterAdder<IStop, ParsableString>
        {
            public NameFilterAdder(List<CommandBase> subcommands, ICollection<IPredicate<IStop>> predicates) :
                base(subcommands, predicates, "name")
            { }

            public override ParsableString GetValue(IStop item) => item.Name;
        }

        private class NameBuilderAdder : FieldBuilderAdder<IStop, ParsableString, IStopBuilder>
        {
            public NameBuilderAdder(List<CommandBase> subcommands, IStopBuilder builder) :
                base(subcommands, builder, "name")
            { }

            public override void BuildValue(IStopBuilder builder, ParsableString value) => builder.AddName(value);
        }

        private class NameSetterAdder : FieldSetterAdder<IStop, ParsableString>
        {
            public NameSetterAdder(List<CommandBase> subcommands, ICollection<IAction<IStop>> actions) :
                base(subcommands, actions, "name")
            { }

            public override void SetValue(IStop item, ParsableString value) => item.Name = value;
        }

        private class TypeFilterAdder : FieldFilterAdder<IStop, ParsableString>
        {
            public TypeFilterAdder(List<CommandBase> subcommands, ICollection<IPredicate<IStop>> predicates) :
                base(subcommands, predicates, "type")
            { }

            public override ParsableString GetValue(IStop item) => item.Type;
        }

        private class TypeBuilderAdder : FieldBuilderAdder<IStop, ParsableString, IStopBuilder>
        {
            public TypeBuilderAdder(List<CommandBase> subcommands, IStopBuilder builder) :
                base(subcommands, builder, "type")
            { }

            public override void BuildValue(IStopBuilder builder, ParsableString value) => builder.AddStopType(value);
        }

        private class TypeSetterAdder : FieldSetterAdder<IStop, ParsableString>
        {
            public TypeSetterAdder(List<CommandBase> subcommands, ICollection<IAction<IStop>> actions) :
                base(subcommands, actions, "type")
            { }

            public override void SetValue(IStop item, ParsableString value) => item.Type = value;
        }
    }
}
