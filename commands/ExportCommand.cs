using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BTM
{
    class ExportCommand : KeywordConsumer, IHelpable
    {
        public ExportCommand(List<IExecutor> executors) : base(new List<CommandBase>() {
            new TextExporter(executors),
            new XMLExporter(executors),
        }, "export")
        { }

        public string HelpKeyword => "export";

        public string Help =>
@"USAGE: export <filename> [(xml|plaintext)]

Writes whole command history (or command queue in case of 
*queue export* call) to <filename>. Second (optional) argument
specifies format of the output file. Supported formats are:
    xml         - (default) XML format
    plaintext   - format accepted by BTM Terminal from stdin
";

        private class TextExporter : CommandBase
        {
            private List<IExecutor> executors;

            public TextExporter(List<IExecutor> executors)
            {
                this.executors = executors;
            }

            public override bool Check(string input)
            {
                int spaceIndex = input.IndexOf(' ');
                return spaceIndex > 0 && input.Substring(spaceIndex).Trim().StartsWith("plaintext");
            }

            public override string Process(string input)
            {
                string filename = input.Substring(0, input.IndexOf(' '));
                File.WriteAllText(filename, ExecutorsToString());
                return "";
            }

            private string ExecutorsToString()
            {
                return string.Join("\n", executors);
            }
        }

        private class XMLExporter : CommandBase
        {
            private List<IExecutor> executors;

            public XMLExporter(List<IExecutor> executors)
            {
                this.executors = executors;
            }

            public override bool Check(string input)
            {
                int spaceIndex = input.IndexOf(' ');
                if (input.Length > 0 && spaceIndex < 0) return true;
                return spaceIndex > 0 && input.Substring(spaceIndex).Trim().StartsWith("xml");
            }

            public override string Process(string input)
            {
                int spaceIndex = input.IndexOf(' ');
                string filename = spaceIndex < 0 ? input : input.Substring(0, spaceIndex);
                File.WriteAllText(filename, ExecutorsToString());
                return "";
            }

            private string ExecutorsToString()
            {
                IEnumerable<string> commands = executors
                    .Select((IExecutor executor, int _) =>
                        "\t\t<line content='" +
                            string.Join(
                                "'/>\n\t\t<line content='",
                                executor.ToString().Split('\n', StringSplitOptions.RemoveEmptyEntries)
                            ) +
                        "'/>");

                return
@$"<?xml version='1.0' encoding='utf-8'?>
<commands>
    <command> 
{string.Join(
@"
    </command>
    <command>
",
    commands
)}
    </command>
</commands>";
            }
        }
    }
}
