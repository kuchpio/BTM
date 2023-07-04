using System.Collections.Generic;

namespace BTM
{
    interface IStopBuilder : IBTMBuilder<IStop>
    {
        void AddId(int id);
        void AddName(string name);
        void AddStopType(string type);
    }

    class StopBaseBuilder : IStopBuilder
    {
        protected int id = 0;
        protected string name = "", type = "";

        public void AddId(int id)
        {
            this.id = id;
        }

        public void AddName(string name)
        {
            this.name = name;
        }

        public void AddStopType(string type)
        {
            this.type = type;
        }

        public void Reset()
        {
            id = 0;
            name = "";
            type = "";
        }

        public IStop Result()
        {
            IStop result = new StopBase(id, name, type);
            Reset();
            return result;
        }

        public override string ToString()
        {
            return "base";
        }
    }

    class StopTextBuilder : StopBaseBuilder
    {
        public new IStop Result()
        {
            IStop result = new StopTextAdapter(id, name, type);
            Reset();
            return result;
        }

        public override string ToString()
        {
            return "secondary";
        }
    }

    class StopBuilderLogger : IStopBuilder
    {
        private IStopBuilder builder;
        private List<string> logs;

        public StopBuilderLogger(IStopBuilder builder)
        {
            this.builder = builder;
            logs = new List<string>();
        }

        public void AddId(int id)
        {
            builder.AddId(id);
            logs.Add($"id=\"{id}\"");
        }

        public void AddName(string name)
        {
            builder.AddName(name);
            logs.Add($"name=\"{name}\"");
        }

        public void AddStopType(string type)
        {
            builder.AddStopType(type);
            logs.Add($"type=\"{type}\"");
        }

        public void Reset()
        {
            builder.Reset();
            logs.Clear();
        }

        public IStop Result()
        {
            logs.Clear();
            return builder.Result();
        }

        public override string ToString()
        {
            return $"add stop {builder.ToString()}\n{string.Join("\n", logs)}\ndone";
        }
    }
}
