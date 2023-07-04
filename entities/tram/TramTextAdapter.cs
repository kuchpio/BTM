using BTM.Text;

namespace BTM
{
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
}
