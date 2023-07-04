using BTM.Text;

namespace BTM
{
    class BytebusTextAdapter : IBytebus
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
                bytebusText.TextRepr = bytebusText.TextRepr.Remove(startIndex + 1, endIndex - startIndex - 1);
                bytebusText.TextRepr = bytebusText.TextRepr.Insert(startIndex + 1, $"<{value}>");
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
                bytebusText.TextRepr = bytebusText.TextRepr.Remove(startIndex + 1, endIndex - startIndex - 1);
                bytebusText.TextRepr = bytebusText.TextRepr.Insert(startIndex + 1, $"<{value}>");
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

        public IBytebus Clone()
        {
            BytebusTextAdapter bytebus = new BytebusTextAdapter(Id, EngineClass);
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
