using System.Collections.Generic;

namespace BTM
{
    class TramQuery : CollectionQuery<ITram>, IBuildableCollectionQuery<ITram, ITramBuilder>
    {
        public TramQuery() : base(BTM.GetInstance().Trams, "tram", "`id`: numeric, `carsNumber`: numeric")
        { }

        public IEnumerable<CommandBase> CreateBuilderAdders(List<CommandBase> subcommands, ITramBuilder builder)
        {
            return new List<CommandBase>()
            {
                new IdBuilderAdder(subcommands, builder),
                new CarsNumberBuilderAdder(subcommands, builder),
            };
        }

        public override IEnumerable<CommandBase> CreateFilterAdders(List<CommandBase> subcommands)
        {
            return new List<CommandBase>()
            {
                new IdFilterAdder(subcommands, Filter.Predicates),
                new CarsNumberFilterAdder(subcommands, Filter.Predicates),
            };
        }

        public override IEnumerable<CommandBase> CreateSetterAdders(List<CommandBase> subcommands)
        {
            return new List<CommandBase>()
            {
                new IdSetterAdder(subcommands, EditActions.Actions),
                new CarsNumberSetterAdder(subcommands, EditActions.Actions),
            };
        }

        private class IdFilterAdder : FieldFilterAdder<ITram, int>
        {
            public IdFilterAdder(List<CommandBase> subcommands, ICollection<IPredicate<ITram>> predicates) :
                base(subcommands, predicates, "id")
            { }

            public override int GetValue(ITram item) => item.Id;
        }

        private class IdBuilderAdder : FieldBuilderAdder<ITram, int, ITramBuilder>
        {
            public IdBuilderAdder(List<CommandBase> subcommands, ITramBuilder builder) :
                base(subcommands, builder, "id")
            { }

            public override void BuildValue(ITramBuilder builder, int value) => builder.AddId(value);
        }

        private class IdSetterAdder : FieldSetterAdder<ITram, int>
        {
            public IdSetterAdder(List<CommandBase> subcommands, ICollection<IAction<ITram>> actions) :
                base(subcommands, actions, "id")
            { }

            public override void SetValue(ITram item, int value) => item.Id = value;
        }

        private class CarsNumberFilterAdder : FieldFilterAdder<ITram, int>
        {
            public CarsNumberFilterAdder(List<CommandBase> subcommands, ICollection<IPredicate<ITram>> predicates) :
                base(subcommands, predicates, "carsnumber")
            { }

            public override int GetValue(ITram item) => item.CarsNumber;
        }

        private class CarsNumberBuilderAdder : FieldBuilderAdder<ITram, int, ITramBuilder>
        {
            public CarsNumberBuilderAdder(List<CommandBase> subcommands, ITramBuilder builder) :
                base(subcommands, builder, "carsnumber")
            { }

            public override void BuildValue(ITramBuilder builder, int value) => builder.AddCarsNumber(value);
        }

        private class CarsNumberSetterAdder : FieldSetterAdder<ITram, int>
        {
            public CarsNumberSetterAdder(List<CommandBase> subcommands, ICollection<IAction<ITram>> actions) :
                base(subcommands, actions, "carsnumber")
            { }

            public override void SetValue(ITram item, int value) => item.CarsNumber = value;
        }
    }
}
