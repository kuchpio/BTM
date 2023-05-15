using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BTM.CollectionUtils;

namespace BTM
{
    interface ICommand
    {
        bool Check(string input);
        string Process(string input);
        void Execute(string input);
    }

    abstract class CommandBase : ICommand
    {
        protected List<CommandBase> subcommands;

        public CommandBase(List<CommandBase> subcommands)
        {
            this.subcommands = subcommands;
        }

        public CommandBase()
        {
            subcommands = new List<CommandBase>();
        }
        
        public abstract bool Check(string input);

        public abstract string Process(string input);

        public virtual void Execute(string input)
        {
            input = Process(input);

            if (subcommands.Count == 0) return;

            input = input.Trim();

            foreach (CommandBase command in subcommands)
            {
                if (command.Check(input))
                {
                    command.Execute(input);
                    return;
                }
            }

            Console.WriteLine($"Unknown command syntax near: \n`{input}`");
        }
    }

    class Terminal
    {
        private bool work;
        private CommandExecutor commandExecutor;

        public Terminal()
        {
            work = true;
            commandExecutor = new CommandExecutor(this);
        }

        public void Run()
        {
            while (work)
            {
                Console.Write("BTM> ");
                string userInput = Console.ReadLine();

                if (commandExecutor.Check(userInput))
                    commandExecutor.Execute(userInput);
            }
        }

        public void Close()
        {
            work = false;
        }
    }

    class CommandExecutor : CommandBase
    {
        public CommandExecutor(Terminal terminal) : 
            base(new List<CommandBase>() { 
                new ListCommand(), 
                new FindCommand(), 
                new AddCommand(),
                new EditCommand(),
                new ExitCommand(terminal) 
            })
        { }

        public override bool Check(string input)
        {
            return input != null;
        }

        public override string Process(string input)
        {
            return input;
        }
    }

    class ExitCommand : CommandBase
    {
        private Terminal terminal;

        public ExitCommand(Terminal terminal)
        {
            this.terminal = terminal;
        }
        
        public override bool Check(string input)
        {
            return input == "exit";
        }

        public override string Process(string input)
        {
            terminal.Close();
            return "";
        }
    }

    // Helper classes
    class CollectionSelector<BTMBase> : CommandBase where BTMBase : IBTMBase
    {
        protected IBTMCollection<BTMBase> collection;
        protected List<IPredicate<BTMBase>> collectionFilters;
        private string collectionName;

        public CollectionSelector(IBTMCollection<BTMBase> collection, string collectionName)
        {
            this.collection = collection;
            collectionFilters = new List<IPredicate<BTMBase>>();
            this.collectionName = collectionName;
        }

        public CollectionSelector(List<CommandBase> subcommands, IBTMCollection<BTMBase> collection, string collectionName) : 
            base(subcommands)
        {
            this.collection = collection;
            this.collectionName = collectionName;
        }

        public override bool Check(string input)
        {
            return input.ToLower().StartsWith(collectionName) &&
                (input.Length == collectionName.Length || input[collectionName.Length] == ' ');
        }

        public override string Process(string input)
        {
            return input.Remove(0, collectionName.Length);
        }
    }

    abstract class FieldValueProcesser<BTMBase> : CommandBase where BTMBase : IBTMBase
    {
        private string fieldLower, operators;
        private bool numeric;

        public FieldValueProcesser(List<CommandBase> subcommands, string fieldLower, bool numeric, string operators = "=") :
            base(subcommands)
        {
            this.fieldLower = fieldLower;
            this.numeric = numeric;
            this.operators = operators;
        }

        public override bool Check(string input)
        {
            if (input.Length < fieldLower.Length + 2 ||
                !input.ToLower().StartsWith(fieldLower) ||
                !operators.Contains(input[fieldLower.Length]))
                return false;

            string valueString = ExtractInputComponents(input).Item3;

            if (valueString == null || (numeric && !int.TryParse(valueString, out _)))
                return false;

            return true;
        }

        protected (string, char, string, string) ExtractInputComponents(string input)
        {
            int valueStart, valueEnd;

            if (input[fieldLower.Length + 1] == '\"')
            {
                if (input.Length == fieldLower.Length + 2) return (fieldLower, input[fieldLower.Length], null, "");

                valueStart = fieldLower.Length + 2;
                valueEnd = input.IndexOf('\"', fieldLower.Length + 2);

                if (valueEnd == -1) return (fieldLower, input[fieldLower.Length], null, "");
            }
            else
            {
                valueStart = fieldLower.Length + 1;
                valueEnd = input.IndexOf(' ', fieldLower.Length + 1);

                if (valueEnd == -1) valueEnd = input.Length;
            }

            return (
                fieldLower,
                input[fieldLower.Length],
                input.Substring(valueStart, valueEnd - valueStart),
                input.Remove(0, (valueEnd < input.Length && input[valueEnd] == '\"') ? valueEnd + 1 : valueEnd)
            );
        }
    }

    abstract class PredicateAdder<BTMBase> : FieldValueProcesser<BTMBase> where BTMBase : IBTMBase
    {
        protected List<IPredicate<BTMBase>> predicates;

        public PredicateAdder(
            List<CommandBase> subcommands,
            List<IPredicate<BTMBase>> predicates,
            string fieldLower,
            string operators,
            bool numeric
            ) : base(subcommands, fieldLower, numeric, operators)
        {
            this.predicates = predicates;
        }

        public override string Process(string input)
        {
            (string _, char relation, string valueString, string remainingInput) = ExtractInputComponents(input);

            predicates.Add(CreatePredicate(relation, valueString));

            return remainingInput;
        }

        protected abstract IPredicate<BTMBase> CreatePredicate(char relation, string valueString);
    }

    class ActionsIfPredicates<BTMBase> : IAction<BTMBase> where BTMBase : IBTMBase
    {
        private List<IPredicate<BTMBase>> predicates;
        private List<IAction<BTMBase>> actions;

        public ActionsIfPredicates(IAction<BTMBase> action, List<IPredicate<BTMBase>> predicates)
        {
            actions = new List<IAction<BTMBase>>() { action };
            this.predicates = predicates;
        }

        public ActionsIfPredicates(List<IAction<BTMBase>> actions, List<IPredicate<BTMBase>> predicates)
        {
            this.actions = actions;
            this.predicates = predicates;
        }

        public void Eval(BTMBase item)
        {
            foreach (IPredicate<BTMBase> predicate in predicates)
                if (predicate.Eval(item) == false) return;

            foreach (IAction<BTMBase> action in actions)
                action.Eval(item);
        }
    }

    

    abstract class ComparableValuePredicate<BTMBase, ValueType> : IPredicate<BTMBase>
            where BTMBase : IBTMBase
            where ValueType : IComparable
    {
        private char op;
        private ValueType value;

        public ComparableValuePredicate(char op, ValueType value)
        {
            this.op = op;
            this.value = value;
        }

        public abstract ValueType GetValue(BTMBase item);

        public bool Eval(BTMBase item)
        {
            int diff = GetValue(item).CompareTo(value);
            switch (op)
            {
                case '=': return diff == 0;
                case '<': return diff < 0;
                case '>': return diff > 0;
            }
            return false;
        }
    }

    class LineFilter : CollectionSelector<ILine>
    {
        public LineFilter(bool enableFiltering = true) : base(BTMSystem.GetInstance().Lines, "line")
        {
            if (!enableFiltering) return;
            subcommands.Add(new FilterByNumberDec(subcommands, collectionFilters));
            subcommands.Add(new FilterByNumberHex(subcommands, collectionFilters));
            subcommands.Add(new FilterByCommonName(subcommands, collectionFilters));
        }

        private class FilterByNumberDec : PredicateAdder<ILine>
        {
            public FilterByNumberDec(List<CommandBase> subcommands, List<IPredicate<ILine>> predicates) :
                base(subcommands, predicates, "numberdec", "=<>", true)
            { }

            protected override IPredicate<ILine> CreatePredicate(char relation, string valueString)
            {
                return new NumberDecPredicate(relation, int.Parse(valueString));
            }

            private class NumberDecPredicate : ComparableValuePredicate<ILine, int>
            {
                public NumberDecPredicate(char op, int number) : base(op, number)
                { }

                public override int GetValue(ILine item)
                {
                    return item.NumberDec;
                }
            }
        }

        private class FilterByNumberHex : PredicateAdder<ILine>
        {
            public FilterByNumberHex(List<CommandBase> subcommands, List<IPredicate<ILine>> predicates) :
                base(subcommands, predicates, "numberhex", "=<>", false)
            { }

            protected override IPredicate<ILine> CreatePredicate(char relation, string valueString)
            {
                return new NumberHexPredicate(relation, valueString);
            }

            private class NumberHexPredicate : ComparableValuePredicate<ILine, string>
            {
                public NumberHexPredicate(char op, string numberHex) : base(op, numberHex)
                { }

                public override string GetValue(ILine item)
                {
                    return item.NumberHex;
                }
            }
        }

        private class FilterByCommonName : PredicateAdder<ILine>
        {
            public FilterByCommonName(List<CommandBase> subcommands, List<IPredicate<ILine>> predicates) :
                base(subcommands, predicates, "commonname", "=<>", false)
            { }

            protected override IPredicate<ILine> CreatePredicate(char relation, string valueString)
            {
                return new CommonNamePredicate(relation, valueString);
            }

            private class CommonNamePredicate : ComparableValuePredicate<ILine, string>
            {
                public CommonNamePredicate(char op, string commonName) : base(op, commonName)
                { }

                public override string GetValue(ILine item)
                {
                    return item.CommonName;
                }
            }
        }
    }

    class StopFilter : CollectionSelector<IStop>
    {
        public StopFilter(bool enableFiltering = true) : base(BTMSystem.GetInstance().Stops, "stop")
        {
            if (!enableFiltering) return;
            subcommands.Add(new FilterById(subcommands, collectionFilters));
            subcommands.Add(new FilterByName(subcommands, collectionFilters));
            subcommands.Add(new FilterByType(subcommands, collectionFilters));
        }

        private class FilterById : PredicateAdder<IStop>
        {
            public FilterById(List<CommandBase> subcommands, List<IPredicate<IStop>> predicates) :
                base(subcommands, predicates, "id", "=<>", true)
            { }

            protected override IPredicate<IStop> CreatePredicate(char relation, string valueString)
            {
                return new IdPredicate(relation, int.Parse(valueString));
            }

            private class IdPredicate : ComparableValuePredicate<IStop, int>
            {
                public IdPredicate(char op, int id) : base(op, id)
                { }

                public override int GetValue(IStop item)
                {
                    return item.Id;
                }
            }
        }

        private class FilterByName : PredicateAdder<IStop>
        {
            public FilterByName(List<CommandBase> subcommands, List<IPredicate<IStop>> predicates) :
                base(subcommands, predicates, "name", "=<>", false)
            { }

            protected override IPredicate<IStop> CreatePredicate(char relation, string valueString)
            {
                return new NamePredicate(relation, valueString);
            }

            private class NamePredicate : ComparableValuePredicate<IStop, string>
            {
                public NamePredicate(char op, string name) : base(op, name)
                { }

                public override string GetValue(IStop item)
                {
                    return item.Name;
                }
            }
        }

        private class FilterByType : PredicateAdder<IStop>
        {
            public FilterByType(List<CommandBase> subcommands, List<IPredicate<IStop>> predicates) :
                base(subcommands, predicates, "type", "=<>", false)
            { }

            protected override IPredicate<IStop> CreatePredicate(char relation, string valueString)
            {
                return new TypePredicate(relation, valueString);
            }

            private class TypePredicate : ComparableValuePredicate<IStop, string>
            {
                public TypePredicate(char op, string type) : base(op, type)
                { }

                public override string GetValue(IStop item)
                {
                    return item.Type;
                }
            }
        }
    }

    class BytebusFilter : CollectionSelector<IBytebus>
    {
        public BytebusFilter(bool enableFiltering = true) : base(BTMSystem.GetInstance().Buses, "bytebus")
        {
            if (!enableFiltering) return;
            subcommands.Add(new FilterById(subcommands, collectionFilters));
            subcommands.Add(new FilterByEngine(subcommands, collectionFilters));
        }

        private class FilterByEngine : PredicateAdder<IBytebus>
        {
            public FilterByEngine(List<CommandBase> subcommands, List<IPredicate<IBytebus>> predicates) :
                base(subcommands, predicates, "engine", "=<>", false)
            { }

            protected override IPredicate<IBytebus> CreatePredicate(char relation, string valueString)
            {
                return new EnginePredicate(relation, valueString);
            }

            private class EnginePredicate : ComparableValuePredicate<IBytebus, string>
            {
                public EnginePredicate(char op, string name) : base(op, name)
                { }

                public override string GetValue(IBytebus item)
                {
                    return item.EngineClass;
                }
            }
        }

        private class FilterById : PredicateAdder<IBytebus>
        {
            public FilterById(List<CommandBase> subcommands, List<IPredicate<IBytebus>> predicates) :
                base(subcommands, predicates, "id", "=<>", true)
            { }

            protected override IPredicate<IBytebus> CreatePredicate(char relation, string valueString)
            {
                return new IdPredicate(relation, int.Parse(valueString));
            }

            private class IdPredicate : ComparableValuePredicate<IBytebus, int>
            {
                public IdPredicate(char op, int id) : base(op, id)
                { }

                public override int GetValue(IBytebus item)
                {
                    return item.Id;
                }
            }
        }
    }

    class TramFilter : CollectionSelector<ITram>
    {
        public TramFilter(bool enableFiltering = true) : base(BTMSystem.GetInstance().Trams, "tram")
        {
            if (!enableFiltering) return;
            subcommands.Add(new FilterById(subcommands, collectionFilters));
            subcommands.Add(new FilterByCarsNumber(subcommands, collectionFilters));
        }

        private class FilterByCarsNumber : PredicateAdder<ITram>
        {
            public FilterByCarsNumber(List<CommandBase> subcommands, List<IPredicate<ITram>> predicates) :
                base(subcommands, predicates, "carsnumber", "=<>", true)
            { }

            protected override IPredicate<ITram> CreatePredicate(char relation, string valueString)
            {
                return new CarsNumberPredicate(relation, int.Parse(valueString));
            }

            private class CarsNumberPredicate : ComparableValuePredicate<ITram, int>
            {
                public CarsNumberPredicate(char op, int cars) : base(op, cars)
                { }

                public override int GetValue(ITram item)
                {
                    return item.CarsNumber;
                }
            }
        }

        private class FilterById : PredicateAdder<ITram>
        {
            public FilterById(List<CommandBase> subcommands, List<IPredicate<ITram>> predicates) :
                base(subcommands, predicates, "id", "=<>", true)
            { }

            protected override IPredicate<ITram> CreatePredicate(char relation, string valueString)
            {
                return new IdPredicate(relation, int.Parse(valueString));
            }

            private class IdPredicate : ComparableValuePredicate<ITram, int>
            {
                public IdPredicate(char op, int id) : base(op, id)
                { }

                public override int GetValue(ITram item)
                {
                    return item.Id;
                }
            }
        }
    }

    class VehicleFilter : CollectionSelector<IVehicle>
    {
        public VehicleFilter(bool enableFiltering = true) : base(BTMSystem.GetInstance().Vehicles, "vehicle")
        {
            if (!enableFiltering) return;
            subcommands.Add(new FilterById(subcommands, collectionFilters));
        }

        private class FilterById : PredicateAdder<IVehicle>
        {
            public FilterById(List<CommandBase> subcommands, List<IPredicate<IVehicle>> predicates) :
                base(subcommands, predicates, "id", "=<>", true)
            { }

            protected override IPredicate<IVehicle> CreatePredicate(char relation, string valueString)
            {
                return new IdPredicate(relation, int.Parse(valueString));
            }

            private class IdPredicate : ComparableValuePredicate<IVehicle, int>
            {
                public IdPredicate(char op, int id) : base(op, id)
                { }

                public override int GetValue(IVehicle item)
                {
                    return item.Id;
                }
            }
        }
    }

    class DriverFilter : CollectionSelector<IDriver>
    {
        public DriverFilter(bool enableFiltering = true) : base(BTMSystem.GetInstance().Drivers, "driver")
        {
            if (!enableFiltering) return;
            subcommands.Add(new FilterByName(subcommands, collectionFilters));
            subcommands.Add(new FilterBySurname(subcommands, collectionFilters));
            subcommands.Add(new FilterBySeniority(subcommands, collectionFilters));
        }

        private class FilterByName : PredicateAdder<IDriver>
        {
            public FilterByName(List<CommandBase> subcommands, List<IPredicate<IDriver>> predicates) :
                base(subcommands, predicates, "name", "=<>", false)
            { }

            protected override IPredicate<IDriver> CreatePredicate(char relation, string valueString)
            {
                return new NamePredicate(relation, valueString);
            }

            private class NamePredicate : ComparableValuePredicate<IDriver, string>
            {
                public NamePredicate(char op, string name) : base(op, name)
                { }

                public override string GetValue(IDriver item)
                {
                    return item.Name;
                }
            }
        }

        private class FilterBySurname : PredicateAdder<IDriver>
        {
            public FilterBySurname(List<CommandBase> subcommands, List<IPredicate<IDriver>> predicates) :
                base(subcommands, predicates, "surname", "=<>", false)
            { }
            protected override IPredicate<IDriver> CreatePredicate(char relation, string valueString)
            {
                return new SurnamePredicate(relation, valueString);
            }

            private class SurnamePredicate : ComparableValuePredicate<IDriver, string>
            {
                public SurnamePredicate(char op, string surname) : base(op, surname)
                { }

                public override string GetValue(IDriver item)
                {
                    return item.Surname;
                }
            }
        }

        private class FilterBySeniority : PredicateAdder<IDriver>
        {
            public FilterBySeniority(List<CommandBase> subcommands, List<IPredicate<IDriver>> predicates) :
                base(subcommands, predicates, "seniority", "=<>", true)
            { }

            protected override IPredicate<IDriver> CreatePredicate(char relation, string valueString)
            {
                return new SeniorityPredicate(relation, int.Parse(valueString));
            }

            private class SeniorityPredicate : ComparableValuePredicate<IDriver, int>
            {
                public SeniorityPredicate(char op, int seniority) : base(op, seniority)
                { }

                public override int GetValue(IDriver item)
                {
                    return item.Seniority;
                }
            }
        }
    }

    class DisplayFilteredCollection<BTMBase> : CommandBase where BTMBase : IBTMBase
    {
        private IBTMCollection<BTMBase> collection;
        private ActionsIfPredicates<BTMBase> filteredPrint;
        private List<IPredicate<BTMBase>> predicates;

        public DisplayFilteredCollection(IBTMCollection<BTMBase> collection, List<IPredicate<BTMBase>> predicates) 
        {
            this.collection = collection;
            filteredPrint = new ActionsIfPredicates<BTMBase>(new Print<BTMBase>(), predicates);
            this.predicates = predicates;
        }
        
        public override bool Check(string input)
        {
            return input == "";
        }

        public override string Process(string input)
        {
            ForEach(collection.First(), filteredPrint);

            predicates.Clear();

            return input;
        }
    }

    abstract class ObjectFieldSetter<BTMBase> : FieldValueProcesser<BTMBase> where BTMBase : IBTMBase
    {
        public ObjectFieldSetter(List<CommandBase> subcommands, string fieldLower, bool numeric) : base(subcommands, fieldLower, numeric)
        { }

        public override string Process(string input)
        {
            string valueString = ExtractInputComponents(input).Item3;

            SetFieldValueTo(valueString);

            return Console.ReadLine();
        }

        protected abstract void SetFieldValueTo(string value);
    }

    class ListCommand : CommandBase
    {
        public ListCommand() : base(new List<CommandBase>()
        {
            new DisplayLines(),
            new DisplayStops(),
            new DisplayBuses(),
            new DisplayTrams(),
            new DisplayVehicles(),
            new DisplayDrivers()
        })
        { }

        public override bool Check(string input)
        {
            return input.StartsWith("list ");
        }

        public override string Process(string input)
        {
            return input.Remove(0, 4);
        }

        private class DisplayLines : LineFilter
        {
            public DisplayLines() : base(false)
            {
                subcommands.Add(new DisplayFilteredCollection<ILine>(collection, collectionFilters));
            }
        }

        private class DisplayStops : StopFilter
        {
            public DisplayStops() : base(false)
            {
                subcommands.Add(new DisplayFilteredCollection<IStop>(collection, collectionFilters));
            }
        }

        private class DisplayBuses : BytebusFilter
        {
            public DisplayBuses() : base(false)
            {
                subcommands.Add(new DisplayFilteredCollection<IBytebus>(collection, collectionFilters));
            }
        }

        private class DisplayTrams : TramFilter
        {
            public DisplayTrams() : base(false)
            {
                subcommands.Add(new DisplayFilteredCollection<ITram>(collection, collectionFilters));
            }
        }

        private class DisplayVehicles : VehicleFilter
        {
            public DisplayVehicles() : base(false)
            {
                subcommands.Add(new DisplayFilteredCollection<IVehicle>(collection, collectionFilters));
            }
        }

        private class DisplayDrivers : DriverFilter
        {
            public DisplayDrivers() : base(false)
            {
                subcommands.Add(new DisplayFilteredCollection<IDriver>(collection, collectionFilters));
            }
        }
    }

    class FindCommand : CommandBase
    {
        public FindCommand() : base(new List<CommandBase>() { 
            new DisplayFilteredLines(), 
            new DisplayFilteredStops(),
            new DisplayFilteredBuses(), 
            new DisplayFilteredTrams(), 
            new DisplayFilteredVehicles(),
            new DisplayFilteredDrivers()
        })
        { }
        
        public override bool Check(string input)
        {
            return input.ToLower().StartsWith("find ");
        }

        public override string Process(string input)
        {
            return input.Remove(0, 4);
        }

        private class DisplayFilteredLines : LineFilter
        {
            public DisplayFilteredLines()
            {
                subcommands.Add(new DisplayFilteredCollection<ILine>(collection, collectionFilters));
            }
        }

        private class DisplayFilteredStops : StopFilter
        {
            public DisplayFilteredStops()
            {
                subcommands.Add(new DisplayFilteredCollection<IStop>(collection, collectionFilters));
            }
        }

        private class DisplayFilteredBuses : BytebusFilter
        {
            public DisplayFilteredBuses()
            {
                subcommands.Add(new DisplayFilteredCollection<IBytebus>(collection, collectionFilters));
            }
        }

        private class DisplayFilteredTrams : TramFilter
        {
            public DisplayFilteredTrams()
            {
                subcommands.Add(new DisplayFilteredCollection<ITram>(collection, collectionFilters));
            }
        }

        private class DisplayFilteredVehicles : VehicleFilter
        {
            public DisplayFilteredVehicles()
            {
                subcommands.Add(new DisplayFilteredCollection<IVehicle>(collection, collectionFilters));
            }
        }

        private class DisplayFilteredDrivers : DriverFilter
        {
            public DisplayFilteredDrivers()
            {
                subcommands.Add(new DisplayFilteredCollection<IDriver>(collection, collectionFilters));
            }
        }
    }

    class AddCommand : CommandBase
    {
        public AddCommand() : base(new List<CommandBase>()
        {
            new AddLine(), 
            new AddStop(), 
            new AddBus(), 
            new AddTram(), 
            new AddDriver()
        })
        { }
        
        public override bool Check(string input)
        {
            return input.StartsWith("add ");
        }

        public override string Process(string input)
        {
            return input.Remove(0, 3);
        }

        private class AddLine : LineFilter
        {
            private const string fieldsDescription = "`numberDec`: numeric, `numberHex`: string, `commonName`: string";

            public AddLine() : base(false)
            {
                subcommands.Add(new SelectLineBaseBuilder(collection));
                subcommands.Add(new SelectLineTextBuilder(collection));
            }

            private class SelectLineBaseBuilder : SelectBuilder<ILine, ILineBuilder>
            {
                public SelectLineBaseBuilder(IBTMCollection<ILine> collection) : 
                    base(collection, new LineBaseBuilder(), "base", fieldsDescription)
                {
                    subcommands.Add(new LineNumberDecSetter(subcommands, builder));
                    subcommands.Add(new LineNumberHexSetter(subcommands, builder));
                    subcommands.Add(new LineCommonNameSetter(subcommands, builder));
                    subcommands.Add(new SoftError(subcommands, fieldsDescription));
                }
            }

            private class SelectLineTextBuilder : SelectBuilder<ILine, ILineBuilder>
            {
                public SelectLineTextBuilder(IBTMCollection<ILine> collection) : 
                    base(collection, new LineTextBuilder(), "secondary", fieldsDescription)
                {
                    subcommands.Add(new LineNumberDecSetter(subcommands, builder));
                    subcommands.Add(new LineNumberHexSetter(subcommands, builder));
                    subcommands.Add(new LineCommonNameSetter(subcommands, builder));
                    subcommands.Add(new SoftError(subcommands, fieldsDescription));
                }
            }

            private class LineNumberDecSetter : ObjectFieldSetter<ILine>
            {
                private ILineBuilder builder;

                public LineNumberDecSetter(List<CommandBase> subcommands, ILineBuilder builder) :
                    base(subcommands, "numberdec", true)
                {
                    this.builder = builder;
                }

                protected override void SetFieldValueTo(string value)
                {
                    builder.AddNumberDec(int.Parse(value));
                }
            }

            private class LineNumberHexSetter : ObjectFieldSetter<ILine>
            {
                private ILineBuilder builder;

                public LineNumberHexSetter(List<CommandBase> subcommands, ILineBuilder builder) :
                    base(subcommands, "numberhex", false)
                {
                    this.builder = builder;
                }

                protected override void SetFieldValueTo(string value)
                {
                    builder.AddNumberHex(value);
                }
            }

            private class LineCommonNameSetter : ObjectFieldSetter<ILine>
            {
                private ILineBuilder builder;

                public LineCommonNameSetter(List<CommandBase> subcommands, ILineBuilder builder) :
                    base(subcommands, "commonname", false)
                {
                    this.builder = builder;
                }

                protected override void SetFieldValueTo(string value)
                {
                    builder.AddCommonName(value);
                }
            }
        }

        private class AddStop : StopFilter
        {
            private const string fieldsDescription = "`id`: numeric, `name`: string, `type`: string";

            public AddStop() : base(false)
            {
                subcommands.Add(new SelectStopBaseBuilder(collection));
                subcommands.Add(new SelectStopTextBuilder(collection));
            }

            private class SelectStopBaseBuilder : SelectBuilder<IStop, IStopBuilder>
            {
                public SelectStopBaseBuilder(IBTMCollection<IStop> collection) : 
                    base(collection, new StopBaseBuilder(), "base", fieldsDescription)
                {
                    subcommands.Add(new StopIdSetter(subcommands, builder));
                    subcommands.Add(new StopNameSetter(subcommands, builder));
                    subcommands.Add(new StopTypeSetter(subcommands, builder));
                    subcommands.Add(new SoftError(subcommands, fieldsDescription));
                }
            }

            private class SelectStopTextBuilder : SelectBuilder<IStop, IStopBuilder>
            {
                public SelectStopTextBuilder(IBTMCollection<IStop> collection) : 
                    base(collection, new StopTextBuilder(), "secondary", fieldsDescription)
                {
                    subcommands.Add(new StopIdSetter(subcommands, builder));
                    subcommands.Add(new StopNameSetter(subcommands, builder));
                    subcommands.Add(new StopTypeSetter(subcommands, builder));
                    subcommands.Add(new SoftError(subcommands, fieldsDescription));
                }
            }

            private class StopIdSetter : ObjectFieldSetter<IStop>
            {
                private IStopBuilder builder;

                public StopIdSetter(List<CommandBase> subcommands, IStopBuilder builder) :
                    base(subcommands, "id", true)
                {
                    this.builder = builder;
                }

                protected override void SetFieldValueTo(string value)
                {
                    builder.AddId(int.Parse(value));
                }
            }

            private class StopNameSetter : ObjectFieldSetter<IStop>
            {
                private IStopBuilder builder;

                public StopNameSetter(List<CommandBase> subcommands, IStopBuilder builder) :
                    base(subcommands, "name", false)
                {
                    this.builder = builder;
                }

                protected override void SetFieldValueTo(string value)
                {
                    builder.AddName(value);
                }
            }

            private class StopTypeSetter : ObjectFieldSetter<IStop>
            {
                private IStopBuilder builder;

                public StopTypeSetter(List<CommandBase> subcommands, IStopBuilder builder) :
                    base(subcommands, "type", false)
                {
                    this.builder = builder;
                }

                protected override void SetFieldValueTo(string value)
                {
                    builder.AddStopType(value);
                }
            }
        }

        private class AddBus : BytebusFilter
        {
            private const string fieldsDescription = "`id`: numeric, `engine`: string";

            public AddBus() : base(false)
            {
                subcommands.Add(new SelectBusBaseBuilder(collection));
                subcommands.Add(new SelectBusTextBuilder(collection));
            }

            private class SelectBusBaseBuilder : SelectBuilder<IBytebus, IBytebusBuilder>
            {
                public SelectBusBaseBuilder(IBTMCollection<IBytebus> collection) : 
                    base(collection, new BytebusBaseBuilder(), "base", fieldsDescription)
                {
                    subcommands.Add(new BusIdSetter(subcommands, builder));
                    subcommands.Add(new BusEngineSetter(subcommands, builder));
                    subcommands.Add(new SoftError(subcommands, fieldsDescription));
                }
            }

            private class SelectBusTextBuilder : SelectBuilder<IBytebus, IBytebusBuilder>
            {
                public SelectBusTextBuilder(IBTMCollection<IBytebus> collection) : 
                    base(collection, new BytebusTextBuilder(), "secondary", fieldsDescription)
                {
                    subcommands.Add(new BusIdSetter(subcommands, builder));
                    subcommands.Add(new BusEngineSetter(subcommands, builder));
                    subcommands.Add(new SoftError(subcommands, fieldsDescription));
                }
            }

            private class BusIdSetter : ObjectFieldSetter<IBytebus>
            {
                private IBytebusBuilder builder;

                public BusIdSetter(List<CommandBase> subcommands, IBytebusBuilder builder) :
                    base(subcommands, "id", true)
                {
                    this.builder = builder;
                }

                protected override void SetFieldValueTo(string value)
                {
                    builder.AddId(int.Parse(value));
                }
            }

            private class BusEngineSetter : ObjectFieldSetter<IBytebus>
            {
                private IBytebusBuilder builder;

                public BusEngineSetter(List<CommandBase> subcommands, IBytebusBuilder builder) :
                    base(subcommands, "engine", false)
                {
                    this.builder = builder;
                }

                protected override void SetFieldValueTo(string value)
                {
                    builder.AddEngine(value);
                }
            }
        }

        private class AddTram : TramFilter
        {
            private const string fieldsDescription = "`id`: numeric, `carsNumber`: numeric";

            public AddTram() : base(false)
            {
                subcommands.Add(new SelectTramBaseBuilder(collection));
                subcommands.Add(new SelectTramTextBuilder(collection));
            }

            private class SelectTramBaseBuilder : SelectBuilder<ITram, ITramBuilder>
            {
                public SelectTramBaseBuilder(IBTMCollection<ITram> collection) : 
                    base(collection, new TramBaseBuilder(), "base", fieldsDescription)
                {
                    subcommands.Add(new TramIdSetter(subcommands, builder));
                    subcommands.Add(new TramCarsNumberSetter(subcommands, builder));
                    subcommands.Add(new SoftError(subcommands, fieldsDescription));
                }
            }

            private class SelectTramTextBuilder : SelectBuilder<ITram, ITramBuilder>
            {
                public SelectTramTextBuilder(IBTMCollection<ITram> collection) : base(
                    collection,
                    new TramTextBuilder(),
                    "secondary",
                    fieldsDescription
                    )
                {
                    subcommands.Add(new TramIdSetter(subcommands, builder));
                    subcommands.Add(new TramCarsNumberSetter(subcommands, builder));
                    subcommands.Add(new SoftError(subcommands, fieldsDescription));
                }
            }

            private class TramIdSetter : ObjectFieldSetter<ITram>
            {
                private ITramBuilder builder;

                public TramIdSetter(List<CommandBase> subcommands, ITramBuilder builder) :
                    base(subcommands, "id", true)
                {
                    this.builder = builder;
                }

                protected override void SetFieldValueTo(string value)
                {
                    builder.AddId(int.Parse(value));
                }
            }

            private class TramCarsNumberSetter : ObjectFieldSetter<ITram>
            {
                private ITramBuilder builder;

                public TramCarsNumberSetter(List<CommandBase> subcommands, ITramBuilder builder) :
                    base(subcommands, "carsnumber", true)
                {
                    this.builder = builder;
                }

                protected override void SetFieldValueTo(string value)
                {
                    builder.AddCarsNumber(int.Parse(value));
                }
            }
        }

        private class AddDriver : DriverFilter
        {
            private const string fieldsDescription = "`name`: string, `surname`: string, `seniority`: numeric";

            public AddDriver() : base(false)
            {
                subcommands.Add(new SelectDriverBaseBuilder(collection));
                subcommands.Add(new SelectDriverTextBuilder(collection));
            }

            private class SelectDriverBaseBuilder : SelectBuilder<IDriver, IDriverBuilder>
            {
                public SelectDriverBaseBuilder(IBTMCollection<IDriver> collection) : 
                    base(collection, new DriverBaseBuilder(), "base", fieldsDescription)
                {
                    subcommands.Add(new DriverNameSetter(subcommands, builder));
                    subcommands.Add(new DriverSurnameSetter(subcommands, builder));
                    subcommands.Add(new DriverSenioritySetter(subcommands, builder));
                    subcommands.Add(new SoftError(subcommands, fieldsDescription));
                }
            }

            private class SelectDriverTextBuilder : SelectBuilder<IDriver, IDriverBuilder>
            {
                public SelectDriverTextBuilder(IBTMCollection<IDriver> collection) : 
                    base(collection, new DriverTextBuilder(), "secondary", fieldsDescription)
                {
                    subcommands.Add(new DriverNameSetter(subcommands, builder));
                    subcommands.Add(new DriverSurnameSetter(subcommands, builder));
                    subcommands.Add(new DriverSenioritySetter(subcommands, builder));
                    subcommands.Add(new SoftError(subcommands, fieldsDescription));
                }
            }

            private class DriverNameSetter : ObjectFieldSetter<IDriver>
            {
                private IDriverBuilder builder;

                public DriverNameSetter(List<CommandBase> subcommands, IDriverBuilder builder) :
                    base(subcommands, "name", false)
                {
                    this.builder = builder;
                }

                protected override void SetFieldValueTo(string value)
                {
                    builder.AddName(value);
                }
            }

            private class DriverSurnameSetter : ObjectFieldSetter<IDriver>
            {
                private IDriverBuilder builder;

                public DriverSurnameSetter(List<CommandBase> subcommands, IDriverBuilder builder) :
                    base(subcommands, "surname", false)
                {
                    this.builder = builder;
                }

                protected override void SetFieldValueTo(string value)
                {
                    builder.AddSurname(value);
                }
            }

            private class DriverSenioritySetter : ObjectFieldSetter<IDriver>
            {
                private IDriverBuilder builder;

                public DriverSenioritySetter(List<CommandBase> subcommands, IDriverBuilder builder) :
                    base(subcommands, "seniority", true)
                {
                    this.builder = builder;
                }

                protected override void SetFieldValueTo(string value)
                {
                    builder.AddSeniority(int.Parse(value));
                }
            }
        }

        private class SelectBuilder<BTMBase, BTMBuilder> : CommandBase 
            where BTMBase : IBTMBase
            where BTMBuilder : IBTMBuilder<BTMBase>
        {
            private string builderName, fieldsDescription;
            protected BTMBuilder builder;
            
            public SelectBuilder(IBTMCollection<BTMBase> collection, BTMBuilder builder, string builderName, string fieldsDescription)
            {
                this.builder = builder;
                this.builderName = builderName;
                this.fieldsDescription = fieldsDescription;

                subcommands.Add(new Confirm<BTMBase>(collection, builder));
                subcommands.Add(new Discard<BTMBase>(builder));
            }

            public override bool Check(string input)
            {
                return input.StartsWith(builderName) &&
                    (input.Length == builderName.Length || input[builderName.Length] == ' ');
            }

            public override string Process(string input)
            {
                builder.Reset();
                Console.WriteLine($"[{fieldsDescription}]");

                return Console.ReadLine();
            }
        }

        private class SoftError : CommandBase
        {
            private string fieldsDescription;

            public SoftError(List<CommandBase> subcommands, string fieldsDescription) :
                base(subcommands)
            {
                this.fieldsDescription = fieldsDescription;
            }
            
            public override bool Check(string input)
            {
                return true;
            }

            public override string Process(string input)
            {
                Console.WriteLine($"Incorrect field or data type. Please refer to: \n[{fieldsDescription}]");
                return Console.ReadLine();
            }
        }

        private class Confirm<BTMBase> : CommandBase where BTMBase : IBTMBase
        {
            private IBTMCollection<BTMBase> collection;
            private IBTMBuilder<BTMBase> builder;

            public Confirm(IBTMCollection<BTMBase> collection, IBTMBuilder<BTMBase> builder)
            {
                this.collection = collection;
                this.builder = builder;
            }

            public override bool Check(string input)
            {
                return input.ToLower().Trim() == "done";
            }

            public override string Process(string input)
            {
                BTMBase result = builder.Result();
                collection.Add(result);

                Console.WriteLine($"Creation successful: {result}.");

                return "";
            }
        }

        private class Discard<BTMBase> : CommandBase where BTMBase : IBTMBase
        {
            private IBTMBuilder<BTMBase> builder;

            public Discard(IBTMBuilder<BTMBase> builder)
            {
                this.builder = builder;
            }

            public override bool Check(string input)
            {
                return input.ToLower().Trim() == "exit";
            }

            public override string Process(string input)
            {
                builder.Reset();

                Console.WriteLine("Creation abandoned.");

                return "";
            }
        }
    }

    class EditCommand : CommandBase
    {
        public EditCommand() : base(new List<CommandBase>()
        {
            new EditLines(),
            new EditStops(),
            new EditBytebuses(),
            new EditTrams(), 
            new EditVehicles(), 
            new EditDrivers()
        })
        { }
        
        public override bool Check(string input)
        {
            return input.ToLower().StartsWith("edit");
        }

        public override string Process(string input)
        {
            return input.Remove(0, 4);
        }

        private class Confirm<BTMBase> : CommandBase where BTMBase : IBTMBase
        {
            private IBTMCollection<BTMBase> collection;
            private IPredicate<BTMBase> filter;
            private List<IAction<BTMBase>> actions;

            public Confirm(IBTMCollection<BTMBase> collection, IPredicate<BTMBase> filter, List<IAction<BTMBase>> actions)
            {
                this.collection = collection;
                this.filter = filter;
                this.actions = actions;
            }

            public override bool Check(string input)
            {
                return input.ToLower().Trim() == "done";
            }

            public override string Process(string input)
            {
                if (CountIf(collection.First(), filter) != 1)
                {
                    Console.WriteLine("Specified conditions do not satisfy one record.");
                    return "";
                }

                ForEach(collection.First(), new ActionsIf<BTMBase>(actions, filter));

                Console.WriteLine("Success.");

                return "";
            }
        }

        private class EditLines : LineFilter
        {
            private List<IAction<ILine>> actions;
            
            public EditLines()
            {
                actions = new List<IAction<ILine>>();
                subcommands.Add(new NumberDecSetterAdder(subcommands, actions));
                subcommands.Add(new NumberHexSetterAdder(subcommands, actions));
                subcommands.Add(new CommonNameSetterAdder(subcommands, actions));
            }

            private class NumberDecSetterAdder : ObjectFieldSetter<ILine>
            {
                private List<IAction<ILine>> actions;

                public NumberDecSetterAdder(List<CommandBase> subcommands, List<IAction<ILine>> actions) :
                    base(subcommands, "numberdec", true)
                {
                    this.actions = actions;
                }
                
                protected override void SetFieldValueTo(string value)
                {
                    actions.Add(new SetNumberDec(int.Parse(value)));
                }

                private class SetNumberDec : IAction<ILine>
                {
                    private int value;

                    public SetNumberDec(int value)
                    {
                        this.value = value;
                    }
                    
                    public void Eval(ILine item)
                    {
                        item.NumberDec = value;
                    }
                }
            }

            private class NumberHexSetterAdder : ObjectFieldSetter<ILine>
            {
                private List<IAction<ILine>> actions;

                public NumberHexSetterAdder(List<CommandBase> subcommands, List<IAction<ILine>> actions) :
                    base(subcommands, "numberhex", false)
                {
                    this.actions = actions;
                }

                protected override void SetFieldValueTo(string value)
                {
                    actions.Add(new SetNumberHex(value));
                }

                private class SetNumberHex : IAction<ILine>
                {
                    private string value;

                    public SetNumberHex(string value)
                    {
                        this.value = value;
                    }

                    public void Eval(ILine item)
                    {
                        item.NumberHex = value;
                    }
                }
            }

            private class CommonNameSetterAdder : ObjectFieldSetter<ILine>
            {
                private List<IAction<ILine>> actions;

                public CommonNameSetterAdder(List<CommandBase> subcommands, List<IAction<ILine>> actions) :
                    base(subcommands, "commonname", false)
                {
                    this.actions = actions;
                }

                protected override void SetFieldValueTo(string value)
                {
                    actions.Add(new SetCommonName(value));
                }

                private class SetCommonName : IAction<ILine>
                {
                    private string value;

                    public SetCommonName(string value)
                    {
                        this.value = value;
                    }

                    public void Eval(ILine item)
                    {
                        item.CommonName = value;
                    }
                }
            }
        }

        private class EditStops : StopFilter
        {
            public EditStops()
            {
                //subcommands.Add(...)
            }
        }

        private class EditBytebuses : BytebusFilter
        {
            public EditBytebuses()
            {
                //subcommands.Add(...)
            }
        }

        private class EditTrams : TramFilter
        {
            public EditTrams()
            {
                //subcommands.Add(...)
            }
        }

        private class EditVehicles : VehicleFilter
        {
            public EditVehicles()
            {
                //subcommands.Add(...)
            }
        }

        private class EditDrivers : DriverFilter
        {
            public EditDrivers()
            {
                //subcommands.Add(...)
            }
        }
    }
}
