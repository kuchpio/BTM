
namespace BTM
{
    interface IStop : IBTMBase, IRestoreable<IStop>
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

        public IStop Clone()
        {
            StopBase stop = new StopBase(Id, Name, Type);
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
