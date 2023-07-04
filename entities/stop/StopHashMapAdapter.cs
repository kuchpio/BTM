using System.Collections.Generic;
using BTM.Hashmap;

namespace BTM
{
    class StopHashMapAdapter : IStop
    {
        private StopHashMap stopHashMap;
        private Vector<ILine> lines;

        public StopHashMapAdapter(int id, string name, string type)
        {
            stopHashMap = new StopHashMap(new Dictionary<int, string>
            {
                ["id".GetHashCode()] = id.ToString(),
                ["name".GetHashCode()] = name,
                ["type".GetHashCode()] = type,
            }, new List<int>());
            lines = new Vector<ILine>();
        }

        public StopHashMapAdapter(StopHashMap stopHashMap, Vector<ILine> lines = null)
        {
            this.stopHashMap = stopHashMap;
            this.lines = lines ?? new Vector<ILine>();
        }

        public int Id
        {
            get => int.Parse(stopHashMap.Hashmap["id".GetHashCode()]);
            set => stopHashMap.Hashmap["id".GetHashCode()] = value.ToString();
        }
        public string Name
        {
            get => stopHashMap.Hashmap["name".GetHashCode()];
            set => stopHashMap.Hashmap["name".GetHashCode()] = value;
        }
        public string Type
        {
            get => stopHashMap.Hashmap["type".GetHashCode()];
            set => stopHashMap.Hashmap["type".GetHashCode()] = value;
        }

        public IIterator<ILine> Lines => lines.First();

        public void AddLine(ILine line)
        {
            stopHashMap.Lines.Add(line.NumberDec);
            lines.Add(line);
        }

        public string ToShortString()
        {
            return Id.ToString();
        }

        public override string ToString()
        {
            return $"Id: {Id}, Name: {Name}, Type: {Type}, Lines: [{CollectionUtils.ToString(Lines)}]";
        }

        public IStop Clone()
        {
            StopHashMapAdapter stop = new StopHashMapAdapter(Id, Name, Type);
            stop.lines.Add(Lines);
            return stop;
        }

        public void CopyFrom(IStop src)
        {
            Id = src.Id;
            Name = src.Name;
            Type = src.Type;
            lines.Clear();
            lines.Add(src.Lines);
        }
    }
}
