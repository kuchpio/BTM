using System.Collections.Generic;

namespace BTM
{
    class DriverQuery : CollectionQuery<IDriver>, IBuildableCollectionQuery<IDriver, IDriverBuilder>
    {
        public DriverQuery() : base(BTM.GetInstance().Drivers, "driver", "`name`: string, `surname`: string, `seniority`: numeric")
        { }

        public IEnumerable<CommandBase> CreateBuilderAdders(List<CommandBase> subcommands, IDriverBuilder builder)
        {
            return new List<CommandBase>()
            {
                new NameBuilderAdder(subcommands, builder),
                new SurnameBuilderAdder(subcommands, builder),
                new SeniorityBuilderAdder(subcommands, builder)
            };
        }

        public override IEnumerable<CommandBase> CreateFilterAdders(List<CommandBase> subcommands)
        {
            return new List<CommandBase>()
            {
                new NameFilterAdder(subcommands, Filter.Predicates),
                new SurnameFilterAdder(subcommands, Filter.Predicates),
                new SeniorityFilterAdder(subcommands, Filter.Predicates)
            };
        }

        public override IEnumerable<CommandBase> CreateSetterAdders(List<CommandBase> subcommands)
        {
            return new List<CommandBase>()
            {
                new NameSetterAdder(subcommands, EditActions.Actions),
                new SurnameSetterAdder(subcommands, EditActions.Actions),
                new SenioritySetterAdder(subcommands, EditActions.Actions)
            };
        }

        private class NameFilterAdder : FieldFilterAdder<IDriver, ParsableString>
        {
            public NameFilterAdder(List<CommandBase> subcommands, ICollection<IPredicate<IDriver>> predicates) :
                base(subcommands, predicates, "name")
            { }

            public override ParsableString GetValue(IDriver item) => item.Name;
        }

        private class NameBuilderAdder : FieldBuilderAdder<IDriver, ParsableString, IDriverBuilder>
        {
            public NameBuilderAdder(List<CommandBase> subcommands, IDriverBuilder builder) :
                base(subcommands, builder, "name")
            { }

            public override void BuildValue(IDriverBuilder builder, ParsableString value) => builder.AddName(value);
        }

        private class NameSetterAdder : FieldSetterAdder<IDriver, ParsableString>
        {
            public NameSetterAdder(List<CommandBase> subcommands, ICollection<IAction<IDriver>> actions) :
                base(subcommands, actions, "name")
            { }

            public override void SetValue(IDriver item, ParsableString value) => item.Name = value;
        }

        private class SurnameFilterAdder : FieldFilterAdder<IDriver, ParsableString>
        {
            public SurnameFilterAdder(List<CommandBase> subcommands, ICollection<IPredicate<IDriver>> predicates) :
                base(subcommands, predicates, "surname")
            { }

            public override ParsableString GetValue(IDriver item) => item.Surname;
        }

        private class SurnameBuilderAdder : FieldBuilderAdder<IDriver, ParsableString, IDriverBuilder>
        {
            public SurnameBuilderAdder(List<CommandBase> subcommands, IDriverBuilder builder) :
                base(subcommands, builder, "surname")
            { }

            public override void BuildValue(IDriverBuilder builder, ParsableString value) => builder.AddSurname(value);
        }

        private class SurnameSetterAdder : FieldSetterAdder<IDriver, ParsableString>
        {
            public SurnameSetterAdder(List<CommandBase> subcommands, ICollection<IAction<IDriver>> actions) :
                base(subcommands, actions, "surname")
            { }

            public override void SetValue(IDriver item, ParsableString value) => item.Surname = value;
        }

        private class SeniorityFilterAdder : FieldFilterAdder<IDriver, int>
        {
            public SeniorityFilterAdder(List<CommandBase> subcommands, ICollection<IPredicate<IDriver>> predicates) :
                base(subcommands, predicates, "seniority")
            { }

            public override int GetValue(IDriver item) => item.Seniority;
        }

        private class SeniorityBuilderAdder : FieldBuilderAdder<IDriver, int, IDriverBuilder>
        {
            public SeniorityBuilderAdder(List<CommandBase> subcommands, IDriverBuilder builder) :
                base(subcommands, builder, "seniority")
            { }

            public override void BuildValue(IDriverBuilder builder, int value) => builder.AddSeniority(value);
        }

        private class SenioritySetterAdder : FieldSetterAdder<IDriver, int>
        {
            public SenioritySetterAdder(List<CommandBase> subcommands, ICollection<IAction<IDriver>> actions) :
                base(subcommands, actions, "seniority")
            { }

            public override void SetValue(IDriver item, int value) => item.Seniority = value;
        }
    }
}
