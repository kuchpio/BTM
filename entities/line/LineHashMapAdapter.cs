using System.Collections.Generic;
using BTM.Hashmap;

namespace BTM
{
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
            set => lineHashMap.Hashmap["numberDec".GetHashCode()] = value.ToString();
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

        public ILine Clone()
        {
            LineHashMapAdapter line = new LineHashMapAdapter(NumberDec, CommonName);
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

