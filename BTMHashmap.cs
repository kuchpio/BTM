using System.Collections;
using System.Collections.Generic;

namespace BTM.Hashmap
{
    interface IHashMapRepresentation
    {
        Dictionary<int, string> Hashmap
        {
            get;
            set;
        }
    }
    
    class LineHashMap: IHashMapRepresentation
    {
        private Dictionary<int, string> hashmap;
        private List<int> stops;
        private List<int> vehicles;

        public Dictionary<int, string> Hashmap
        {
            get { return hashmap; }
            set { hashmap = value; }
        }

        public List<int> Stops
        {
            get { return stops; }
            set { stops = value; }
        }

        public List<int> Vehicles
        {
            get { return vehicles; }
            set { vehicles = value; }
        }

        public LineHashMap(Dictionary<int, string> hashmap, List<int> stops, List<int> vehicles)
        {
            Hashmap = hashmap;
            Stops = stops;
            Vehicles = vehicles;
        }
    }

    class StopHashMap: IHashMapRepresentation
    {
        private Dictionary<int, string> hashmap;
        private List<int> lines;

        public Dictionary<int, string> Hashmap
        {
            get { return hashmap; }
            set { hashmap = value; }
        }

        public List<int> Lines
        {
            get { return lines; }
            set { lines = value; }
        }

        public StopHashMap(Dictionary<int, string> hashmap, List<int> lines)
        {
            Hashmap = hashmap;
            Lines = lines;
        }
    }

    class BytebusHashMap: IHashMapRepresentation
    {
        private Dictionary<int, string> hashmap;
        private List<int> lines;

        public Dictionary<int, string> Hashmap
        {
            get { return hashmap; }
            set { hashmap = value; }
        }

        public List<int> Lines
        {
            get { return lines; }
            set { lines = value; }
        }

        public BytebusHashMap(Dictionary<int, string> hashmap, List<int> lines)
        {
            Hashmap = hashmap;
            Lines = lines;
        }
    }

    class TramHashMap: IHashMapRepresentation
    {
        private Dictionary<int, string> hashmap;
        private int line;

        public Dictionary<int, string> Hashmap
        {
            get { return hashmap; }
            set { hashmap = value; }
        }

        public int Line
        {
            get { return line; }
            set { line = value; }
        }

        public TramHashMap(Dictionary<int, string> hashmap, int line)
        {
            Hashmap = hashmap;
            Line = line;
        }
    }

    class DriverHashMap: IHashMapRepresentation
    {
        private Dictionary<int, string> hashmap;
        private List<int> vehicles;

        public Dictionary<int, string> Hashmap
        {
            get { return hashmap; }
            set { hashmap = value; }
        }

        public List<int> Vehicles
        {
            get { return vehicles; }
            set { vehicles = value; }
        }

        public DriverHashMap(Dictionary<int, string> hashmap, List<int> vehicles)
        {
            Hashmap = hashmap;
            Vehicles = vehicles;
        }
    }
}
