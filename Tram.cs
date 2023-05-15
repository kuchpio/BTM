﻿using System.Collections;
using BTM.Text;
using BTM.Hashmap;
using System.Collections.Generic;

namespace BTM
{
    interface ITram : IVehicle, IBTMBase
    {
        int CarsNumber { get; set; }
        ILine Line { get; set; }
    }

    class TramBase : VehicleBase, ITram
    {
        private int carsNumber;
        private ILine line;

        public TramBase(int id, int carsNumber, ILine line = null): base(id)
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
    }

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
                tramText.TextRepr.Remove(startIndex + 1, endIndex - startIndex - 1);
                tramText.TextRepr.Insert(startIndex + 1, $"<{value}>");
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
                tramText.TextRepr.Remove(startIndex + 1, endIndex - startIndex - 1);
                tramText.TextRepr.Insert(startIndex + 1, $"<{value}>");
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
    }

    class TramHashMapAdapter : ITram
    {
        private TramHashMap tramHashMap;
        private ILine line;
        private IDriver driver;

        public TramHashMapAdapter(int id, int carsNumber, ILine line = null)
        {
            tramHashMap = new TramHashMap(new Dictionary<int, string>
            {
                ["id".GetHashCode()] = id.ToString(),
                ["carsNumber".GetHashCode()] = carsNumber.ToString()
            }, line == null ? 0 : line.NumberDec);
            tramHashMap.Line = line != null ? line.NumberDec : -1;
            this.line = line;
        }

        public TramHashMapAdapter(TramHashMap tramHashMap, ILine line)
        {
            this.tramHashMap = tramHashMap;
            this.line = line;
        }

        public int Id 
        { 
            get => int.Parse(tramHashMap.Hashmap["id".GetHashCode()]);
            set => tramHashMap.Hashmap["id".GetHashCode()] = value.ToString();
        }
        public int CarsNumber 
        { 
            get => int.Parse(tramHashMap.Hashmap["carsNumber".GetHashCode()]);
            set => tramHashMap.Hashmap["carsNumber".GetHashCode()] = value.ToString();
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
    }
    interface ITramBuilder : IBTMBuilder<ITram>
    {
        void AddId(int id);
        void AddCarsNumber(int cars);
    }

    class TramBaseBuilder : ITramBuilder
    {
        protected int id = 0, cars = 0;

        public void AddCarsNumber(int cars)
        {
            this.cars = cars;
        }

        public void AddId(int id)
        {
            this.id = id;
        }

        public void Reset()
        {
            id = 0;
            cars = 0;
        }

        public ITram Result()
        {
            ITram result = new TramBase(id, cars);
            Reset();
            return result;
        }
    }

    class TramTextBuilder : TramBaseBuilder
    {
        public new ITram Result()
        {
            ITram result = new TramTextAdapter(id, cars);
            Reset();
            return result;
        }
    }
}