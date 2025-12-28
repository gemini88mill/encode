using System;
using System.CommandLine;
using System.IO;
using Encode;

namespace Encode.Commands;

internal static class EncodeCommand
{
    public static Command Create()
    {
        var encodeFileOption = new Option<bool>("--file", "-f")
        {
            Description = "Treat input as a file path and use the file contents."
        };
        var encodeOutOption = new Option<FileInfo?>("--out", "-o")
        {
            Description = "Write output to a file instead of stdout."
        };
        var decodeOption = new Option<bool>("--decode", "-d")
        {
            Description = "Decode instead of encode."
        };

        var encodeCommand = new Command(
            "encode",
            "Encode or decode text or files and write the result to stdout or a file. Caveat: encoding is reversible and not secure.");
        var encodeAlgorithmArgument = new Argument<string>("algorithm")
        {
            Description = "Encoding algorithm (e.g., base64 or url)."
        };
        var encodeInputArgument = new Argument<string>("input")
        {
            Description = "Text input, or a file path when --file is provided."
        };

        encodeCommand.Arguments.Add(encodeAlgorithmArgument);
        encodeCommand.Arguments.Add(encodeInputArgument);
        encodeCommand.Options.Add(encodeFileOption);
        encodeCommand.Options.Add(encodeOutOption);
        encodeCommand.Options.Add(decodeOption);

        encodeCommand.SetAction(parseResult =>
        {
            var algorithm = parseResult.GetValue(encodeAlgorithmArgument);
            var input = parseResult.GetValue(encodeInputArgument);
            var file = parseResult.GetValue(encodeFileOption);
            var outFile = parseResult.GetValue(encodeOutOption);
            var decode = parseResult.GetValue(decodeOption);

            if (string.IsNullOrWhiteSpace(algorithm) || string.IsNullOrWhiteSpace(input))
            {
                Console.Error.WriteLine("Algorithm and input are required.");
                return 2;
            }

            if (file && !File.Exists(input))
            {
                Console.Error.WriteLine($"Input file not found: {input}");
                return 2;
            }

            EncoderBase? encoder = algorithm.ToUpperInvariant() switch
            {
                "BASE64" => new Base64Encoder(),
                "BASE-64" => new Base64Encoder(),
                "URL" => new UrlEncoder(),
                _ => null
            };

            if (encoder is null)
            {
                Console.Error.WriteLine($"Unsupported encoding algorithm: {algorithm}");
                return 2;
            }

            string output;
            try
            {
                output = outFile is null
                    ? encoder.TransformToString(input, file, decode)
                    : encoder.TransformToFile(input, file, outFile.FullName, decode);
            }
            catch (FormatException ex)
            {
                Console.Error.WriteLine(ex.Message);
                return 2;
            }

            if (outFile is null)
            {
                Console.WriteLine(output);
            }

            return 0;
        });

        return encodeCommand;
    }
}
