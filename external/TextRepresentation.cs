

namespace BTM.Text
{
    interface ITextRepresentation
    {
        string TextRepr
        {
            get;
            set;
        }
    }

    class LineText : ITextRepresentation
    {
        private string textRepr;

        public string TextRepr
        {
            get { return textRepr; }
            set { textRepr = value; }
        }

        public LineText(string textRepr)
        {
            TextRepr = textRepr;
        }
    }

    class StopText : ITextRepresentation
    {
        private string textRepr;

        public string TextRepr
        {
            get => textRepr;
            set => textRepr = value;
        }

        public StopText(string textRepr)
        {
            TextRepr = textRepr;
        }
    }

    class BytebusText : ITextRepresentation
    {
        private string textRepr;

        public string TextRepr
        {
            get { return textRepr; }
            set { textRepr = value; }
        }

        public BytebusText(string textRepr)
        {
            TextRepr = textRepr;
        }
    }

    class TramText : ITextRepresentation
    {
        private string textRepr;

        public string TextRepr
        {
            get { return textRepr; }
            set { textRepr = value; }
        }

        public TramText(string textRepr)
        {
            TextRepr = textRepr;
        }
    }

    class DriverText : ITextRepresentation
    {
        private string textRepr;

        public string TextRepr
        {
            get { return textRepr; }
            set { textRepr = value; }
        }

        public DriverText(string textRepr)
        {
            TextRepr = textRepr;
        }
    }
}
