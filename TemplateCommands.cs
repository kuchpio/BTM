using static BTM.CollectionUtils;

namespace BTM {

    interface ICommand
    {
        ICommandExecutor Execute(string input);
    }

    abstract class CommandBase : ICommand
    {
        protected List<CommandBase> subcommands;

        public CommandBase(List<CommandBase> subcommands)
        {
            this.subcommands = subcommands;
        }

        public CommandBase() : this(new List<CommandBase>())
        { }
        
        public abstract bool Check(string input);

        public abstract string Process(string input);

        public virtual ICommandExecutor Execute(string input)
        {
            input = Process(input);

            if (subcommands.Count == 0) return new EmptyExecutor();

            input = input.Trim();

            foreach (CommandBase command in subcommands)
            {
                if (command.Check(input))
                {
                    return command.Execute(input);
                }
            }
            
            Console.WriteLine($"Unknown command syntax near: \n`{input}`");
            return new EmptyExecutor();
        }
    }

    class KeywordConsumer : CommandBase
    {
        private string keyword;
        
        public KeywordConsumer(string keyword) : this(new List<CommandBase>(), keyword)
        { }
        public KeywordConsumer(CommandBase subcommand, string keyword) : this(new List<CommandBase>() { subcommand }, keyword)
        { }
        public KeywordConsumer(List<CommandBase> subcommands, string keyword) : base(subcommands)
        {
            this.keyword = keyword;
        }
        
        public override bool Check(string input)
        {
            return input.StartsWith(keyword) && (input.Length == keyword.Length || input[keyword.Length] == ' ');
        }

        public override string Process(string input)
        {
            Action();
            
            return input.Remove(0, keyword.Length);
        }

        public virtual void Action()
        { }
    }

    abstract class FieldValueProcesser<BTMBase> : CommandBase where BTMBase : IBTMBase
    {
        private readonly string fieldLower, operators;
        protected readonly bool numeric;

        public FieldValueProcesser(List<CommandBase> subcommands, string fieldLower, string operators, bool numeric) :
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

        public override string Process(string input)
        {
            (string _, char relation, string valueString, string remainingInput) = ExtractInputComponents(input);

            React(relation, valueString);

            return remainingInput;
        }

        protected abstract void React(char relation, string valueString);
    }

    interface FieldValueGetter<BTMBase, ValueType> where BTMBase : IBTMBase 
    {
        ValueType GetValue(BTMBase item);
    }

    interface FieldValueBuilder<BTMBase, BTMBaseBuilder, ValueType> 
        where BTMBase : IBTMBase
        where BTMBaseBuilder : IBTMBuilder<BTMBase>
    {
        void BuildValue(BTMBaseBuilder builder, ValueType value);
    }

    interface FieldValueSetter<BTMBase, ValueType> where BTMBase : IBTMBase 
    {
        void SetValue(BTMBase item, ValueType value);
    }

    abstract class NumericFieldFilterAdder<BTMBase> : FieldValueProcesser<BTMBase>, FieldValueGetter<BTMBase, int> where BTMBase : IBTMBase
    {
        protected All<BTMBase> filter;

        public NumericFieldFilterAdder(List<CommandBase> subcommands, All<BTMBase> filter, string fieldLowercase) : 
            base(subcommands, fieldLowercase, "=<>", true)
        {
            this.filter = filter;
        }

        protected override void React(char relation, string valueString)
        {
            filter.Add(new ComparableValuePredicate<BTMBase, int>(relation, int.Parse(valueString), this));
        }

        public abstract int GetValue(BTMBase item);
    }

    abstract class StringFieldFilterAdder<BTMBase> : FieldValueProcesser<BTMBase>, FieldValueGetter<BTMBase, string> where BTMBase : IBTMBase
    {
        protected All<BTMBase> filter;

        public StringFieldFilterAdder(List<CommandBase> subcommands, All<BTMBase> filter, string fieldLowercase) : 
            base(subcommands, fieldLowercase, "=<>", false)
        {
            this.filter = filter;
        }

        protected override void React(char relation, string valueString)
        {
            filter.Add(new ComparableValuePredicate<BTMBase, string>(relation, valueString, this));
        }

        public abstract string GetValue(BTMBase item);
    }

    class ComparableValuePredicate<BTMBase, ValueType> : IPredicate<BTMBase>
            where BTMBase : IBTMBase
            where ValueType : IComparable
    {
        private char op;
        private ValueType value;
        private FieldValueGetter<BTMBase, ValueType> getter;

        public ComparableValuePredicate(char op, ValueType value, FieldValueGetter<BTMBase, ValueType> getter)
        {
            this.op = op;
            this.value = value;
            this.getter = getter;
        }

        public bool Eval(BTMBase item)
        {
            int diff = getter.GetValue(item).CompareTo(value);
            switch (op)
            {
                case '=': return diff == 0;
                case '<': return diff < 0;
                case '>': return diff > 0;
            }
            return false;
        }
    }

    abstract class NumericFieldBuilderAdder<BTMBase, BTMBaseBuilder> : FieldValueProcesser<BTMBase>, FieldValueBuilder<BTMBase, BTMBaseBuilder, int> 
        where BTMBase : IBTMBase
        where BTMBaseBuilder : IBTMBuilder<BTMBase>
    {
        protected BTMBaseBuilder builder;
        
        public NumericFieldBuilderAdder(List<CommandBase> subcommands, BTMBaseBuilder builder, string fieldLowercase) :
            base(subcommands, fieldLowercase, "=", true)
        { 
            this.builder = builder;
        }

        protected override void React(char _, string valueString)
        {
            BuildValue(builder, int.Parse(valueString));
        }

        public abstract void BuildValue(BTMBaseBuilder builder, int value);
    }

    abstract class StringFieldBuilderAdder<BTMBase, BTMBaseBuilder> : FieldValueProcesser<BTMBase>, FieldValueBuilder<BTMBase, BTMBaseBuilder, string> 
        where BTMBase : IBTMBase
        where BTMBaseBuilder : IBTMBuilder<BTMBase>
    {
        protected BTMBaseBuilder builder;
        
        public StringFieldBuilderAdder(List<CommandBase> subcommands, BTMBaseBuilder builder, string fieldLowercase) :
            base(subcommands, fieldLowercase, "=", false)
        { 
            this.builder = builder;
        }

        protected override void React(char _, string valueString)
        {
            BuildValue(builder, valueString);
        }

        public abstract void BuildValue(BTMBaseBuilder builder, string value);
    }

    abstract class NumericFieldSetterAdder<BTMBase> : FieldValueProcesser<BTMBase>, FieldValueSetter<BTMBase, int> where BTMBase : IBTMBase
    {
        protected ActionSequence<BTMBase> setActions;
        
        public NumericFieldSetterAdder(List<CommandBase> subcommands, ActionSequence<BTMBase> setActions, string fieldLowercase) :
            base(subcommands, fieldLowercase, "=", true)
        { 
            this.setActions = setActions;
        }

        protected override void React(char _, string valueString)
        {
            setActions.Add(new SetAction<BTMBase, int>(int.Parse(valueString), this));
        }

        public abstract void SetValue(BTMBase item, int value);
    }

    abstract class StringFieldSetterAdder<BTMBase> : FieldValueProcesser<BTMBase>, FieldValueSetter<BTMBase, string> where BTMBase : IBTMBase
    {
        protected ActionSequence<BTMBase> setActions;
        
        public StringFieldSetterAdder(List<CommandBase> subcommands, ActionSequence<BTMBase> setActions, string fieldLowercase) :
            base(subcommands, fieldLowercase, "=", false)
        { 
            this.setActions = setActions;
        }

        protected override void React(char _, string valueString)
        {
            setActions.Add(new SetAction<BTMBase, string>(valueString, this));
        }

        public abstract void SetValue(BTMBase item, string value);
    }

    class SetAction<BTMBase, ValueType> : IAction<BTMBase> where BTMBase : IBTMBase
    {
        private ValueType value;
        private FieldValueSetter<BTMBase, ValueType> setter;

        public SetAction(ValueType value, FieldValueSetter<BTMBase, ValueType> setter)
        {
            this.value = value;
            this.setter = setter;
        }
        
        public void Eval(BTMBase item)
        {
            setter.SetValue(item, value);
        }
    }

    class CollectionSelector<BTMBase> : KeywordConsumer where BTMBase : IBTMBase
    {
        protected IBTMCollection<BTMBase> collection;
        protected All<BTMBase> collectionFilter;

        public CollectionSelector(IBTMCollection<BTMBase> collection, string collectionName) : 
            this(new List<CommandBase>(), collection, collectionName)
        { }

        public CollectionSelector(List<CommandBase> subcommands, IBTMCollection<BTMBase> collection, string collectionName) : 
            base(subcommands, collectionName)
        {
            this.collection = collection;
            collectionFilter = new All<BTMBase>();
        }

        public override void Action()
        {
            collectionFilter.Clear();
        }
    }

    class LineSelector : CollectionSelector<ILine> 
    {
        public LineSelector() : base(BTMSystem.GetInstance().Lines, "line")
        { }

        protected class NumberDecFilterAdder : NumericFieldFilterAdder<ILine>
        {
            public NumberDecFilterAdder(List<CommandBase> subcommands, All<ILine> filterSet) :
                base(subcommands, filterSet, "numberdec")
            { }

            public override int GetValue(ILine item)
            {
                return item.NumberDec;
            }
        }

        protected class NumberDecBuilderAdder : NumericFieldBuilderAdder<ILine, ILineBuilder>
        {
            public NumberDecBuilderAdder(List<CommandBase> subcommands, ILineBuilder builder) : 
                base(subcommands, builder, "numberdec")
            { }

            public override void BuildValue(ILineBuilder builder, int value)
            {
                builder.AddNumberDec(value);
            }
        }

        protected class NumberDecSetterAdder : NumericFieldSetterAdder<ILine> 
        {
            public NumberDecSetterAdder(List<CommandBase> subcommands, ActionSequence<ILine> setActions) : 
                base(subcommands, setActions, "numberdec")
            { }

            public override void SetValue(ILine item, int value)
            {
                item.NumberDec = value;
            }
        }

        protected class NumberHexFilterAdder : StringFieldFilterAdder<ILine>
        {
            public NumberHexFilterAdder(List<CommandBase> subcommands, All<ILine> filterSet) :
                base(subcommands, filterSet, "numberhex")
            { }

            public override string GetValue(ILine item)
            {
                return item.NumberHex;
            }
        }

        protected class NumberHexBuilderAdder : StringFieldBuilderAdder<ILine, ILineBuilder>
        {
            public NumberHexBuilderAdder(List<CommandBase> subcommands, ILineBuilder builder) : 
                base(subcommands, builder, "numberhex")
            { }

            public override void BuildValue(ILineBuilder builder, string value)
            {
                builder.AddNumberHex(value);
            }
        }

        protected class NumberHexSetterAdder : StringFieldSetterAdder<ILine> 
        {
            public NumberHexSetterAdder(List<CommandBase> subcommands, ActionSequence<ILine> setActions) : 
                base(subcommands, setActions, "numberhex")
            { }

            public override void SetValue(ILine item, string value)
            {
                item.NumberHex = value;
            }
        }

        protected class CommonNameFilterAdder : StringFieldFilterAdder<ILine>
        {
            public CommonNameFilterAdder(List<CommandBase> subcommands, All<ILine> filterSet) :
                base(subcommands, filterSet, "commonname")
            { }

            public override string GetValue(ILine item)
            {
                return item.CommonName;
            }
        }

        protected class CommonNameBuilderAdder : StringFieldBuilderAdder<ILine, ILineBuilder>
        {
            public CommonNameBuilderAdder(List<CommandBase> subcommands, ILineBuilder builder) : 
                base(subcommands, builder, "commonname")
            { }

            public override void BuildValue(ILineBuilder builder, string value)
            {
                builder.AddCommonName(value);
            }
        }

        protected class CommonNameSetterAdder : StringFieldSetterAdder<ILine> 
        {
            public CommonNameSetterAdder(List<CommandBase> subcommands, ActionSequence<ILine> setActions) : 
                base(subcommands, setActions, "commonname")
            { }

            public override void SetValue(ILine item, string value)
            {
                item.CommonName = value;
            }
        }
    }

    class StopSelector : CollectionSelector<IStop>
    {
        public StopSelector() : base(BTMSystem.GetInstance().Stops, "stop")
        { }

        protected class IdFilterAdder : NumericFieldFilterAdder<IStop>
        {
            public IdFilterAdder(List<CommandBase> subcommands, All<IStop> filterSet) :
                base(subcommands, filterSet, "id")
            { }

            public override int GetValue(IStop item)
            {
                return item.Id;
            }
        }

        protected class IdBuilderAdder : NumericFieldBuilderAdder<IStop, IStopBuilder>
        {
            public IdBuilderAdder(List<CommandBase> subcommands, IStopBuilder builder) : 
                base(subcommands, builder, "id")
            { }

            public override void BuildValue(IStopBuilder builder, int value)
            {
                builder.AddId(value);
            }
        }

        protected class IdSetterAdder : NumericFieldSetterAdder<IStop> 
        {
            public IdSetterAdder(List<CommandBase> subcommands, ActionSequence<IStop> setActions) : 
                base(subcommands, setActions, "id")
            { }

            public override void SetValue(IStop item, int value)
            {
                item.Id = value;
            }
        }

        protected class NameFilterAdder : StringFieldFilterAdder<IStop>
        {
            public NameFilterAdder(List<CommandBase> subcommands, All<IStop> filterSet) :
                base(subcommands, filterSet, "name")
            { }

            public override string GetValue(IStop item)
            {
                return item.Name;
            }
        }

        protected class NameBuilderAdder : StringFieldBuilderAdder<IStop, IStopBuilder>
        {
            public NameBuilderAdder(List<CommandBase> subcommands, IStopBuilder builder) : 
                base(subcommands, builder, "name")
            { }

            public override void BuildValue(IStopBuilder builder, string value)
            {
                builder.AddName(value);
            }
        }

        protected class NameSetterAdder : StringFieldSetterAdder<IStop> 
        {
            public NameSetterAdder(List<CommandBase> subcommands, ActionSequence<IStop> setActions) : 
                base(subcommands, setActions, "name")
            { }

            public override void SetValue(IStop item, string value)
            {
                item.Name = value;
            }
        }

        protected class TypeFilterAdder : StringFieldFilterAdder<IStop>
        {
            public TypeFilterAdder(List<CommandBase> subcommands, All<IStop> filterSet) :
                base(subcommands, filterSet, "type")
            { }

            public override string GetValue(IStop item)
            {
                return item.Type;
            }
        }

        protected class TypeBuilderAdder : StringFieldBuilderAdder<IStop, IStopBuilder>
        {
            public TypeBuilderAdder(List<CommandBase> subcommands, IStopBuilder builder) : 
                base(subcommands, builder, "type")
            { }

            public override void BuildValue(IStopBuilder builder, string value)
            {
                builder.AddStopType(value);
            }
        }

        protected class TypeSetterAdder : StringFieldSetterAdder<IStop> 
        {
            public TypeSetterAdder(List<CommandBase> subcommands, ActionSequence<IStop> setActions) : 
                base(subcommands, setActions, "type")
            { }

            public override void SetValue(IStop item, string value)
            {
                item.Type = value;
            }
        }
    }

    class BytebusSelector : CollectionSelector<IBytebus>
    {
        public BytebusSelector() : base(BTMSystem.GetInstance().Buses, "bytebus")
        { }

        protected class IdFilterAdder : NumericFieldFilterAdder<IBytebus>
        {
            public IdFilterAdder(List<CommandBase> subcommands, All<IBytebus> filterSet) :
                base(subcommands, filterSet, "id")
            { }

            public override int GetValue(IBytebus item)
            {
                return item.Id;
            }
        }

        protected class IdBuilderAdder : NumericFieldBuilderAdder<IBytebus, IBytebusBuilder>
        {
            public IdBuilderAdder(List<CommandBase> subcommands, IBytebusBuilder builder) : 
                base(subcommands, builder, "id")
            { }

            public override void BuildValue(IBytebusBuilder builder, int value)
            {
                builder.AddId(value);
            }
        }

        protected class IdSetterAdder : NumericFieldSetterAdder<IBytebus> 
        {
            public IdSetterAdder(List<CommandBase> subcommands, ActionSequence<IBytebus> setActions) : 
                base(subcommands, setActions, "id")
            { }

            public override void SetValue(IBytebus item, int value)
            {
                item.Id = value;
            }
        }

        protected class EngineFilterAdder : StringFieldFilterAdder<IBytebus>
        {
            public EngineFilterAdder(List<CommandBase> subcommands, All<IBytebus> filterSet) :
                base(subcommands, filterSet, "engine")
            { }

            public override string GetValue(IBytebus item)
            {
                return item.EngineClass;
            }
        }

        protected class EngineBuilderAdder : StringFieldBuilderAdder<IBytebus, IBytebusBuilder>
        {
            public EngineBuilderAdder(List<CommandBase> subcommands, IBytebusBuilder builder) : 
                base(subcommands, builder, "engine")
            { }

            public override void BuildValue(IBytebusBuilder builder, string value)
            {
                builder.AddEngine(value);
            }
        }

        protected class EngineSetterAdder : StringFieldSetterAdder<IBytebus> 
        {
            public EngineSetterAdder(List<CommandBase> subcommands, ActionSequence<IBytebus> setActions) : 
                base(subcommands, setActions, "engine")
            { }

            public override void SetValue(IBytebus item, string value)
            {
                item.EngineClass = value;
            }
        }
    }

    class TramSelector : CollectionSelector<ITram>
    {
        public TramSelector() : base(BTMSystem.GetInstance().Trams, "tram")
        { }

        protected class IdFilterAdder : NumericFieldFilterAdder<ITram>
        {
            public IdFilterAdder(List<CommandBase> subcommands, All<ITram> filterSet) :
                base(subcommands, filterSet, "id")
            { }

            public override int GetValue(ITram item)
            {
                return item.Id;
            }
        }

        protected class IdBuilderAdder : NumericFieldBuilderAdder<ITram, ITramBuilder>
        {
            public IdBuilderAdder(List<CommandBase> subcommands, ITramBuilder builder) : 
                base(subcommands, builder, "id")
            { }

            public override void BuildValue(ITramBuilder builder, int value)
            {
                builder.AddId(value);
            }
        }

        protected class IdSetterAdder : NumericFieldSetterAdder<ITram> 
        {
            public IdSetterAdder(List<CommandBase> subcommands, ActionSequence<ITram> setActions) : 
                base(subcommands, setActions, "id")
            { }

            public override void SetValue(ITram item, int value)
            {
                item.Id = value;
            }
        }

        protected class CarsNumberFilterAdder : NumericFieldFilterAdder<ITram>
        {
            public CarsNumberFilterAdder(List<CommandBase> subcommands, All<ITram> filterSet) :
                base(subcommands, filterSet, "carsnumber")
            { }

            public override int GetValue(ITram item)
            {
                return item.CarsNumber;
            }
        }

        protected class CarsNumberBuilderAdder : NumericFieldBuilderAdder<ITram, ITramBuilder>
        {
            public CarsNumberBuilderAdder(List<CommandBase> subcommands, ITramBuilder builder) : 
                base(subcommands, builder, "carsnumber")
            { }

            public override void BuildValue(ITramBuilder builder, int value)
            {
                builder.AddCarsNumber(value);
            }
        }

        protected class CarsNumberSetterAdder : NumericFieldSetterAdder<ITram> 
        {
            public CarsNumberSetterAdder(List<CommandBase> subcommands, ActionSequence<ITram> setActions) : 
                base(subcommands, setActions, "carsnumber")
            { }

            public override void SetValue(ITram item, int value)
            {
                item.CarsNumber = value;
            }
        }
    }

    class VehicleSelector : CollectionSelector<IVehicle>
    {
        public VehicleSelector() : base(BTMSystem.GetInstance().Vehicles, "vehicle")
        { }

        protected class IdFilterAdder : NumericFieldFilterAdder<IVehicle>
        {
            public IdFilterAdder(List<CommandBase> subcommands, All<IVehicle> filterSet) :
                base(subcommands, filterSet, "id")
            { }

            public override int GetValue(IVehicle item)
            {
                return item.Id;
            }
        }

        protected class IdSetterAdder : NumericFieldSetterAdder<IVehicle> 
        {
            public IdSetterAdder(List<CommandBase> subcommands, ActionSequence<IVehicle> setActions) : 
                base(subcommands, setActions, "id")
            { }

            public override void SetValue(IVehicle item, int value)
            {
                item.Id = value;
            }
        }
    }

    class DriverSelector : CollectionSelector<IDriver>
    {
        public DriverSelector() : base(BTMSystem.GetInstance().Drivers, "driver")
        { }

        protected class NameFilterAdder : StringFieldFilterAdder<IDriver>
        {
            public NameFilterAdder(List<CommandBase> subcommands, All<IDriver> filterSet) :
                base(subcommands, filterSet, "name")
            { }

            public override string GetValue(IDriver item)
            {
                return item.Name;
            }
        }

        protected class NameBuilderAdder : StringFieldBuilderAdder<IDriver, IDriverBuilder>
        {
            public NameBuilderAdder(List<CommandBase> subcommands, IDriverBuilder builder) : 
                base(subcommands, builder, "name")
            { }

            public override void BuildValue(IDriverBuilder builder, string value)
            {
                builder.AddName(value);
            }
        }

        protected class NameSetterAdder : StringFieldSetterAdder<IDriver> 
        {
            public NameSetterAdder(List<CommandBase> subcommands, ActionSequence<IDriver> setActions) : 
                base(subcommands, setActions, "name")
            { }

            public override void SetValue(IDriver item, string value)
            {
                item.Name = value;
            }
        }

        protected class SurnameFilterAdder : StringFieldFilterAdder<IDriver>
        {
            public SurnameFilterAdder(List<CommandBase> subcommands, All<IDriver> filterSet) :
                base(subcommands, filterSet, "surname")
            { }

            public override string GetValue(IDriver item)
            {
                return item.Surname;
            }
        }

        protected class SurnameBuilderAdder : StringFieldBuilderAdder<IDriver, IDriverBuilder>
        {
            public SurnameBuilderAdder(List<CommandBase> subcommands, IDriverBuilder builder) : 
                base(subcommands, builder, "surname")
            { }

            public override void BuildValue(IDriverBuilder builder, string value)
            {
                builder.AddSurname(value);
            }
        }

        protected class SurnameSetterAdder : StringFieldSetterAdder<IDriver> 
        {
            public SurnameSetterAdder(List<CommandBase> subcommands, ActionSequence<IDriver> setActions) : 
                base(subcommands, setActions, "surname")
            { }

            public override void SetValue(IDriver item, string value)
            {
                item.Surname = value;
            }
        }

        protected class SeniorityFilterAdder : NumericFieldFilterAdder<IDriver>
        {
            public SeniorityFilterAdder(List<CommandBase> subcommands, All<IDriver> filterSet) :
                base(subcommands, filterSet, "seniority")
            { }

            public override int GetValue(IDriver item)
            {
                return item.Seniority;
            }
        }

        protected class SeniorityBuilderAdder : NumericFieldBuilderAdder<IDriver, IDriverBuilder>
        {
            public SeniorityBuilderAdder(List<CommandBase> subcommands, IDriverBuilder builder) : 
                base(subcommands, builder, "seniority")
            { }

            public override void BuildValue(IDriverBuilder builder, int value)
            {
                builder.AddSeniority(value);
            }
        }

        protected class SenioritySetterAdder : NumericFieldSetterAdder<IDriver> 
        {
            public SenioritySetterAdder(List<CommandBase> subcommands, ActionSequence<IDriver> setActions) : 
                base(subcommands, setActions, "seniority")
            { }

            public override void SetValue(IDriver item, int value)
            {
                item.Seniority = value;
            }
        }
    }

    abstract class LineReader : CommandBase
    {   
        public LineReader(List<CommandBase> subcommands) : base(subcommands)
        { }
        
        public override bool Check(string input)
        {
            return true;
        }

        public override string Process(string input)
        {
            return GetNextLine();
        }

        public abstract string GetNextLine();
    }

    class ConsoleLineReader : LineReader
    {
        public ConsoleLineReader(List<CommandBase> subcommands) : base(subcommands)
        { }
        
        public override string GetNextLine()
        {
            return Console.ReadLine();
        }
    }

    abstract class MessageWriter : CommandBase
    {
        private string message;

        public MessageWriter(List<CommandBase> subcommands, string message) :
            base(subcommands)
        {
            this.message = message;
        }
        
        public override bool Check(string input)
        {
            return true;
        }

        public override string Process(string input)
        {
            Write(message);
            return input;
        }

        public abstract void Write(string message);
    }

    class ConsoleMessageWriter : MessageWriter
    {
        public ConsoleMessageWriter(List<CommandBase> subcommands, string message) : base(subcommands, message)
        { }

        public ConsoleMessageWriter(CommandBase subcommand, string message) : this(new List<CommandBase>() { subcommand }, message)
        { }

        public ConsoleMessageWriter(string message) : this(new List<CommandBase>(), message)
        { }

        public override void Write(string message)
        {
            Console.WriteLine(message);
        }
    }

    class RecordNumberVerifier<BTMBase> : CommandBase where BTMBase : IBTMBase
    {
        private IBTMCollection<BTMBase> collection;
        private All<BTMBase> filter;
        List<CommandBase> happySubcommands, sadSubcommands;
        int number;
        
        public RecordNumberVerifier(int number, List<CommandBase> happySubcommands, List<CommandBase> sadSubcommands, IBTMCollection<BTMBase> collection, All<BTMBase> filter)
        {
            this.number = number;
            this.collection = collection;
            this.filter = filter;
            this.happySubcommands = happySubcommands;
            this.sadSubcommands = sadSubcommands;
        }

        public RecordNumberVerifier(int number, CommandBase happySubcommand, CommandBase sadSubcommand, IBTMCollection<BTMBase> collection, All<BTMBase> filter) :
            this(number, new List<CommandBase>() { happySubcommand }, new List<CommandBase>() { sadSubcommand }, collection, filter)
        { }

        public override bool Check(string input)
        {
            return input == "";
        }

        public override string Process(string input)
        {
            subcommands = CountIf(collection.First(), filter) == number ? happySubcommands : sadSubcommands;
            
            return input;
        }
    }
}