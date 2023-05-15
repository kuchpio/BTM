using System.Collections;
using System.Collections.Generic;
using BTM.Text;
using BTM.Hashmap;

namespace BTM
{
    interface IStop : IBTMBase
    {
        int Id { get; set; }
        string Name { get; set; }
        string Type { get; set; }
        IIterator<ILine> Lines { get; }
        void AddLine(ILine line);
    }

    class StopBase : IStop
    {
        private int id;
        private Vector<ILine> lines;
        private string name;
        private string type;

        public StopBase(int id, string name, string type)
        {
            this.id = id;
            this.name = name;
            this.type = type;
            lines = new Vector<ILine>();
        }

        public int Id { get => id; set => id = value; }
        public string Name { get => name; set => name = value; }
        public string Type { get => type; set => type = value; }
        public IIterator<ILine> Lines => lines.First();

        public void AddLine(ILine line)
        {
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
    }

    class StopTextAdapter : IStop
    {
        StopText stopText;
        Vector<ILine> lines;

        public StopTextAdapter(int id, string name, string type)
        {
            stopText = new StopText($"#<{id}>()<{name}>/<{type}>");
            lines = new Vector<ILine>();
        }

        public StopTextAdapter(StopText stopText, Vector<ILine> lines = null)
        {
            this.stopText = stopText;
            this.lines = lines ?? new Vector<ILine>();
        }

        public int Id 
        { 
            get
            {
                int startIndex = stopText.TextRepr.IndexOf('#');
                int endIndex = stopText.TextRepr.IndexOf('(', startIndex + 1);
                return startIndex >= 0 && startIndex < endIndex ?
                    int.Parse(stopText.TextRepr.Substring(startIndex + 1, endIndex - startIndex - 1).Trim('<', '>')) : -1;
            }
            set
            {
                int startIndex = stopText.TextRepr.IndexOf('#');
                int endIndex = stopText.TextRepr.IndexOf('(', startIndex + 1);
                if (startIndex < 0 || startIndex >= endIndex) return;
                stopText.TextRepr.Remove(startIndex + 1, endIndex - startIndex - 1);
                stopText.TextRepr.Insert(startIndex + 1, $"<{value}>");
            }
        }
        public string Name 
        { 
            get
            {
                int startIndex = stopText.TextRepr.IndexOf(')');
                int endIndex = stopText.TextRepr.IndexOf('/', startIndex + 1);
                return startIndex >= 0 && startIndex < endIndex ?
                    stopText.TextRepr.Substring(startIndex + 1, endIndex - startIndex - 1).Trim('<', '>') : "";
            }
            set
            {
                int startIndex = stopText.TextRepr.IndexOf(')');
                int endIndex = stopText.TextRepr.IndexOf('/', startIndex + 1);
                if (startIndex < 0 || startIndex >= endIndex) return;
                stopText.TextRepr.Remove(startIndex + 1, endIndex - startIndex - 1);
                stopText.TextRepr.Insert(startIndex + 1, $"<{value}>");
            }
        }
        public string Type 
        { 
            get
            {
                int startIndex = stopText.TextRepr.IndexOf('/');
                return startIndex >= 0 ?
                    stopText.TextRepr.Substring(startIndex + 1).Trim('<', '>') : "";
            }
            set
            {
                int startIndex = stopText.TextRepr.IndexOf('/');
                int endIndex = stopText.TextRepr.Length;
                if (startIndex < 0) return;
                stopText.TextRepr.Remove(startIndex + 1, endIndex - startIndex - 1);
                stopText.TextRepr.Insert(startIndex + 1, $"<{value}>");
            }
        }
        public IIterator<ILine> Lines => lines.First();

        public void AddLine(ILine line)
        {
            int index = stopText.TextRepr.LastIndexOf('(');
            if (index < 0) return;
            stopText.TextRepr = stopText.TextRepr.Insert(index + 1, $"<{line.NumberDec}>");
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
    }

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
    }

    interface IStopBuilder : IBTMBuilder<IStop>
    {
        void AddId(int id);
        void AddName(string name);
        void AddStopType(string type);
    }

    class StopBaseBuilder : IStopBuilder
    {
        protected int id = 0;
        protected string name = "", type = "";
        
        public void AddId(int id)
        {
            this.id = id;
        }

        public void AddName(string name)
        {
            this.name = name;
        }

        public void AddStopType(string type)
        {
            this.type = type;
        }

        public void Reset()
        {
            id = 0;
            name = "";
            type = "";
        }

        public IStop Result()
        {
            IStop result = new StopBase(id, name, type);
            Reset();
            return result;
        }
    }

    class StopTextBuilder : StopBaseBuilder
    {
        public new IStop Result()
        {
            IStop result = new StopTextAdapter(id, name, type);
            Reset();
            return result;
        }
    }
}
