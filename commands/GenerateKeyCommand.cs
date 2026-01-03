using System;
using System.CommandLine;
using System.Security.Cryptography;
using Encode;

namespace Encode.Commands;

internal static class GenerateKeyCommand
{
    public static Command Create()
    {
        var generateKeyCommand = new Command(
            "generateKey",
            "Generate a random key and write it to stdout.");
        var keyBytesOption = new Option<int>("--bytes", "-b")
        {
            Description = "Number of bytes to generate.",
            DefaultValueFactory = _ => 32
        };
        var keyFormatOption = new Option<OutputFormat>("--format")
        {
            Description = "Output format: base64 or hex.",
            DefaultValueFactory = _ => OutputFormat.Base64
        };
        var keyLowerOption = new Option<bool>("--lower")
        {
            Description = "Output lowercase hex when applicable (default)."
        };
        var keyUpperOption = new Option<bool>("--upper")
        {
            Description = "Output uppercase hex when applicable."
        };

        generateKeyCommand.Options.Add(keyBytesOption);
        generateKeyCommand.Options.Add(keyFormatOption);
        generateKeyCommand.Options.Add(keyLowerOption);
        generateKeyCommand.Options.Add(keyUpperOption);

        generateKeyCommand.SetAction(parseResult =>
        {
            var length = parseResult.GetValue(keyBytesOption);
            var format = parseResult.GetValue(keyFormatOption);
            var lower = parseResult.GetValue(keyLowerOption);
            var upper = parseResult.GetValue(keyUpperOption);

            if (length <= 0)
            {
                Logger.Error("Byte length must be greater than zero.");
                return 2;
            }

            if (lower && upper)
            {
                Logger.Error("Choose only one of --lower or --upper.");
                return 2;
            }

            if ((lower || upper) && format != OutputFormat.Hex)
            {
                Logger.Error("--lower/--upper only apply to hex output.");
                return 2;
            }

            var bytes = RandomNumberGenerator.GetBytes(length);
            var output = format switch
            {
                OutputFormat.Base64 => Convert.ToBase64String(bytes),
                OutputFormat.Hex => upper
                    ? Convert.ToHexString(bytes)
                    : Convert.ToHexString(bytes).ToLowerInvariant(),
                _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unsupported output format.")
            };

            Logger.WriteLine(output);
            return 0;
        });

        return generateKeyCommand;
    }
}

