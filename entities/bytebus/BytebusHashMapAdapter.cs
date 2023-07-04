using System.Collections.Generic;
using BTM.Hashmap;

namespace BTM
{
    class BytebusHashMapAdapter : IBytebus
    {
        private BytebusHashMap bytebusHashMap;
        private Vector<ILine> lines;
        private IDriver driver;

        public BytebusHashMapAdapter(int id, string engineClass)
        {
            bytebusHashMap = new BytebusHashMap(new Dictionary<int, string>
            {
                ["id".GetHashCode()] = id.ToString(),
                ["engineClass".GetHashCode()] = engineClass
            }, new List<int>());
            lines = new Vector<ILine>();
        }

        public BytebusHashMapAdapter(BytebusHashMap bytebusHashMap, Vector<ILine> lines = null)
        {
            this.bytebusHashMap = bytebusHashMap;
            this.lines = lines ?? new Vector<ILine>();
        }

        public int Id
        {
            get => int.Parse(bytebusHashMap.Hashmap["id".GetHashCode()]);
            set => bytebusHashMap.Hashmap["id".GetHashCode()] = value.ToString();
        }
        public string EngineClass
        {
            get => bytebusHashMap.Hashmap["engineClass".GetHashCode()];
            set => bytebusHashMap.Hashmap["engineClass".GetHashCode()] = value;
        }

        public IIterator<ILine> Lines => lines.First();
        public IDriver Driver { get => driver; set => driver = value; }

        public void AddLine(ILine line)
        {
            bytebusHashMap.Lines.Add(line.NumberDec);
            lines.Add(line);
        }

        public override string ToString()
        {
            string driver = Driver == null ? "TBA" : $"{Driver.Name} {Driver.Surname}";
            return $"Id: {Id}, Lines: [{CollectionUtils.ToString(Lines)}], Engine: {EngineClass}, Driver: {driver}";
        }

        public string ToShortString()
        {
            return Id.ToString();
        }

        public IBytebus Clone()
        {
            BytebusHashMapAdapter bytebus = new BytebusHashMapAdapter(Id, EngineClass);
            bytebus.lines.Add(Lines);
            bytebus.Driver = Driver;
            return bytebus;
        }

        public void CopyFrom(IBytebus src)
        {
            CopyFrom((IVehicle)src);
            EngineClass = src.EngineClass;
            lines.Clear();
            lines.Add(src.Lines);
        }

        public void CopyFrom(IVehicle src)
        {
            Id = src.Id;
            Driver = src.Driver;
        }

        IVehicle IRestoreable<IVehicle>.Clone() => this.Clone();
    }
}
