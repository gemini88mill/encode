using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.Security.Cryptography;
using System.Text;
using Encode;

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
        Console.Error.WriteLine("Choose only one of --lower or --upper.");
        return 2;
    }

    if ((lower || upper) && format != OutputFormat.Hex)
    {
        Console.Error.WriteLine("--lower/--upper only apply to hex output.");
        return 2;
    }

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

    HashEncoderBase? encoder = algorithm.ToUpperInvariant() switch
    {
        "MD5" => new Md5Encoder(),
        "SHA-1" => new Sha1Encoder(),
        "SHA1" => new Sha1Encoder(),
        "SHA-256" => new Sha256Encoder(),
        "SHA256" => new Sha256Encoder(),
        "SHA-384" => new Sha384Encoder(),
        "SHA384" => new Sha384Encoder(),
        "SHA-512" => new Sha512Encoder(),
        "SHA512" => new Sha512Encoder(),
        _ => null
    };

    if (encoder is null)
    {
        Console.Error.WriteLine($"Unsupported hash algorithm: {algorithm}");
        return 2;
    }

    var upperCaseHex = upper;

    var output = outFile is null
        ? encoder.HashToString(input, file, format, upperCaseHex)
        : encoder.HashToFile(input, file, outFile.FullName, format, upperCaseHex);

    if (outFile is null)
    {
        Console.WriteLine(output);
    }

    return 0;
});

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

    EncodingBase? encoder = algorithm.ToUpperInvariant() switch
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

var encryptCommand = new Command(
    "encrypt",
    "Encrypt or decrypt text or files and write the result to stdout or a file.");
var encryptAlgorithmArgument = new Argument<string>("algorithm")
{
    Description = "Encryption algorithm (e.g., AES-256-GCM)."
};
var encryptInputArgument = new Argument<string>("input")
{
    Description = "Text input, or a file path when --file is provided."
};
var encryptFileOption = new Option<bool>("--file", "-f")
{
    Description = "Treat input as a file path and use the file contents."
};
var encryptOutOption = new Option<FileInfo?>("--out", "-o")
{
    Description = "Write output to a file instead of stdout."
};
var encryptDecryptOption = new Option<bool>("--decrypt", "-d")
{
    Description = "Decrypt instead of encrypt."
};
var encryptKeyOption = new Option<string>("--key", "-k")
{
    Description = "Encryption key bytes (format determined by --key-format).",
    Required = true
};
var encryptKeyFormatOption = new Option<OutputFormat>("--key-format")
{
    Description = "Key format: base64 or hex.",
    DefaultValueFactory = _ => OutputFormat.Base64
};
var encryptNonceOption = new Option<string?>("--nonce", "-n")
{
    Description = "Nonce/IV for AES-GCM (optional, uses --key-format)."
};
var encryptAadOption = new Option<string?>("--aad")
{
    Description = "Associated data (AAD) as UTF-8 text (optional)."
};
var encryptFormatOption = new Option<OutputFormat>("--format")
{
    Description = "Output payload format: base64 or hex.",
    DefaultValueFactory = _ => OutputFormat.Base64
};
var encryptLowerOption = new Option<bool>("--lower")
{
    Description = "Output lowercase hex when applicable (default)."
};
var encryptUpperOption = new Option<bool>("--upper")
{
    Description = "Output uppercase hex when applicable."
};
encryptCommand.Arguments.Add(encryptAlgorithmArgument);
encryptCommand.Arguments.Add(encryptInputArgument);
encryptCommand.Options.Add(encryptFileOption);
encryptCommand.Options.Add(encryptOutOption);
encryptCommand.Options.Add(encryptDecryptOption);
encryptCommand.Options.Add(encryptKeyOption);
encryptCommand.Options.Add(encryptKeyFormatOption);
encryptCommand.Options.Add(encryptNonceOption);
encryptCommand.Options.Add(encryptAadOption);
encryptCommand.Options.Add(encryptFormatOption);
encryptCommand.Options.Add(encryptLowerOption);
encryptCommand.Options.Add(encryptUpperOption);

encryptCommand.SetAction(parseResult =>
{
    var algorithm = parseResult.GetValue(encryptAlgorithmArgument);
    var input = parseResult.GetValue(encryptInputArgument);
    var file = parseResult.GetValue(encryptFileOption);
    var outFile = parseResult.GetValue(encryptOutOption);
    var decrypt = parseResult.GetValue(encryptDecryptOption);
    var keyText = parseResult.GetValue(encryptKeyOption);
    var keyFormat = parseResult.GetValue(encryptKeyFormatOption);
    var nonceText = parseResult.GetValue(encryptNonceOption);
    var aadText = parseResult.GetValue(encryptAadOption);
    var format = parseResult.GetValue(encryptFormatOption);
    var lower = parseResult.GetValue(encryptLowerOption);
    var upper = parseResult.GetValue(encryptUpperOption);

    if (lower && upper)
    {
        Console.Error.WriteLine("Choose only one of --lower or --upper.");
        return 2;
    }

    if ((lower || upper) && format != OutputFormat.Hex)
    {
        Console.Error.WriteLine("--lower/--upper only apply to hex output.");
        return 2;
    }

    if (string.IsNullOrWhiteSpace(algorithm) || string.IsNullOrWhiteSpace(input))
    {
        Console.Error.WriteLine("Algorithm and input are required.");
        return 2;
    }

    if (string.IsNullOrWhiteSpace(keyText))
    {
        Console.Error.WriteLine("Key is required.");
        return 2;
    }

    if (file && !File.Exists(input))
    {
        Console.Error.WriteLine($"Input file not found: {input}");
        return 2;
    }

    EncryptionBase? encoder = algorithm.ToUpperInvariant() switch
    {
        "AES-256-GCM" => new Aes256GcmEncoder(),
        "AES256GCM" => new Aes256GcmEncoder(),
        "AES256-GCM" => new Aes256GcmEncoder(),
        "AES-GCM" => new Aes256GcmEncoder(),
        _ => null
    };

    if (encoder is null)
    {
        Console.Error.WriteLine($"Unsupported encryption algorithm: {algorithm}");
        return 2;
    }

    if (!CliHelpers.TryParseBytes(keyText, keyFormat, out var keyBytes))
    {
        Console.Error.WriteLine($"Invalid key for {keyFormat.ToString().ToLowerInvariant()} format.");
        return 2;
    }

    byte[]? nonceBytes = null;
    if (!string.IsNullOrWhiteSpace(nonceText))
    {
        if (!CliHelpers.TryParseBytes(nonceText, keyFormat, out var parsedNonce))
        {
            Console.Error.WriteLine($"Invalid nonce for {keyFormat.ToString().ToLowerInvariant()} format.");
            return 2;
        }

        nonceBytes = parsedNonce;
    }

    byte[]? aadBytes = string.IsNullOrWhiteSpace(aadText) ? null : Encoding.UTF8.GetBytes(aadText);
    var upperCaseHex = upper;

    try
    {
        string output = decrypt
            ? outFile is null
                ? encoder.DecryptToString(input, file, keyBytes, format, aadBytes)
                : encoder.DecryptToFile(input, file, outFile.FullName, keyBytes, format, aadBytes)
            : outFile is null
                ? encoder.EncryptToString(input, file, keyBytes, nonceBytes, format, upperCaseHex, aadBytes)
                : encoder.EncryptToFile(input, file, outFile.FullName, keyBytes, nonceBytes, format, upperCaseHex, aadBytes);

        if (outFile is null)
        {
            Console.WriteLine(output);
        }
    }
    catch (FormatException ex)
    {
        Console.Error.WriteLine(ex.Message);
        return 2;
    }
    catch (CryptographicException ex)
    {
        Console.Error.WriteLine(ex.Message);
        return 2;
    }
    catch (ArgumentException ex)
    {
        Console.Error.WriteLine(ex.Message);
        return 2;
    }

    return 0;
});

var rootCommand = new RootCommand("Hash or encode text or files and write the result to stdout or a file.");
rootCommand.Subcommands.Add(hashCommand);
rootCommand.Subcommands.Add(encodeCommand);
rootCommand.Subcommands.Add(encryptCommand);

for (var i = 0; i < rootCommand.Options.Count; i++)
{
    if (rootCommand.Options[i] is HelpOption defaultHelpOption)
    {
        defaultHelpOption.Action = new CustomHelpAction((HelpAction)defaultHelpOption.Action!);
        break;
    }
}

return rootCommand.Parse(args).Invoke();

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

internal static class CliHelpers
{
    public static bool TryParseBytes(string value, OutputFormat format, out byte[] bytes)
    {
        try
        {
            bytes = format switch
            {
                OutputFormat.Base64 => Convert.FromBase64String(value),
                OutputFormat.Hex => Convert.FromHexString(value),
                _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unsupported format.")
            };
            return true;
        }
        catch (FormatException)
        {
            bytes = Array.Empty<byte>();
            return false;
        }
        catch (ArgumentException)
        {
            bytes = Array.Empty<byte>();
            return false;
        }
    }
}
