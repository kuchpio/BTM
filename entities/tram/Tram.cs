
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

        public TramBase(int id, int carsNumber, ILine line = null) : base(id)
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
}
