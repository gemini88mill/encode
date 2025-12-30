using System;
using System.CommandLine;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Encode;

namespace Encode.Commands;

internal static class EncryptCommand
{
    public static Command Create()
    {
        var encryptCommand = new Command(
            "encrypt",
            "Encrypt or decrypt text or files and write the result to a file.");
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
            Description = "Write output to a file path (defaults to encrypt-output.txt or decrypt-output.txt in the project root)."
        };
        var encryptDecryptOption = new Option<bool>("--decrypt", "-d")
        {
            Description = "Decrypt instead of encrypt."
        };
        var encryptKeyOption = new Option<string>("--key", "-k")
        {
            Description = "Encryption key bytes (format determined by --key-format).",
            Required = false
        };
        var encryptPasswordOption = new Option<string?>("--password", "-p")
        {
            Description = "Password to derive a key from (UTF-8 text)."
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
        encryptCommand.Options.Add(encryptPasswordOption);
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
            var passwordText = parseResult.GetValue(encryptPasswordOption);
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

            var hasPassword = !string.IsNullOrWhiteSpace(passwordText);
            var hasKey = !string.IsNullOrWhiteSpace(keyText);

            if (hasPassword && hasKey)
            {
                Console.Error.WriteLine("Provide either --password or --key, not both.");
                return 2;
            }

            if (!hasPassword && !hasKey)
            {
                Console.Error.WriteLine("Key is required when --password is not provided.");
                return 2;
            }

            if (file && !File.Exists(input))
            {
                Console.Error.WriteLine($"Input file not found: {input}");
                return 2;
            }

            EncrypterBase? encoder = algorithm.ToUpperInvariant() switch
            {
                "AES-256-GCM" => new Aes256GcmEncrypter(),
                "AES256GCM" => new Aes256GcmEncrypter(),
                "AES256-GCM" => new Aes256GcmEncrypter(),
                "AES-GCM" => new Aes256GcmEncrypter(),
                _ => null
            };

            if (encoder is null)
            {
                Console.Error.WriteLine($"Unsupported encryption algorithm: {algorithm}");
                return 2;
            }

            byte[] keyBytes;
            if (hasPassword)
            {
                keyBytes = CliHelpers.DeriveKeyFromPassword(passwordText!);
            }
            else if (!CliHelpers.TryParseBytes(keyText!, keyFormat, out keyBytes))
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

            var resolvedOutFile = outFile ?? new FileInfo(Path.Combine(
                Directory.GetCurrentDirectory(),
                decrypt ? "decrypt-output.txt" : "encrypt-output.txt"));

            try
            {
                if (decrypt)
                {
                    encoder.DecryptToFile(input, file, resolvedOutFile.FullName, keyBytes, format, aadBytes);
                }
                else
                {
                    encoder.EncryptToFile(input, file, resolvedOutFile.FullName, keyBytes, nonceBytes, format, upperCaseHex, aadBytes);
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

        return encryptCommand;
    }
}
