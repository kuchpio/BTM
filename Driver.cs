using System.Collections;
using System.Collections.Generic;
using BTM.Text;
using BTM.Hashmap;

namespace BTM
{
    interface IDriver : IBTMBase
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
    }

    class DriverTextAdapter : IDriver
    {
        private DriverText driverText;
        private Vector<IVehicle> vehicles;

        public DriverTextAdapter(string name, string surname, int seniority)
        {
            driverText = new DriverText($"<{name}> <{surname}>(<{seniority}>)@");
            vehicles = new Vector<IVehicle>();
        }

        public DriverTextAdapter(DriverText driverText, Vector<IVehicle> vehicles = null)
        {
            this.driverText = driverText;
            this.vehicles = vehicles ?? new Vector<IVehicle>();
        }

        public string Name 
        { 
            get
            {
                int startIndex = -1;
                int endIndex = driverText.TextRepr.IndexOf(' ');
                return endIndex > startIndex ?
                    driverText.TextRepr.Substring(0, endIndex).Trim('<', '>') : "";
            }
            set
            {
                int startIndex = -1;
                int endIndex = driverText.TextRepr.IndexOf(' ');
                if (startIndex >= endIndex) return;
                driverText.TextRepr.Remove(startIndex + 1, endIndex - startIndex - 1);
                driverText.TextRepr.Insert(startIndex + 1, $"<{value}>");
            }
        }
        public string Surname 
        { 
            get
            {
                int startIndex = driverText.TextRepr.IndexOf(' ');
                int endIndex = driverText.TextRepr.IndexOf('(', startIndex + 1);
                return startIndex >= 0 && startIndex < endIndex ?
                    driverText.TextRepr.Substring(startIndex + 1, endIndex - startIndex - 1).Trim('<', '>') : "";
            }
            set
            {
                int startIndex = driverText.TextRepr.IndexOf(' ');
                int endIndex = driverText.TextRepr.IndexOf('(', startIndex + 1);
                if (startIndex < 0 || startIndex >= endIndex) return;
                driverText.TextRepr.Remove(startIndex + 1, endIndex - startIndex - 1);
                driverText.TextRepr.Insert(startIndex + 1, $"<{value}>");
            }
        }
        public int Seniority 
        { 
            get
            {
                int startIndex = driverText.TextRepr.IndexOf('(');
                int endIndex = driverText.TextRepr.IndexOf(')', startIndex + 1);
                return startIndex >= 0 && startIndex < endIndex ?
                    int.Parse(driverText.TextRepr.Substring(startIndex + 1, endIndex - startIndex - 1).Trim('<', '>')) : -1;
            }
            set
            {
                int startIndex = driverText.TextRepr.IndexOf('(');
                int endIndex = driverText.TextRepr.IndexOf(')', startIndex + 1);
                if (startIndex < 0 || startIndex >= endIndex) return;
                driverText.TextRepr.Remove(startIndex + 1, endIndex - startIndex - 1);
                driverText.TextRepr.Insert(startIndex + 1, $"<{value}>");
            }
        }

        public IIterator<IVehicle> Vehicles => vehicles.First();

        public void AddVehicle(IVehicle vehicle)
        {
            int index = driverText.TextRepr.LastIndexOf('@');
            if (index < 0) return;
            driverText.TextRepr.Insert(index + 1, $"<{vehicle.Id}>");
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
    }

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
    }

    interface IDriverBuilder : IBTMBuilder<IDriver>
    {
        void AddName(string name);
        void AddSurname(string surname);
        void AddSeniority(int seniority);
    }

    class DriverBaseBuilder : IDriverBuilder
    {
        protected int seniority = 0;
        protected string name = "", surname = "";

        public void AddName(string name)
        {
            this.name = name;
        }

        public void AddSeniority(int seniority)
        {
            this.seniority = seniority;
        }

        public void AddSurname(string surname)
        {
            this.surname = surname;
        }

        public void Reset()
        {
            name = "";
            surname = "";
            seniority = 0;
        }

        public IDriver Result()
        {
            IDriver result = new DriverBase(name, surname, seniority);
            Reset();
            return result;
        }

        public override string ToString()
        {
            return "base";
        }
    }

    class DriverTextBuilder : DriverBaseBuilder
    {
        public new IDriver Result()
        {
            IDriver result = new DriverTextAdapter(name, surname, seniority);
            Reset();
            return result;
        }

        public override string ToString()
        {
            return "secondary";
        }
    }

    class DriverBuilderLogger : IDriverBuilder
    {
        private IDriverBuilder builder;
        private List<string> logs;

        public DriverBuilderLogger(IDriverBuilder builder)
        {
            this.builder = builder;
            logs = new List<string>();
        }

        public void AddName(string name)
        {
            builder.AddName(name);
            logs.Add($"name=\"{name}\"");
        }

        public void AddSeniority(int seniority)
        {
            throw new NotImplementedException();
        }

        public void AddSurname(string surname)
        {
            builder.AddName(surname);
            logs.Add($"surname=\"{surname}\"");
        }

        public void Reset()
        {
            builder.Reset();
            logs.Clear();
        }

        public IDriver Result()
        {
            logs.Clear();
            return builder.Result();
        }

        public override string ToString()
        {
            return $"add driver {builder.ToString()}\n{string.Join("\n", logs)}\ndone";
        }
    }
}
