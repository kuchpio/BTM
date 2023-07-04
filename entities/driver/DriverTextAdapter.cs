using BTM.Text;

namespace BTM
{
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
                driverText.TextRepr = driverText.TextRepr.Remove(startIndex + 1, endIndex - startIndex - 1);
                driverText.TextRepr = driverText.TextRepr.Insert(startIndex + 1, $"<{value}>");
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
                driverText.TextRepr = driverText.TextRepr.Remove(startIndex + 1, endIndex - startIndex - 1);
                driverText.TextRepr = driverText.TextRepr.Insert(startIndex + 1, $"<{value}>");
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
                driverText.TextRepr = driverText.TextRepr.Remove(startIndex + 1, endIndex - startIndex - 1);
                driverText.TextRepr = driverText.TextRepr.Insert(startIndex + 1, $"<{value}>");
            }
        }

        public IIterator<IVehicle> Vehicles => vehicles.First();

        public void AddVehicle(IVehicle vehicle)
        {
            int index = driverText.TextRepr.LastIndexOf('@');
            if (index < 0) return;
            driverText.TextRepr = driverText.TextRepr.Insert(index + 1, $"<{vehicle.Id}>");
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
            DriverTextAdapter driver = new DriverTextAdapter(Name, Surname, Seniority);
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
