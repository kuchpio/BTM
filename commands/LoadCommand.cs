using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace BTM
{
    class LoadCommand : KeywordConsumer
    {
        public LoadCommand(Terminal terminal) : base(new FileLoader(terminal), "load")
        { }

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
