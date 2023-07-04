using System.Collections.Generic;

namespace BTM
{
    interface IDriverBuilder : IBTMBuilder<IDriver>
    {
        void AddName(string name);
        void AddSurname(string surname);
        void AddSeniority(int seniority);
    }

    class DriverBaseBuilder : IDriverBuilder
    {
        protected int seniority = 0;
        protected string name = "", surname = "";

        public void AddName(string name)
        {
            this.name = name;
        }

        public void AddSeniority(int seniority)
        {
            this.seniority = seniority;
        }

        public void AddSurname(string surname)
        {
            this.surname = surname;
        }

        public void Reset()
        {
            name = "";
            surname = "";
            seniority = 0;
        }

        public IDriver Result()
        {
            IDriver result = new DriverBase(name, surname, seniority);
            Reset();
            return result;
        }

        public override string ToString()
        {
            return "base";
        }
    }

    class DriverTextBuilder : DriverBaseBuilder
    {
        public new IDriver Result()
        {
            IDriver result = new DriverTextAdapter(name, surname, seniority);
            Reset();
            return result;
        }

        public override string ToString()
        {
            return "secondary";
        }
    }

    class DriverBuilderLogger : IDriverBuilder
    {
        private IDriverBuilder builder;
        private List<string> logs;

        public DriverBuilderLogger(IDriverBuilder builder)
        {
            this.builder = builder;
            logs = new List<string>();
        }

        public void AddName(string name)
        {
            builder.AddName(name);
            logs.Add($"name=\"{name}\"");
        }

        public void AddSeniority(int seniority)
        {
            builder.AddSeniority(seniority);
            logs.Add($"seniority=\"{seniority}\"");
        }

        public void AddSurname(string surname)
        {
            builder.AddSurname(surname);
            logs.Add($"surname=\"{surname}\"");
        }

        public void Reset()
        {
            builder.Reset();
            logs.Clear();
        }

        public IDriver Result()
        {
            logs.Clear();
            return builder.Result();
        }

        public override string ToString()
        {
            return $"add driver {builder.ToString()}\n{string.Join("\n", logs)}\ndone";
        }
    }
}
