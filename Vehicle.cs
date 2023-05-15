

namespace BTM
{
    interface IVehicle : IBTMBase
    {
        int Id { get; set; }
        IDriver Driver { get; set; }
    }

    class VehicleBase : IVehicle
    {
        private int id;
        private IDriver driver;

        public VehicleBase(int id)
        {
            this.id = id;
        }

        public int Id { get => id; set => id = value; }
        public IDriver Driver { get => driver; set => driver = value; }

        public string ToShortString()
        {
            return id.ToString();
        }
    }
}
