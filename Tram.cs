using System.Collections;
using BTM.Text;
using BTM.Hashmap;
using System.Collections.Generic;

namespace BTM
{
    interface ITram : IVehicle, IBTMBase, IRestoreable<ITram>
    {
        int CarsNumber { get; set; }
        ILine Line { get; set; }
    }

    class TramBase : VehicleBase, ITram
    {
        private int carsNumber;
        private ILine line;

        public TramBase(int id, int carsNumber, ILine line = null): base(id)
        {
            this.carsNumber = carsNumber;
            this.line = line;
        }

        public int CarsNumber { get => carsNumber; set => carsNumber = value; }
        public ILine Line { get => line; set => line = value; }

        public override string ToString()
        {
            string lineNumber = line == null ? "TBA" : Line.NumberDec.ToString();
            string driver = Driver == null ? "TBA" : $"{Driver.Name} {Driver.Surname}";
            return $"Id: {Id}, Cars: {CarsNumber}, Line: {lineNumber}, Driver: {driver}";
        }

        public override ITram Clone()
        {
            TramBase tram = new TramBase(Id, CarsNumber, Line);
            tram.Driver = Driver;
            return tram;
        }

        public void CopyFrom(ITram src)
        {
            base.CopyFrom(src);
            CarsNumber = src.CarsNumber;
            Line = src.Line;
        }
    }

    class TramTextAdapter : ITram
    {
        private TramText tramText;
        private ILine line;
        private IDriver driver;

        public TramTextAdapter(int id, int carsNumber, ILine line = null)
        {
            if (line == null)
            {
                tramText = new TramText($"#<{id}>(<{carsNumber}>)");
            }
            else
            {
                tramText = new TramText($"#<{id}>(<{carsNumber}>)<{line.NumberDec}>");
            }
            this.line = line;
        }

        public TramTextAdapter(TramText tramText, ILine line = null)
        {
            this.tramText = tramText;
            this.line = line;
        }

        public int Id 
        { 
            get
            {
                int startIndex = tramText.TextRepr.IndexOf('#');
                int endIndex = tramText.TextRepr.IndexOf('(', startIndex + 1);
                return startIndex >= 0 && startIndex < endIndex ?
                    int.Parse(tramText.TextRepr.Substring(startIndex + 1, endIndex - startIndex - 1).Trim('<', '>')) : -1;
            }
            set
            {
                int startIndex = tramText.TextRepr.IndexOf('#');
                int endIndex = tramText.TextRepr.IndexOf('(', startIndex + 1);
                if (startIndex < 0 || startIndex >= endIndex) return;
                tramText.TextRepr = tramText.TextRepr.Remove(startIndex + 1, endIndex - startIndex - 1);
                tramText.TextRepr = tramText.TextRepr.Insert(startIndex + 1, $"<{value}>");
            }
        }
        public int CarsNumber 
        { 
            get
            {
                int startIndex = tramText.TextRepr.IndexOf('(');
                int endIndex = tramText.TextRepr.IndexOf(')', startIndex + 1);
                return startIndex >= 0 && startIndex < endIndex ?
                    int.Parse(tramText.TextRepr.Substring(startIndex + 1, endIndex - startIndex - 1).Trim('<', '>')) : -1;
            }
            set
            {
                int startIndex = tramText.TextRepr.IndexOf('(');
                int endIndex = tramText.TextRepr.IndexOf(')', startIndex + 1);
                if (startIndex < 0 || startIndex >= endIndex) return;
                tramText.TextRepr = tramText.TextRepr.Remove(startIndex + 1, endIndex - startIndex - 1);
                tramText.TextRepr = tramText.TextRepr.Insert(startIndex + 1, $"<{value}>");
            }
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
            TramTextAdapter tram = new TramTextAdapter(Id, CarsNumber, Line);
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
    interface ITramBuilder : IBTMBuilder<ITram>
    {
        void AddId(int id);
        void AddCarsNumber(int cars);
    }

    class TramBaseBuilder : ITramBuilder
    {
        protected int id = 0, cars = 0;

        public void AddCarsNumber(int cars)
        {
            this.cars = cars;
        }

        public void AddId(int id)
        {
            this.id = id;
        }

        public void Reset()
        {
            id = 0;
            cars = 0;
        }

        public ITram Result()
        {
            ITram result = new TramBase(id, cars);
            Reset();
            return result;
        }

        public override string ToString()
        {
            return "base";
        }
    }

    class TramTextBuilder : TramBaseBuilder
    {
        public new ITram Result()
        {
            ITram result = new TramTextAdapter(id, cars);
            Reset();
            return result;
        }

        public override string ToString()
        {
            return "secondary";
        }
    }

    class TramBuilderLogger : ITramBuilder
    {
        private ITramBuilder builder;
        private List<string> logs;

        public TramBuilderLogger(ITramBuilder builder)
        {
            this.builder = builder;
            logs = new List<string>();
        }

        public void AddId(int id)
        {
            builder.AddId(id);
            logs.Add($"id=\"{id}\"");
        }

        public void AddCarsNumber(int cars)
        {
            builder.AddCarsNumber(cars);
            logs.Add($"carsnumber=\"{cars}\"");
        }

        public void Reset()
        {
            builder.Reset();
            logs.Clear();
        }

        public ITram Result()
        {
            logs.Clear();
            return builder.Result();
        }

        public override string ToString()
        {
            return $"add tram {builder.ToString()}\n{string.Join("\n", logs)}\ndone";
        }
    }
}
