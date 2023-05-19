using BTM.Hashmap;
using BTM.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTM
{
    class BTMSystem
    {
        private static BTMSystem instance;
        private IBTMCollection<ILine> lines;
        private IBTMCollection<IStop> stops;
        private IBTMCollection<IBytebus> buses;
        private IBTMCollection<ITram> trams;
        private IBTMCollection<IDriver> drivers;

        private BTMSystem()
        {
            lines = new Vector<ILine>();
            stops = new Vector<IStop>();
            buses = new Vector<IBytebus>();
            trams = new Vector<ITram>();
            drivers = new Vector<IDriver>();
        }

        private class AddToVehicleCollection<V> : IAction<V> where V : IVehicle
        {
            private IBTMCollection<IVehicle> collection;

            public AddToVehicleCollection(IBTMCollection<IVehicle> collection)
            {
                this.collection = collection;
            }

            public void Eval(V item)
            {
                collection.Add(item);
            }
        }

        public IBTMCollection<ILine> Lines { get => lines; set => lines = value; }
        public IBTMCollection<IStop> Stops { get => stops; set => stops = value; }
        public IBTMCollection<IBytebus> Buses { get => buses; set => buses = value; }
        public IBTMCollection<ITram> Trams { get => trams; set => trams = value; }
        public IBTMCollection<IVehicle> Vehicles
        {
            get
            {
                Vector<IVehicle> vehicles = new Vector<IVehicle>();
                CollectionUtils.ForEach(buses.First(), new AddToVehicleCollection<IBytebus>(vehicles));
                CollectionUtils.ForEach(trams.First(), new AddToVehicleCollection<ITram>(vehicles));
                return vehicles;
            }
        }
        public IBTMCollection<IDriver> Drivers { get => drivers; set => drivers = value; }

        public static BTMSystem GetInstance()
        {
            return instance ?? (instance = new BTMSystem());
        }

        public void SetBaseExample()
        {
            ILine l1b = new LineBase(16, "SIMD");
            ILine l2b = new LineBase(23, "Isengard-Mordor");
            ILine l3b = new LineBase(14, "Museum of Plant");

            IStop s1b = new StopBase(1, "SPIR-V", "bus");
            IStop s2b = new StopBase(2, "GLSL", "tram");
            IStop s3b = new StopBase(3, "HLSL", "other");
            IStop s4b = new StopBase(4, "Dol Guldur", "bus");
            IStop s5b = new StopBase(5, "Amon Hen", "bus");
            IStop s6b = new StopBase(6, "Gondolin", "bus");
            IStop s7b = new StopBase(7, "Bitazon", "tram");
            IStop s8b = new StopBase(8, "Bytecroft", "bus");
            IStop s9b = new StopBase(9, "Maple", "other");

            IBytebus b1b = new BytebusBase(11, "Byte5");
            IBytebus b2b = new BytebusBase(12, "bisel20");
            IBytebus b3b = new BytebusBase(13, "bisel20");
            IBytebus b4b = new BytebusBase(14, "gibgaz");
            IBytebus b5b = new BytebusBase(15, "gibgaz");

            ITram t1b = new TramBase(21, 1);
            ITram t2b = new TramBase(22, 2);
            ITram t3b = new TramBase(23, 6);

            IDriver d1b = new DriverBase("Tomas", "Chairman", 20);
            IDriver d2b = new DriverBase("Tomas", "Thetank", 4);
            IDriver d3b = new DriverBase("Oru", "Bii", 55);

            l1b.AddStop(s1b);
            l1b.AddStop(s2b);
            l1b.AddStop(s3b);
            l1b.AddStop(s8b);
            l1b.AddVehicle(b1b);
            l1b.AddVehicle(b2b);
            l1b.AddVehicle(b3b);

            l2b.AddStop(s4b);
            l2b.AddStop(s5b);
            l2b.AddStop(s6b);
            l2b.AddStop(s7b);
            l2b.AddVehicle(b1b);
            l2b.AddVehicle(b4b);
            l2b.AddVehicle(b5b);

            l3b.AddStop(s7b);
            l3b.AddStop(s8b);
            l3b.AddStop(s9b);
            l3b.AddVehicle(b4b);
            l3b.AddVehicle(t1b);
            l3b.AddVehicle(t2b);
            l3b.AddVehicle(t3b);

            s1b.AddLine(l1b);
            s2b.AddLine(l1b);
            s3b.AddLine(l1b);
            s4b.AddLine(l2b);
            s5b.AddLine(l2b);
            s6b.AddLine(l2b);
            s7b.AddLine(l2b);
            s7b.AddLine(l3b);
            s8b.AddLine(l1b);
            s8b.AddLine(l3b);
            s9b.AddLine(l3b);

            b1b.AddLine(l1b);
            b1b.AddLine(l2b);
            b2b.AddLine(l1b);
            b3b.AddLine(l1b);
            b4b.AddLine(l2b);
            b4b.AddLine(l3b);
            b5b.AddLine(l2b);

            t1b.Line = l3b;
            t2b.Line = l3b;
            t3b.Line = l3b;

            d1b.AddVehicle(b1b);
            b1b.Driver = d1b;
            d1b.AddVehicle(t1b);
            t1b.Driver = d1b;
            d1b.AddVehicle(b5b);
            b5b.Driver = d1b;

            d2b.AddVehicle(b2b);
            b2b.Driver = d2b;
            d2b.AddVehicle(b3b);
            b3b.Driver = d2b;
            d2b.AddVehicle(b4b);
            b4b.Driver = d2b;

            d3b.AddVehicle(t2b);
            t2b.Driver = d3b;
            d3b.AddVehicle(t3b);
            t3b.Driver = d3b;

            Lines = new Vector<ILine>() { l1b, l2b, l3b };
            Stops = new Vector<IStop>() {
                s1b, s2b, s3b, s4b, s5b, s6b, s7b, s8b, s9b
            };
            Drivers = new Vector<IDriver>() { d1b, d2b, d3b };
            Buses = new Vector<IBytebus>() {
                b1b, b2b, b3b, b4b, b5b
            };
            Trams = new Vector<ITram>() { t1b, t2b, t3b };
        }

        public void SetTextExample()
        {
            ILine l1ta = new LineTextAdapter(new LineText("<10>(<16>)`<SIMD>`@!"));
            ILine l2ta = new LineTextAdapter(new LineText("<17>(<23>)`<Isengard-Mordor>`@!"));
            ILine l3ta = new LineTextAdapter(new LineText("<E>(<14>)`<Museum of Plant>`@!"));

            IStop s1ta = new StopTextAdapter(new StopText("#<1>(<16>)<SPIR-V>/<bus>"), new Vector<ILine>() { l1ta });
            IStop s2ta = new StopTextAdapter(new StopText("#<2>(<16>)<GLSL>/<tram>"), new Vector<ILine>() { l1ta });
            IStop s3ta = new StopTextAdapter(new StopText("#<3>(<16>)<HLSL>/<other>"), new Vector<ILine>() { l1ta });
            IStop s4ta = new StopTextAdapter(new StopText("#<4>(<23>)<Dol Guldur>/<bus>"), new Vector<ILine>() { l2ta });
            IStop s5ta = new StopTextAdapter(new StopText("#<5>(<23>)<Amon Hen>/<bus>"), new Vector<ILine>() { l2ta });
            IStop s6ta = new StopTextAdapter(new StopText("#<6>(<23>)<Gondolin>/<bus>"), new Vector<ILine>() { l2ta });
            IStop s7ta = new StopTextAdapter(new StopText("#<7>(<23><14>)<Bitazon>/<tram>"), new Vector<ILine>() { l2ta, l3ta });
            IStop s8ta = new StopTextAdapter(new StopText("#<8>(<16><14>)<Bytecroft>/<bus>"), new Vector<ILine>() { l1ta, l3ta });
            IStop s9ta = new StopTextAdapter(new StopText("#<9>(<14>)<Maple>/<other>"), new Vector<ILine>() { l3ta });

            IBytebus b1ta = new BytebusTextAdapter(new BytebusText("#<11>^<Byte5>*<16><23>"), new Vector<ILine>() { l1ta, l2ta });
            IBytebus b2ta = new BytebusTextAdapter(new BytebusText("#<12>^<bisel20>*<16>"), new Vector<ILine>() { l1ta });
            IBytebus b3ta = new BytebusTextAdapter(new BytebusText("#<13>^<bisel20>*<16>"), new Vector<ILine>() { l1ta });
            IBytebus b4ta = new BytebusTextAdapter(new BytebusText("#<14>^<gibgaz>*<23><14>"), new Vector<ILine>() { l2ta, l3ta });
            IBytebus b5ta = new BytebusTextAdapter(new BytebusText("#<15>^<gibgaz>*<23>"), new Vector<ILine>() { l2ta });

            ITram t1ta = new TramTextAdapter(new TramText("#<21>(<1>)<14>"), l3ta);
            ITram t2ta = new TramTextAdapter(new TramText("#<22>(<2>)<14>"), l3ta);
            ITram t3ta = new TramTextAdapter(new TramText("#<23>(<6>)<14>"), l3ta);

            IDriver d1ta = new DriverTextAdapter(new DriverText("<Tomas> <Chairman>(<20>)@<11><21><15>"), new Vector<IVehicle>() { b1ta, t1ta, b5ta });
            IDriver d2ta = new DriverTextAdapter(new DriverText("<Tomas> <Thetank>(<4>)@<12><13><14>"), new Vector<IVehicle>() { b2ta, b3ta, b4ta });
            IDriver d3ta = new DriverTextAdapter(new DriverText("<Oru> <Bii>(<55>)@<22><23>"), new Vector<IVehicle>() { t2ta, t3ta });

            l1ta.AddStop(s1ta);
            l1ta.AddStop(s2ta);
            l1ta.AddStop(s3ta);
            l1ta.AddStop(s8ta);
            l1ta.AddVehicle(b1ta);
            l1ta.AddVehicle(b2ta);
            l1ta.AddVehicle(b3ta);

            l2ta.AddStop(s4ta);
            l2ta.AddStop(s5ta);
            l2ta.AddStop(s6ta);
            l2ta.AddStop(s7ta);
            l2ta.AddVehicle(b1ta);
            l2ta.AddVehicle(b4ta);
            l2ta.AddVehicle(b5ta);

            l3ta.AddStop(s7ta);
            l3ta.AddStop(s8ta);
            l3ta.AddStop(s9ta);
            l3ta.AddVehicle(b4ta);
            l3ta.AddVehicle(t1ta);
            l3ta.AddVehicle(t2ta);
            l3ta.AddVehicle(t3ta);

            b1ta.Driver = d1ta;
            t1ta.Driver = d1ta;
            b5ta.Driver = d1ta;

            b2ta.Driver = d2ta;
            b3ta.Driver = d2ta;
            b4ta.Driver = d2ta;

            t2ta.Driver = d3ta;
            t3ta.Driver = d3ta;

            Lines = new Vector<ILine>() { l1ta, l2ta, l3ta };
            Stops = new Vector<IStop>() {
                s1ta, s2ta, s3ta, s4ta, s5ta, s6ta, s7ta, s8ta, s9ta
            };
            Drivers = new Vector<IDriver>() { d1ta, d2ta, d3ta };
            Buses = new Vector<IBytebus>() {
                b1ta, b2ta, b3ta, b4ta, b5ta
            };
            Trams = new Vector<ITram>() { t1ta, t2ta, t3ta };
        }

        public void SetHashmapExample()
        {
            ILine l1ha = new LineHashMapAdapter(new LineHashMap(new Dictionary<int, string>()
            {
                ["numberDec".GetHashCode()] = "16",
                ["numberHex".GetHashCode()] = "10",
                ["commonName".GetHashCode()] = "SIMD"
            }, new List<int>(), new List<int>()));

            ILine l2ha = new LineHashMapAdapter(new LineHashMap(new Dictionary<int, string>()
            {
                ["numberDec".GetHashCode()] = "23",
                ["numberHex".GetHashCode()] = "17",
                ["commonName".GetHashCode()] = "Isengard-Mordor"
            }, new List<int>(), new List<int>()));

            ILine l3ha = new LineHashMapAdapter(new LineHashMap(new Dictionary<int, string>()
            {
                ["numberDec".GetHashCode()] = "14",
                ["numberHex".GetHashCode()] = "E",
                ["commonName".GetHashCode()] = "Museum of Plant"
            }, new List<int>(), new List<int>()));

            IStop s1ha = new StopHashMapAdapter(new StopHashMap(new Dictionary<int, string>()
            {
                ["id".GetHashCode()] = "1",
                ["name".GetHashCode()] = "SPIR-V",
                ["type".GetHashCode()] = "bus",
            }, new List<int>() { 16 }), new Vector<ILine>() { l1ha });

            IStop s2ha = new StopHashMapAdapter(new StopHashMap(new Dictionary<int, string>()
            {
                ["id".GetHashCode()] = "2",
                ["name".GetHashCode()] = "GLSL",
                ["type".GetHashCode()] = "tram",
            }, new List<int>() { 16 }), new Vector<ILine>() { l1ha });

            IStop s3ha = new StopHashMapAdapter(new StopHashMap(new Dictionary<int, string>()
            {
                ["id".GetHashCode()] = "3",
                ["name".GetHashCode()] = "HLSL",
                ["type".GetHashCode()] = "other",
            }, new List<int>() { 16 }), new Vector<ILine>() { l1ha });

            IStop s4ha = new StopHashMapAdapter(new StopHashMap(new Dictionary<int, string>()
            {
                ["id".GetHashCode()] = "4",
                ["name".GetHashCode()] = "Dol Guldur",
                ["type".GetHashCode()] = "bus",
            }, new List<int>() { 23 }), new Vector<ILine>() { l2ha });

            IStop s5ha = new StopHashMapAdapter(new StopHashMap(new Dictionary<int, string>()
            {
                ["id".GetHashCode()] = "5",
                ["name".GetHashCode()] = "Amon Hen",
                ["type".GetHashCode()] = "bus",
            }, new List<int>() { 23 }), new Vector<ILine>() { l2ha });

            IStop s6ha = new StopHashMapAdapter(new StopHashMap(new Dictionary<int, string>()
            {
                ["id".GetHashCode()] = "6",
                ["name".GetHashCode()] = "Gondolin",
                ["type".GetHashCode()] = "bus",
            }, new List<int>() { 23 }), new Vector<ILine>() { l2ha });

            IStop s7ha = new StopHashMapAdapter(new StopHashMap(new Dictionary<int, string>()
            {
                ["id".GetHashCode()] = "7",
                ["name".GetHashCode()] = "Bitazon",
                ["type".GetHashCode()] = "tram",
            }, new List<int>() { 23, 14 }), new Vector<ILine>() { l2ha, l3ha });

            IStop s8ha = new StopHashMapAdapter(new StopHashMap(new Dictionary<int, string>()
            {
                ["id".GetHashCode()] = "8",
                ["name".GetHashCode()] = "Bytecroft",
                ["type".GetHashCode()] = "bus",
            }, new List<int>() { 16, 14 }), new Vector<ILine>() { l1ha, l3ha });

            IStop s9ha = new StopHashMapAdapter(new StopHashMap(new Dictionary<int, string>()
            {
                ["id".GetHashCode()] = "9",
                ["name".GetHashCode()] = "Maple",
                ["type".GetHashCode()] = "other",
            }, new List<int>() { 14 }), new Vector<ILine>() { l3ha });

            IBytebus b1ha = new BytebusHashMapAdapter(new BytebusHashMap(new Dictionary<int, string>()
            {
                ["id".GetHashCode()] = "11",
                ["engineClass".GetHashCode()] = "Byte5",
            }, new List<int>() { 16, 23 }), new Vector<ILine>() { l1ha, l2ha });

            IBytebus b2ha = new BytebusHashMapAdapter(new BytebusHashMap(new Dictionary<int, string>()
            {
                ["id".GetHashCode()] = "12",
                ["engineClass".GetHashCode()] = "bisel20",
            }, new List<int>() { 16 }), new Vector<ILine>() { l1ha });

            IBytebus b3ha = new BytebusHashMapAdapter(new BytebusHashMap(new Dictionary<int, string>()
            {
                ["id".GetHashCode()] = "13",
                ["engineClass".GetHashCode()] = "bisel20",
            }, new List<int>() { 16 }), new Vector<ILine>() { l1ha });

            IBytebus b4ha = new BytebusHashMapAdapter(new BytebusHashMap(new Dictionary<int, string>()
            {
                ["id".GetHashCode()] = "14",
                ["engineClass".GetHashCode()] = "gibgaz",
            }, new List<int>() { 23, 14 }), new Vector<ILine>() { l2ha, l3ha });

            IBytebus b5ha = new BytebusHashMapAdapter(new BytebusHashMap(new Dictionary<int, string>()
            {
                ["id".GetHashCode()] = "15",
                ["engineClass".GetHashCode()] = "gibgaz",
            }, new List<int>() { 23 }), new Vector<ILine>() { l2ha });

            ITram t1ha = new TramHashMapAdapter(new TramHashMap(new Dictionary<int, string>()
            {
                ["id".GetHashCode()] = "21",
                ["carsNumber".GetHashCode()] = "1",
            }, 14), l3ha);

            ITram t2ha = new TramHashMapAdapter(new TramHashMap(new Dictionary<int, string>()
            {
                ["id".GetHashCode()] = "22",
                ["carsNumber".GetHashCode()] = "2",
            }, 14), l3ha);

            ITram t3ha = new TramHashMapAdapter(new TramHashMap(new Dictionary<int, string>()
            {
                ["id".GetHashCode()] = "23",
                ["carsNumber".GetHashCode()] = "6",
            }, 14), l3ha);

            IDriver d1ha = new DriverHashMapAdapter(new DriverHashMap(new Dictionary<int, string>()
            {
                ["name".GetHashCode()] = "Tomas",
                ["surname".GetHashCode()] = "Chairman",
                ["seniority".GetHashCode()] = "20"
            }, new List<int>() { 11, 21, 15 }), new Vector<IVehicle>() { b1ha, t1ha, b5ha });

            IDriver d2ha = new DriverHashMapAdapter(new DriverHashMap(new Dictionary<int, string>()
            {
                ["name".GetHashCode()] = "Tomas",
                ["surname".GetHashCode()] = "Thetank",
                ["seniority".GetHashCode()] = "4"
            }, new List<int>() { 12, 13, 14 }), new Vector<IVehicle>() { b2ha, b3ha, b4ha });

            IDriver d3ha = new DriverHashMapAdapter(new DriverHashMap(new Dictionary<int, string>()
            {
                ["name".GetHashCode()] = "Oru",
                ["surname".GetHashCode()] = "Bii",
                ["seniority".GetHashCode()] = "55"
            }, new List<int>() { 22, 23 }), new Vector<IVehicle>() { t2ha, t3ha });

            l1ha.AddStop(s1ha);
            l1ha.AddStop(s2ha);
            l1ha.AddStop(s3ha);
            l1ha.AddStop(s8ha);
            l1ha.AddVehicle(b1ha);
            l1ha.AddVehicle(b2ha);
            l1ha.AddVehicle(b3ha);

            l2ha.AddStop(s4ha);
            l2ha.AddStop(s5ha);
            l2ha.AddStop(s6ha);
            l2ha.AddStop(s7ha);
            l2ha.AddVehicle(b1ha);
            l2ha.AddVehicle(b4ha);
            l2ha.AddVehicle(b5ha);

            l3ha.AddStop(s7ha);
            l3ha.AddStop(s8ha);
            l3ha.AddStop(s9ha);
            l3ha.AddVehicle(b4ha);
            l3ha.AddVehicle(t1ha);
            l3ha.AddVehicle(t2ha);
            l3ha.AddVehicle(t3ha);

            b1ha.Driver = d1ha;
            t1ha.Driver = d1ha;
            b5ha.Driver = d1ha;

            b2ha.Driver = d2ha;
            b3ha.Driver = d2ha;
            b4ha.Driver = d2ha;

            t2ha.Driver = d3ha;
            t3ha.Driver = d3ha;

            Lines = new Vector<ILine>() { l1ha, l2ha, l3ha };
            Stops = new Vector<IStop>() {
                s1ha, s2ha, s3ha, s4ha, s5ha, s6ha, s7ha, s8ha, s9ha
            };
            Drivers = new Vector<IDriver>() { d1ha, d2ha, d3ha };
            Buses = new Vector<IBytebus>() {
                b1ha, b2ha, b3ha, b4ha, b5ha
            };
            Trams = new Vector<ITram>() { t1ha, t2ha, t3ha };
        }
    }
}
