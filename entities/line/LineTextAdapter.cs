using BTM.Text;

namespace BTM
{
    class LineTextAdapter : ILine
    {
        LineText lineText;
        Vector<IStop> stops;
        Vector<IVehicle> vehicles;

        public LineTextAdapter(int number, string commonName)
        {
            lineText = new LineText($"<{number:X}>(<{number}>)`<{commonName}>`@!");
            stops = new Vector<IStop>();
            vehicles = new Vector<IVehicle>();
        }

        public LineTextAdapter(LineText lineText, Vector<IStop> stops = null, Vector<IVehicle> vehicles = null)
        {
            this.lineText = lineText;
            this.stops = stops ?? new Vector<IStop>();
            this.vehicles = vehicles ?? new Vector<IVehicle>();
        }

        public int NumberDec 
        { 
            get
            {
                int startIndex = lineText.TextRepr.IndexOf('(');
                int endIndex = lineText.TextRepr.IndexOf(')', startIndex + 1);
                return startIndex >= 0 && startIndex < endIndex ? 
                    int.Parse(lineText.TextRepr.Substring(startIndex + 1, endIndex - startIndex - 1).Trim('<', '>')) : -1; 
            }
            set
            {
                if (NumberDec == value) return;
                int startIndex = lineText.TextRepr.IndexOf('(');
                int endIndex = lineText.TextRepr.IndexOf(')', startIndex + 1);
                if (startIndex < 0 || startIndex >= endIndex) return;
                lineText.TextRepr = lineText.TextRepr.Remove(startIndex + 1, endIndex - startIndex - 1);
                lineText.TextRepr = lineText.TextRepr.Insert(startIndex + 1, $"<{value}>");
                NumberHex = value.ToString("X");
            }
        }

        public string NumberHex 
        { 
            get
            {
                int startIndex = -1;
                int endIndex = lineText.TextRepr.IndexOf('(');
                return startIndex < endIndex ? 
                    lineText.TextRepr.Substring(0, endIndex).Trim('<', '>') : "";
            }
            set
            {
                if (NumberHex == value) return;
                int startIndex = -1;
                int endIndex = lineText.TextRepr.IndexOf('(');
                if (startIndex >= endIndex) return;
                lineText.TextRepr = lineText.TextRepr.Remove(startIndex + 1, endIndex - startIndex - 1);
                lineText.TextRepr = lineText.TextRepr.Insert(startIndex + 1, $"<{value}>");
                NumberDec = int.Parse(value, System.Globalization.NumberStyles.HexNumber); 
            }
        }

        public string CommonName 
        { 
            get 
            {
                int startIndex = lineText.TextRepr.IndexOf('`');
                int endIndex = lineText.TextRepr.IndexOf('`', startIndex + 1);

                return startIndex >= 0 && startIndex < endIndex ? 
                    lineText.TextRepr.Substring(startIndex + 1, endIndex - startIndex - 1).Trim('<', '>') : "";
            }
            set
            {
                int startIndex = lineText.TextRepr.IndexOf('`');
                int endIndex = lineText.TextRepr.IndexOf('`', startIndex + 1);
                if (startIndex < 0 || startIndex >= endIndex) return;
                lineText.TextRepr = lineText.TextRepr.Remove(startIndex + 1, endIndex - startIndex - 1);
                lineText.TextRepr = lineText.TextRepr.Insert(startIndex + 1, $"<{value}>");
            }
        }

        public IIterator<IStop> Stops => stops.First();

        public IIterator<IVehicle> Vehicles => vehicles.First();

        public void AddStop(IStop stop)
        {
            int index = lineText.TextRepr.LastIndexOf('@');
            if (index < 0) return;
            lineText.TextRepr = lineText.TextRepr.Insert(index + 1, $"<{stop.Id}>");
            stops.Add(stop);
        }

        public void AddVehicle(IVehicle vehicle)
        {
            int index = lineText.TextRepr.LastIndexOf('!');
            if (index >= 0)
            {
                lineText.TextRepr = lineText.TextRepr.Insert(index + 1, $"<{vehicle.Id}>");
            }
            vehicles.Add(vehicle);
        }

        public override string ToString()
        {
            return $"Hex: {NumberHex}, Dec: {NumberDec}, Name: {CommonName}, Stops: [{CollectionUtils.ToString(Stops)}], Vehicles: [{CollectionUtils.ToString(Vehicles)}]";
        }

        public string ToShortString()
        {
            return NumberDec.ToString();
        }

        public ILine Clone()
        {
            LineTextAdapter line = new LineTextAdapter(NumberDec, CommonName);
            line.stops.Add(Stops);
            line.vehicles.Add(Vehicles);
            return line;
        }

        public void CopyFrom(ILine src)
        {
            NumberDec = src.NumberDec;
            CommonName = src.CommonName;
            stops.Clear();
            stops.Add(src.Stops);
            vehicles.Clear();
            vehicles.Add(src.Vehicles);
        }
    }
}

