using System.Collections;
using System.Collections.Generic;
using BTM.Text;
using BTM.Hashmap;

namespace BTM
{
    
    interface IBytebus : IVehicle, IBTMBase
    {
        string EngineClass { get; set; }
        IIterator<ILine> Lines { get; }
        void AddLine(ILine line);
    }

    class BytebusBase : VehicleBase, IBytebus
    {
        private Vector<ILine> lines;
        private string engineClass;

        public BytebusBase(int id, string engineClass): base(id)
        {
            this.engineClass = engineClass;
            lines = new Vector<ILine>();
        }

        public string EngineClass { get => engineClass; set => engineClass = value; }

        public IIterator<ILine> Lines => lines.First();

        public void AddLine(ILine line)
        {
            lines.Add(line);
        }

        public override string ToString()
        {
            string driver = Driver == null ? "TBA" : $"{Driver.Name} {Driver.Surname}";
            return $"Id: {Id}, Lines: [{CollectionUtils.ToString(Lines)}], Engine: {EngineClass}, Driver: {driver}";
        }
    }

    class BytebusTextAdapter: IBytebus
    {
        private BytebusText bytebusText;
        private Vector<ILine> lines;
        private IDriver driver;

        public BytebusTextAdapter(int id, string engineClass)
        {
            bytebusText = new BytebusText($"#<{id}>^<{engineClass}>*");
            lines = new Vector<ILine>();
        }

        public BytebusTextAdapter(BytebusText bytebusText, Vector<ILine> lines = null)
        {
            this.bytebusText = bytebusText;
            this.lines = lines ?? new Vector<ILine>();
        }

        public int Id 
        { 
            get
            {
                int startIndex = bytebusText.TextRepr.IndexOf('#');
                int endIndex = bytebusText.TextRepr.IndexOf('^', startIndex + 1);
                return startIndex >= 0 && startIndex < endIndex ?
                    int.Parse(bytebusText.TextRepr.Substring(startIndex + 1, endIndex - startIndex - 1).Trim('<', '>')) : -1;
            }
            set
            {
                int startIndex = bytebusText.TextRepr.IndexOf('#');
                int endIndex = bytebusText.TextRepr.IndexOf('^', startIndex + 1);
                if (startIndex < 0 || startIndex >= endIndex) return;
                bytebusText.TextRepr.Remove(startIndex + 1, endIndex - startIndex - 1);
                bytebusText.TextRepr.Insert(startIndex + 1, $"<{value}>");
            }
        }
        public string EngineClass 
        { 
            get
            {
                int startIndex = bytebusText.TextRepr.IndexOf('^');
                int endIndex = bytebusText.TextRepr.IndexOf('*', startIndex + 1);
                return startIndex >= 0 && startIndex < endIndex ?
                    bytebusText.TextRepr.Substring(startIndex + 1, endIndex - startIndex - 1).Trim('<', '>') : "";
            }
            set
            {
                int startIndex = bytebusText.TextRepr.IndexOf('^');
                int endIndex = bytebusText.TextRepr.IndexOf('*', startIndex + 1);
                if (startIndex < 0 || startIndex >= endIndex) return;
                bytebusText.TextRepr.Remove(startIndex + 1, endIndex - startIndex - 1);
                bytebusText.TextRepr.Insert(startIndex + 1, $"<{value}>");
            }
        }

        public IIterator<ILine> Lines => lines.First();
        public IDriver Driver { get => driver; set => driver = value; }

        public void AddLine(ILine line)
        {
            bytebusText.TextRepr += $"<{line.NumberDec}>";
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
    }

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
    }

    interface IBytebusBuilder : IBTMBuilder<IBytebus>
    {
        void AddId(int id);
        void AddEngine(string engine);
    }

    class BytebusBaseBuilder : IBytebusBuilder
    {
        protected int id = 0;
        protected string engine = "";

        public void Reset()
        {
            id = 0;
            engine = "";
        }

        public IBytebus Result()
        {
            IBytebus result = new BytebusBase(id, engine);
            Reset();
            return result;
        }

        public void AddEngine(string engine)
        {
            this.engine = engine;
        }

        public void AddId(int id)
        {
            this.id = id;
        }
    }

    class BytebusTextBuilder : BytebusBaseBuilder
    {
        public new IBytebus Result()
        {
            IBytebus result = new BytebusTextAdapter(id, engine);
            Reset();
            return result;
        }
    }
}
