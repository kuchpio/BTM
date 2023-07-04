
namespace BTM
{
    interface ILine : IBTMBase, IRestoreable<ILine>
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

        public ILine Clone()
        {
            LineBase line = new LineBase(NumberDec, CommonName);
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
