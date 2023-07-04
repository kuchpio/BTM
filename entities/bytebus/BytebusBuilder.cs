using System.Collections.Generic;

namespace BTM
{
    interface IBytebusBuilder : IBTMBuilder<IBytebus>
    {
        void AddId(int id);
        void AddEngine(string engine);
    }

    class BytebusBaseBuilder : IBytebusBuilder
    {
        protected int id = 0;
        protected string engine = "";

        public void Reset()
        {
            id = 0;
            engine = "";
        }

        public IBytebus Result()
        {
            IBytebus result = new BytebusBase(id, engine);
            Reset();
            return result;
        }

        public void AddEngine(string engine)
        {
            this.engine = engine;
        }

        public void AddId(int id)
        {
            this.id = id;
        }

        public override string ToString()
        {
            return "base";
        }
    }

    class BytebusTextBuilder : BytebusBaseBuilder
    {
        public new IBytebus Result()
        {
            IBytebus result = new BytebusTextAdapter(id, engine);
            Reset();
            return result;
        }

        public override string ToString()
        {
            return "secondary";
        }
    }

    class BytebusBuilderLogger : IBytebusBuilder
    {
        private IBytebusBuilder builder;
        private List<string> logs;

        public BytebusBuilderLogger(IBytebusBuilder builder)
        {
            this.builder = builder;
            logs = new List<string>();
        }

        public void AddEngine(string engine)
        {
            builder.AddEngine(engine);
            logs.Add($"engine=\"{engine}\"");
        }

        public void AddId(int id)
        {
            builder.AddId(id);
            logs.Add($"id=\"{id}\"");
        }

        public void Reset()
        {
            builder.Reset();
            logs.Clear();
        }

        public IBytebus Result()
        {
            logs.Clear();
            return builder.Result();
        }

        public override string ToString()
        {
            return $"add bytebus {builder.ToString()}\n{string.Join("\n", logs)}\ndone";
        }
    }
}
