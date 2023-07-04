using System.Collections.Generic;

namespace BTM
{
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

        public override string ToString()
        {
            return "base";
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

        public override string ToString()
        {
            return "secondary";
        }
    }

    class TramBuilderLogger : ITramBuilder
    {
        private ITramBuilder builder;
        private List<string> logs;

        public TramBuilderLogger(ITramBuilder builder)
        {
            this.builder = builder;
            logs = new List<string>();
        }

        public void AddId(int id)
        {
            builder.AddId(id);
            logs.Add($"id=\"{id}\"");
        }

        public void AddCarsNumber(int cars)
        {
            builder.AddCarsNumber(cars);
            logs.Add($"carsnumber=\"{cars}\"");
        }

        public void Reset()
        {
            builder.Reset();
            logs.Clear();
        }

        public ITram Result()
        {
            logs.Clear();
            return builder.Result();
        }

        public override string ToString()
        {
            return $"add tram {builder.ToString()}\n{string.Join("\n", logs)}\ndone";
        }
    }
}
