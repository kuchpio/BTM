using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace BTM
{
    // In .Net 8.0 standard string will implement IParsable<string>
    // and this class won't be needed.
    struct ParsableString : IParsable<ParsableString>, IComparable<ParsableString>
    {
        private readonly string value;

        public ParsableString(string value)
        {
            this.value = value;
        }

        public static ParsableString Parse(string s, IFormatProvider provider) => s;
        public static bool TryParse([NotNullWhen(true)] string s, IFormatProvider provider, [MaybeNullWhen(false)] out ParsableString result)
        {
            result = s;
            return true;
        }
        public int CompareTo(ParsableString other) => value.CompareTo(other);
        public static implicit operator string(ParsableString ps) => ps.value;
        public static implicit operator ParsableString(string str) => new ParsableString(str);
        public override string ToString() => value;
    }

    abstract class FieldValueProcesser<BTMBase, ValueType> : CommandBase
        where BTMBase : IBTMBase
        where ValueType : IParsable<ValueType>
    {
        private readonly string fieldLower, operators;

        public FieldValueProcesser(List<CommandBase> subcommands, string fieldLower, string operators) :
            base(subcommands)
        {
            this.fieldLower = fieldLower;
            this.operators = operators;
        }

        public override bool Check(string input)
        {
            if (input.Length < fieldLower.Length + 2 ||
                !input.ToLower().StartsWith(fieldLower) ||
                !operators.Contains(input[fieldLower.Length]))
                return false;

            string valueString = ExtractInputComponents(input).Item3;

            if (valueString == null || !ValueType.TryParse(valueString, CultureInfo.InvariantCulture, out ValueType _))
                return false;

            return true;
        }

        private (string, char, string, string) ExtractInputComponents(string input)
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
            (string fieldName, char relation, string valueString, string remainingInput) = ExtractInputComponents(input);

            Action(fieldName, relation, ValueType.Parse(valueString, CultureInfo.InvariantCulture));

            return remainingInput;
        }

        protected abstract void Action(string fieldName, char relation, ValueType valueString);
    }

    abstract class FieldFilterAdder<BTMBase, ValueType> : FieldValueProcesser<BTMBase, ValueType>
        where BTMBase : IBTMBase
        where ValueType : IComparable<ValueType>, IParsable<ValueType>
    {
        private ICollection<IPredicate<BTMBase>> predicates;

        public FieldFilterAdder(List<CommandBase> subcommands, ICollection<IPredicate<BTMBase>> predicates, string fieldLowercase) :
            base(subcommands, fieldLowercase, "=<>")
        {
            this.predicates = predicates;
        }

        protected override void Action(string fieldName, char relation, ValueType value)
        {
            predicates.Add(new ComparableValuePredicate(fieldName, relation, value, this));
        }

        public abstract ValueType GetValue(BTMBase item);

        private class ComparableValuePredicate : IPredicate<BTMBase>
        {
            private string fieldName;
            private char relation;
            private ValueType value;
            private FieldFilterAdder<BTMBase, ValueType> getter;

            public ComparableValuePredicate(string fieldName, char relation, ValueType value, FieldFilterAdder<BTMBase, ValueType> getter)
            {
                this.fieldName = fieldName;
                this.relation = relation;
                this.value = value;
                this.getter = getter;
            }

            public bool Eval(BTMBase item)
            {
                int diff = getter.GetValue(item).CompareTo(value);
                switch (relation)
                {
                    case '=': return diff == 0;
                    case '<': return diff < 0;
                    case '>': return diff > 0;
                }
                return false;
            }

            public override string ToString()
            {
                return $"{fieldName}{relation}\"{value}\"";
            }
        }
    }

    abstract class FieldBuilderAdder<BTMBase, ValueType, BTMBaseBuilder> : FieldValueProcesser<BTMBase, ValueType>
        where BTMBase : IBTMBase
        where ValueType : IParsable<ValueType>
        where BTMBaseBuilder : IBTMBuilder<BTMBase>
    {
        private BTMBaseBuilder builder;

        public FieldBuilderAdder(List<CommandBase> subcommands, BTMBaseBuilder builder, string fieldLowercase) :
            base(subcommands, fieldLowercase, "=")
        {
            this.builder = builder;
        }

        protected override void Action(string fieldName, char relation, ValueType value)
        {
            BuildValue(builder, value);
        }

        public abstract void BuildValue(BTMBaseBuilder builder, ValueType value);
    }

    abstract class FieldSetterAdder<BTMBase, ValueType> : FieldValueProcesser<BTMBase, ValueType>
        where BTMBase : IBTMBase
        where ValueType : IParsable<ValueType>
    {
        private ICollection<IAction<BTMBase>> actions;

        public FieldSetterAdder(List<CommandBase> subcommands, ICollection<IAction<BTMBase>> actions, string fieldLowercase) :
            base(subcommands, fieldLowercase, "=")
        {
            this.actions = actions;
        }

        protected override void Action(string fieldName, char relation, ValueType value)
        {
            actions.Add(new SetAction(fieldName, value, this));
        }

        public abstract void SetValue(BTMBase item, ValueType value);

        class SetAction : IAction<BTMBase>
        {
            private string fieldName;
            private ValueType value;
            private FieldSetterAdder<BTMBase, ValueType> setter;

            public SetAction(string fieldName, ValueType value, FieldSetterAdder<BTMBase, ValueType> setter)
            {
                this.fieldName = fieldName;
                this.value = value;
                this.setter = setter;
            }

            public void Eval(BTMBase item)
            {
                setter.SetValue(item, value);
            }

            public override string ToString()
            {
                return $"{fieldName}=\"{value}\"";
            }
        }
    }
}
