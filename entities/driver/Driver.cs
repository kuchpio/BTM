
namespace BTM
{
    interface IDriver : IBTMBase, IRestoreable<IDriver>
    {
        string Name { get; set; }
        string Surname { get; set; }
        int Seniority { get; set; }
        IIterator<IVehicle> Vehicles { get; }
        void AddVehicle(IVehicle vehicle);
    }

    class DriverBase : IDriver
    {
        private Vector<IVehicle> vehicles;
        private string name;
        private string surname;
        private int seniority;

        public DriverBase(string name, string surname, int seniority)
        {
            this.name = name;
            this.surname = surname;
            this.seniority = seniority;
            vehicles = new Vector<IVehicle>();
        }

        public string Name { get => name; set => name = value; }
        public string Surname { get => surname; set => surname = value; }
        public int Seniority { get => seniority; set => seniority = value; }

        public IIterator<IVehicle> Vehicles => vehicles.First();

        public void AddVehicle(IVehicle vehicle)
        {
            vehicles.Add(vehicle);
        }

        public string ToShortString()
        {
            return $"{Name} {Surname}";
        }

        public override string ToString()
        {
            return $"Fullname: {Name} {Surname}, Seniority: {Seniority}, Vehicles: [{CollectionUtils.ToString(Vehicles)}]";
        }

        public IDriver Clone()
        {
            DriverBase driver = new DriverBase(Name, Surname, Seniority);
            driver.vehicles.Add(Vehicles);
            return driver;
        }

        public void CopyFrom(IDriver src)
        {
            Name = src.Name;
            Surname = src.Surname;
            Seniority = src.Seniority;
            vehicles.Clear();
            vehicles.Add(src.Vehicles);
        }
    }
}
