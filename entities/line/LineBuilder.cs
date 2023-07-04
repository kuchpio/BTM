using System.Collections.Generic;

namespace BTM 
{
    interface ILineBuilder : IBTMBuilder<ILine>
    {
        void AddNumberDec(int number);
        void AddNumberHex(string number);
        void AddCommonName(string name);
    }

    class LineBaseBuilder : ILineBuilder
    {
        protected int dec = 0;
        protected string hex = "", name = "";

        public void AddCommonName(string name)
        {
            this.name = name;
        }

        public void AddNumberDec(int number)
        {
            dec = number;
            hex = number.ToString("X");
        }

        public void AddNumberHex(string number)
        {
            hex = number;
            dec = int.Parse(hex, System.Globalization.NumberStyles.HexNumber);
        }

        public void Reset()
        {
            dec = 0;
            hex = "0";
            name = "";
        }

        public ILine Result()
        {
            ILine result = new LineBase(dec, name);
            Reset();
            return result;
        }

        public override string ToString()
        {
            return "base";
        }
    }

    class LineTextBuilder : LineBaseBuilder
    {
        public new ILine Result()
        {
            ILine result = new LineTextAdapter(dec, name);
            Reset();
            return result;
        }

        public override string ToString()
        {
            return "secondary";
        }
    }

    class LineBuilderLogger : ILineBuilder
    {
        private ILineBuilder builder;
        private List<string> logs;

        public LineBuilderLogger(ILineBuilder builder)
        {
            this.builder = builder;
            logs = new List<string>();
        }

        public void AddCommonName(string name)
        {
            builder.AddCommonName(name);
            logs.Add($"commonname=\"{name}\"");
        }

        public void AddNumberDec(int number)
        {
            builder.AddNumberDec(number);
            logs.Add($"numberdec=\"{number}\"");
        }

        public void AddNumberHex(string number)
        {
            builder.AddNumberHex(number);
            logs.Add($"numberhex=\"{number}\"");
        }

        public void Reset()
        {
            builder.Reset();
            logs.Clear();
        }

        public ILine Result()
        {
            logs.Clear();
            return builder.Result();
        }

        public override string ToString()
        {
            return $"add line {builder.ToString()}\n{string.Join("\n", logs)}\ndone";
        }
    }
}
