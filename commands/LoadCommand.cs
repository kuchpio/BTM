using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace BTM
{
    class LoadCommand : KeywordConsumer, IHelpable
    {
        public LoadCommand(Terminal terminal) : base(new FileLoader(terminal), "load")
        { }

        public string HelpKeyword => "load";

        public string Help =>
@"USAGE: load <filename>

Reads commands from <filename>, which can be formatted in any way
supported by *export* command. Then these commands get called, 
which can result in them being either executed or enqueued, 
depending on BTM terminal mode. 
";

        private class FileLoader : CommandBase
        {
            private Terminal terminal;

            public FileLoader(Terminal terminal)
            {
                this.terminal = terminal;
            }

            public override bool Check(string input)
            {
                return input != "";
            }

            public override string Process(string input)
            {
                string text;
                try
                {
                    text = File.ReadAllText(input);
                }
                catch
                {
                    Console.Error.WriteLine($"Could not read file {input}.");
                    return "";
                }

                if (text.Trim().StartsWith("<?xml"))
                {
                    List<string> lines = new List<string>();
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(text);
                    foreach (XmlNode node in doc.SelectNodes("//line"))
                    {
                        lines.Add(node.Attributes.GetNamedItem("content").InnerText);
                    }
                    text = string.Join("\n", lines);
                }

                TextReader stdin = Console.In;
                TextWriter stdout = Console.Out;

                using (TextReader filestream = new StringReader(text))
                using (TextWriter writer = new StringWriter())
                {
                    Console.SetIn(filestream);
                    Console.SetOut(writer);

                    terminal.Run(false);
                }

                Console.SetIn(stdin);
                Console.SetOut(stdout);

                return "";
            }
        }
    }
}
