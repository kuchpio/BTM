using System.Collections.Generic;
using BTM.Hashmap;

namespace BTM
{
    class DriverHashMapAdapter : IDriver
    {
        private DriverHashMap driverHashMap;
        private Vector<IVehicle> vehicles;

        public DriverHashMapAdapter(string name, string surname, int seniority)
        {
            driverHashMap = new DriverHashMap(new Dictionary<int, string>
            {
                ["name".GetHashCode()] = name,
                ["surname".GetHashCode()] = surname,
                ["seniority".GetHashCode()] = seniority.ToString()
            }, new List<int>());
            vehicles = new Vector<IVehicle>();
        }

        public DriverHashMapAdapter(DriverHashMap driverHashMap, Vector<IVehicle> vehicles = null)
        {
            this.driverHashMap = driverHashMap;
            this.vehicles = vehicles ?? new Vector<IVehicle>();
        }

        public string Name
        {
            get => driverHashMap.Hashmap["name".GetHashCode()];
            set => driverHashMap.Hashmap["name".GetHashCode()] = value;
        }
        public string Surname
        {
            get => driverHashMap.Hashmap["surname".GetHashCode()];
            set => driverHashMap.Hashmap["surname".GetHashCode()] = value;
        }
        public int Seniority
        {
            get => int.Parse(driverHashMap.Hashmap["seniority".GetHashCode()]);
            set => driverHashMap.Hashmap["seniority".GetHashCode()] = value.ToString();
        }

        public IIterator<IVehicle> Vehicles => vehicles.First();

        public void AddVehicle(IVehicle vehicle)
        {
            driverHashMap.Vehicles.Add(vehicle.Id);
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
            DriverHashMapAdapter driver = new DriverHashMapAdapter(Name, Surname, Seniority);
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
