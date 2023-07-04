using System.Collections.Generic;

namespace BTM
{
    class VehicleQuery : CollectionQuery<IVehicle>
    {
        public VehicleQuery() : base(BTM.GetInstance().Vehicles, "vehicle", "`id`: numeric")
        { }

        public override IEnumerable<CommandBase> CreateFilterAdders(List<CommandBase> subcommands)
        {
            return new List<CommandBase>()
            {
                new IdFilterAdder(subcommands, Filter.Predicates),
            };
        }

        public override IEnumerable<CommandBase> CreateSetterAdders(List<CommandBase> subcommands)
        {
            return new List<CommandBase>()
            {
                new IdSetterAdder(subcommands, EditActions.Actions),
            };
        }

        private class IdFilterAdder : FieldFilterAdder<IVehicle, int>
        {
            public IdFilterAdder(List<CommandBase> subcommands, ICollection<IPredicate<IVehicle>> predicates) :
                base(subcommands, predicates, "id")
            { }

            public override int GetValue(IVehicle item) => item.Id;
        }

        private class IdSetterAdder : FieldSetterAdder<IVehicle, int>
        {
            public IdSetterAdder(List<CommandBase> subcommands, ICollection<IAction<IVehicle>> actions) :
                base(subcommands, actions, "id")
            { }

            public override void SetValue(IVehicle item, int value) => item.Id = value;
        }
    }
}
