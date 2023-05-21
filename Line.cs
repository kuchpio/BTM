using System.Collections;
using System.Collections.Generic;
using BTM.Text;
using BTM.Hashmap;

namespace BTM
{
    interface ILine : IBTMBase
    {
        int NumberDec { get; set; }
        string NumberHex { get; set; }
        string CommonName { get; set; }
        IIterator<IStop> Stops { get; }
        IIterator<IVehicle> Vehicles { get; }
        void AddStop(IStop stop);
        void AddVehicle(IVehicle vehicle);
    }

    class LineBase : ILine
    {
        private int numberDec;
        private string numberHex;
        private string commonName;
        private Vector<IStop> stops;
        private Vector<IVehicle> vehicles;

        public LineBase(int numberDec, string commonName)
        {
            NumberDec = numberDec;
            CommonName = commonName;
            stops = new Vector<IStop>();
            vehicles = new Vector<IVehicle>();
        }

        public LineBase(string numberHex, string commonName)
        {
            NumberHex = numberHex;
            CommonName = commonName;
            stops = new Vector<IStop>();
            vehicles = new Vector<IVehicle>();
        }

        public int NumberDec 
        { 
            get => numberDec;
            set
            {
                numberDec = value;
                numberHex = value.ToString("X");
            }
        }

        public string NumberHex 
        {
            get => numberHex;
            set 
            {
                numberHex = value;
                numberDec = int.Parse(value, System.Globalization.NumberStyles.HexNumber);
            }
        }

        public string CommonName 
        {
            get => commonName;
            set => commonName = value;
        }

        public IIterator<IStop> Stops => stops.First();

        public IIterator<IVehicle> Vehicles => vehicles.First();

        public void AddStop(IStop stop)
        {
            stops.Add(stop);
        }

        public void AddVehicle(IVehicle vehicle)
        {
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
    }

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
                int startIndex = lineText.TextRepr.IndexOf('(');
                int endIndex = lineText.TextRepr.IndexOf(')', startIndex + 1);
                if (startIndex < 0 || startIndex >= endIndex) return;
                lineText.TextRepr.Remove(startIndex + 1, endIndex - startIndex - 1);
                lineText.TextRepr.Insert(startIndex + 1, $"<{value}>");
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
                int startIndex = -1;
                int endIndex = lineText.TextRepr.IndexOf('(');
                if (startIndex >= endIndex) return;
                lineText.TextRepr.Remove(startIndex + 1, endIndex - startIndex - 1);
                lineText.TextRepr.Insert(startIndex + 1, $"<{value}>");
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
                lineText.TextRepr.Remove(startIndex + 1, endIndex - startIndex - 1);
                lineText.TextRepr.Insert(startIndex + 1, $"<{value}>");
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
    }

    class LineHashMapAdapter : ILine
    {
        private LineHashMap lineHashMap;
        private Vector<IStop> stops;
        private Vector<IVehicle> vehicles;

        public LineHashMapAdapter(int number, string commonName)
        {
            lineHashMap = new LineHashMap(new Dictionary<int, string>
            {
                ["numberDec".GetHashCode()] = number.ToString(),
                ["numberHex".GetHashCode()] = number.ToString("X"),
                ["commonName".GetHashCode()] = commonName
            }, new List<int>(), new List<int>());

            stops = new Vector<IStop>();
            vehicles = new Vector<IVehicle>();
        }

        public LineHashMapAdapter(LineHashMap lineHashMap, Vector<IStop> stops = null, Vector<IVehicle> vehicles = null)
        {
            this.lineHashMap = lineHashMap;
            this.stops = stops ?? new Vector<IStop>();
            this.vehicles = vehicles ?? new Vector<IVehicle>();
        }

        public int NumberDec 
        { 
            get => int.Parse(lineHashMap.Hashmap["numberDec".GetHashCode()]);
            set => lineHashMap.Hashmap["commonName".GetHashCode()] = value.ToString();
        }
        public string NumberHex 
        { 
            get => lineHashMap.Hashmap["numberHex".GetHashCode()];
            set => lineHashMap.Hashmap["numberHex".GetHashCode()] = value;
        }
        public string CommonName 
        { 
            get => lineHashMap.Hashmap["commonName".GetHashCode()];
            set => lineHashMap.Hashmap["commonName".GetHashCode()] = value;
        }

        public IIterator<IStop> Stops => stops.First();

        public IIterator<IVehicle> Vehicles => vehicles.First();

        public void AddStop(IStop stop)
        {
            lineHashMap.Stops.Add(stop.Id);
            stops.Add(stop);
        }

        public void AddVehicle(IVehicle vehicle)
        {
            lineHashMap.Vehicles.Add(vehicle.Id);
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
    }

    interface ILineBuilder : IBTMBuilder<ILine>
    {
        void AddNumberDec(int number);
        void AddNumberHex(string number);
        void AddCommonName(string name);
    }

    class LineBaseBuilder : ILineBuilder
    {
        protected int dec = 0;
        protected string hex = "", name = "";

        public void AddCommonName(string name)
        {
            this.name = name;
        }

        public void AddNumberDec(int number)
        {
            dec = number;
            hex = number.ToString("X");
        }

        public void AddNumberHex(string number)
        {
            hex = number;
            dec = int.Parse(hex, System.Globalization.NumberStyles.HexNumber);
        }

        public void Reset()
        {
            dec = 0;
            hex = "0";
            name = "";
        }

        public ILine Result()
        {
            ILine result = new LineBase(dec, name);
            Reset();
            return result;
        }

        public override string ToString()
        {
            return "base";
        }
    }

    class LineTextBuilder : LineBaseBuilder
    {
        public new ILine Result()
        {
            ILine result = new LineTextAdapter(dec, name);
            Reset();
            return result;
        }

        public override string ToString()
        {
            return "secondary";
        }
    }

    class LineBuilderLogger : ILineBuilder
    {
        private ILineBuilder builder;
        private List<string> logs;

        public LineBuilderLogger(ILineBuilder builder)
        {
            this.builder = builder;
            logs = new List<string>();
        }

        public void AddCommonName(string name)
        {
            builder.AddCommonName(name);
            logs.Add($"commonname=\"{name}\"");
        }

        public void AddNumberDec(int number)
        {
            builder.AddNumberDec(number);
            logs.Add($"numberdec=\"{number}\"");
        }

        public void AddNumberHex(string number)
        {
            builder.AddNumberHex(number);
            logs.Add($"numberhex=\"{number}\"");
        }

        public void Reset()
        {
            builder.Reset();
            logs.Clear();
        }

        public ILine Result()
        {
            logs.Clear();
            return builder.Result();
        }

        public override string ToString()
        {
            return $"add line {builder.ToString()}\n{string.Join("\n", logs)}\ndone";
        }
    }
}
