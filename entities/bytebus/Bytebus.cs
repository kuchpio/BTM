
namespace BTM
{
    
    interface IBytebus : IVehicle, IBTMBase, IRestoreable<IBytebus>
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

        public override IBytebus Clone()
        {
            BytebusBase bytebus = new BytebusBase(Id, EngineClass);
            bytebus.lines.Add(Lines);
            bytebus.Driver = Driver;
            return bytebus;
        }

        public void CopyFrom(IBytebus src)
        {
            base.CopyFrom(src);
            EngineClass = src.EngineClass;
            lines.Clear();
            lines.Add(src.Lines);
        }
    }
}
