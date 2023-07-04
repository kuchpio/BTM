using System.Collections.Generic;
using BTM.Hashmap;

namespace BTM
{
    class TramHashMapAdapter : ITram
    {
        private TramHashMap tramHashMap;
        private ILine line;
        private IDriver driver;

        public TramHashMapAdapter(int id, int carsNumber, ILine line = null)
        {
            tramHashMap = new TramHashMap(new Dictionary<int, string>
            {
                ["id".GetHashCode()] = id.ToString(),
                ["carsNumber".GetHashCode()] = carsNumber.ToString()
            }, line == null ? 0 : line.NumberDec);
            tramHashMap.Line = line != null ? line.NumberDec : -1;
            this.line = line;
        }

        public TramHashMapAdapter(TramHashMap tramHashMap, ILine line)
        {
            this.tramHashMap = tramHashMap;
            this.line = line;
        }

        public int Id
        {
            get => int.Parse(tramHashMap.Hashmap["id".GetHashCode()]);
            set => tramHashMap.Hashmap["id".GetHashCode()] = value.ToString();
        }
        public int CarsNumber
        {
            get => int.Parse(tramHashMap.Hashmap["carsNumber".GetHashCode()]);
            set => tramHashMap.Hashmap["carsNumber".GetHashCode()] = value.ToString();
        }
        public ILine Line { get => line; set => line = value; }
        public IDriver Driver { get => driver; set => driver = value; }

        public override string ToString()
        {
            string lineNumber = line == null ? "TBA" : Line.NumberDec.ToString();
            string driver = Driver == null ? "TBA" : $"{Driver.Name} {Driver.Surname}";
            return $"Id: {Id}, Cars: {CarsNumber}, Line: {lineNumber}, Driver: {driver}";
        }

        public string ToShortString()
        {
            return Id.ToString();
        }

        public ITram Clone()
        {
            TramHashMapAdapter tram = new TramHashMapAdapter(Id, CarsNumber, Line);
            tram.Driver = Driver;
            return tram;
        }

        public void CopyFrom(ITram src)
        {
            this.CopyFrom((IVehicle)src);
            CarsNumber = src.CarsNumber;
            Line = src.Line;
        }

        public void CopyFrom(IVehicle src)
        {
            Id = src.Id;
            Driver = src.Driver;
        }

        IVehicle IRestoreable<IVehicle>.Clone() => this.Clone();
    }
}
