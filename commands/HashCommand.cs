using System;
using System.CommandLine;
using System.IO;
using Encode;

namespace Encode.Commands;

internal static class HashCommand
{
    public static Command Create()
    {
        var hashCommand = new Command(
            "hash",
            "Hash text or files and write the result to stdout or a file. Caveat: hashing is one-way and not reversible.");
        var hashAlgorithmArgument = new Argument<string>("algorithm")
        {
            Description = "Hash algorithm (e.g., SHA-256)."
        };
        var hashInputArgument = new Argument<string>("input")
        {
            Description = "Text input, or a file path when --file is provided."
        };
        var hashFileOption = new Option<bool>("--file", "-f")
        {
            Description = "Treat input as a file path and use the file contents."
        };
        var hashOutOption = new Option<FileInfo?>("--out", "-o")
        {
            Description = "Write output to a file instead of stdout."
        };
        var hashLowerOption = new Option<bool>("--lower")
        {
            Description = "Output lowercase hex when applicable (default)."
        };
        var hashUpperOption = new Option<bool>("--upper")
        {
            Description = "Output uppercase hex when applicable."
        };
        var hashFormatOption = new Option<OutputFormat>("--format")
        {
            Description = "Output format: base64 or hex.",
            DefaultValueFactory = _ => OutputFormat.Base64
        };

        hashCommand.Arguments.Add(hashAlgorithmArgument);
        hashCommand.Arguments.Add(hashInputArgument);
        hashCommand.Options.Add(hashFileOption);
        hashCommand.Options.Add(hashOutOption);
        hashCommand.Options.Add(hashLowerOption);
        hashCommand.Options.Add(hashUpperOption);
        hashCommand.Options.Add(hashFormatOption);

        hashCommand.SetAction(parseResult =>
        {
            var algorithm = parseResult.GetValue(hashAlgorithmArgument);
            var input = parseResult.GetValue(hashInputArgument);
            var file = parseResult.GetValue(hashFileOption);
            var outFile = parseResult.GetValue(hashOutOption);
            var lower = parseResult.GetValue(hashLowerOption);
            var upper = parseResult.GetValue(hashUpperOption);
            var format = parseResult.GetValue(hashFormatOption);

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

            if (string.IsNullOrWhiteSpace(algorithm) || string.IsNullOrWhiteSpace(input))
            {
                Logger.Error("Algorithm and input are required.");
                return 2;
            }

            if (file && !File.Exists(input))
            {
                Logger.Error($"Input file not found: {input}");
                return 2;
            }

            HasherBase? encoder = algorithm.ToUpperInvariant() switch
            {
                "MD5" => new Md5Hasher(),
                "SHA-1" => new Sha1Hasher(),
                "SHA1" => new Sha1Hasher(),
                "SHA-256" => new Sha256Hasher(),
                "SHA256" => new Sha256Hasher(),
                "SHA-384" => new Sha384Hasher(),
                "SHA384" => new Sha384Hasher(),
                "SHA-512" => new Sha512Hasher(),
                "SHA512" => new Sha512Hasher(),
                _ => null
            };

            if (encoder is null)
            {
                Logger.Error($"Unsupported hash algorithm: {algorithm}");
                return 2;
            }

            var upperCaseHex = upper;

            var output = outFile is null
                ? encoder.HashToString(input, file, format, upperCaseHex)
                : encoder.HashToFile(input, file, outFile.FullName, format, upperCaseHex);

            if (outFile is null)
            {
                Logger.WriteLine(output);
            }

            return 0;
        });

        return hashCommand;
    }
}

