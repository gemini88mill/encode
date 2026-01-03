using System.CommandLine;
using System.CommandLine.Help;
using Encode;
using Encode.Commands;

var rootCommand = new RootCommand("Hash or encode text or files and write the result to stdout or a file.");
rootCommand.Subcommands.Add(HashCommand.Create());
rootCommand.Subcommands.Add(EncodeCommand.Create());
rootCommand.Subcommands.Add(GenerateKeyCommand.Create());
rootCommand.Subcommands.Add(EncryptCommand.Create());
rootCommand.Subcommands.Add(InspectCommand.Create());

for (var i = 0; i < rootCommand.Options.Count; i++)
{
    if (rootCommand.Options[i] is HelpOption defaultHelpOption)
    {
        defaultHelpOption.Action = new CustomHelpAction((HelpAction)defaultHelpOption.Action!);
        break;
    }
}

return rootCommand.Parse(args).Invoke();
