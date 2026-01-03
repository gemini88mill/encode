using System;
using System.CommandLine;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Encode.Inspector;

namespace Encode.Commands;

internal static class InspectCommand
{
    private static readonly JsonSerializerOptions OutputOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static Command Create()
    {
        var inspectCommand = new Command(
            "inspect",
            "Inspect an encrypted file and print non-secret envelope metadata as JSON.");

        var fileArgument = new Argument<FileInfo>("file")
        {
            Description = "Encrypted file to inspect."
        };

        inspectCommand.Arguments.Add(fileArgument);
        inspectCommand.SetAction(parseResult =>
        {
            var file = parseResult.GetValue(fileArgument);
            return HandleInspect(file);
        });

        return inspectCommand;
    }

    private static int HandleInspect(FileInfo? file)
    {
        if (file is null)
        {
            Logger.Error("File path is required.");
            return 2;
        }

        if (!file.Exists)
        {
            Logger.Error($"File not found: {file.FullName}");
            return 2;
        }

        string payloadText;
        try
        {
            payloadText = File.ReadAllText(file.FullName);
        }
        catch (Exception ex)
        {
            Logger.Exception(ex);
            return 2;
        }

        if (!InspectorHelpers.TryReadEnvelopeMetadata(payloadText, file, out var inspection, out var errorMessage))
        {
            Logger.Error(errorMessage);
            return 2;
        }

        var json = JsonSerializer.Serialize(inspection, OutputOptions);
        Logger.WriteLine(json);
        return 0;
    }


}


