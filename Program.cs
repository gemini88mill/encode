using System.CommandLine;
using Encode;

var algorithmArgument = new Argument<string>("algorithm")
{
    Description = "Encoding or hashing algorithm (e.g., SHA-256)."
};
var inputArgument = new Argument<string>("input")
{
    Description = "Text input, or a file path when --file is provided."
};

var fileOption = new Option<bool>("--file", "-f")
{
    Description = "Treat input as a file path and encode the file contents."
};
var outOption = new Option<FileInfo?>("--out", "-o")
{
    Description = "Write output to a file instead of stdout."
};
var lowerOption = new Option<bool>("--lower")
{
    Description = "Output lowercase hex when applicable (default)."
};
var upperOption = new Option<bool>("--upper")
{
    Description = "Output uppercase hex when applicable."
};
var formatOption = new Option<OutputFormat>("--format")
{
    Description = "Output format: hex or base64.",
    DefaultValueFactory = _ => OutputFormat.Hex
};

var rootCommand = new RootCommand("Encode text or files and write the result to stdout or a file.");
rootCommand.Arguments.Add(algorithmArgument);
rootCommand.Arguments.Add(inputArgument);
rootCommand.Options.Add(fileOption);
rootCommand.Options.Add(outOption);
rootCommand.Options.Add(lowerOption);
rootCommand.Options.Add(upperOption);
rootCommand.Options.Add(formatOption);

rootCommand.SetAction(parseResult =>
{
    var algorithm = parseResult.GetValue(algorithmArgument);
    var input = parseResult.GetValue(inputArgument);
    var file = parseResult.GetValue(fileOption);
    var outFile = parseResult.GetValue(outOption);
    var lower = parseResult.GetValue(lowerOption);
    var upper = parseResult.GetValue(upperOption);
    var format = parseResult.GetValue(formatOption);

    if (lower && upper)
    {
        Console.Error.WriteLine("Choose only one of --lower or --upper.");
        return 2;
    }

    if (file && !File.Exists(input))
    {
        Console.Error.WriteLine($"Input file not found: {input}");
        return 2;
    }

    EncoderBase? encoder = algorithm.ToUpperInvariant() switch
    {
        "SHA-1" => new Sha1Encoder(),
        "SHA1" => new Sha1Encoder(),
        "SHA-256" => new Sha256Encoder(),
        "SHA256" => new Sha256Encoder(),
        "SHA-512" => new Sha512Encoder(),
        "SHA512" => new Sha512Encoder(),
        _ => null
    };

    if (encoder is null)
    {
        Console.Error.WriteLine($"Unsupported algorithm: {algorithm}");
        return 2;
    }

    var output = outFile is null
        ? encoder.EncodeToString(input, file)
        : encoder.EncodeToFile(input, file, outFile.FullName);

    if (outFile is null)
    {
        Console.WriteLine(output);
    }

    return 0;
});

return rootCommand.Parse(args).Invoke();

enum OutputFormat
{
    Hex,
    Base64
}
