using BTM.Text;

namespace BTM
{
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
                stopText.TextRepr = stopText.TextRepr.Remove(startIndex + 1, endIndex - startIndex - 1);
                stopText.TextRepr = stopText.TextRepr.Insert(startIndex + 1, $"<{value}>");
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
                stopText.TextRepr = stopText.TextRepr.Remove(startIndex + 1, endIndex - startIndex - 1);
                stopText.TextRepr = stopText.TextRepr.Insert(startIndex + 1, $"<{value}>");
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
                stopText.TextRepr = stopText.TextRepr.Remove(startIndex + 1, endIndex - startIndex - 1);
                stopText.TextRepr = stopText.TextRepr.Insert(startIndex + 1, $"<{value}>");
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

        public IStop Clone()
        {
            StopTextAdapter stop = new StopTextAdapter(Id, Name, Type);
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

