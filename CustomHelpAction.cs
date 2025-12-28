using System;
using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.Invocation;

namespace Encode;

internal sealed class CustomHelpAction : SynchronousCommandLineAction
{
    private readonly HelpAction defaultHelp;

    public CustomHelpAction(HelpAction action) => defaultHelp = action;

    public override int Invoke(ParseResult parseResult)
    {
        var title = parseResult.RootCommandResult.Command.Description;
        if (!string.IsNullOrWhiteSpace(title))
        {
            Console.WriteLine(title);
            Console.WriteLine();
        }

        var result = defaultHelp.Invoke(parseResult);
        Console.WriteLine("Sample usage: --file input.txt");
        return result;
    }
}
