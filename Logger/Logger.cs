using System;
using Spectre.Console;

namespace Encode;

public static class Logger
{
    public static void Write(string message)
    {
        AnsiConsole.Markup(Markup.Escape(message));
    }

    public static void WriteLine(string message)
    {
        AnsiConsole.MarkupLine(Markup.Escape(message));
    }

    public static void Info(string message)
    {
        AnsiConsole.MarkupLine(Markup.Escape(message));
    }

    public static void Success(string message)
    {
        AnsiConsole.MarkupLine($"[green]{Markup.Escape(message)}[/]");
    }

    public static void Warning(string message)
    {
        AnsiConsole.MarkupLine($"[yellow]{Markup.Escape(message)}[/]");
    }

    public static void Error(string message)
    {
        AnsiConsole.MarkupLine($"[red]{Markup.Escape(message)}[/]");
    }

    public static void Exception(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        AnsiConsole.WriteException(exception);
    }

    public static string PromptPassword(string prompt, int minLength = 1, int maxAttempts = 3)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            throw new ArgumentException("Prompt is required.", nameof(prompt));
        }

        if (minLength < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(minLength), minLength, "Minimum length must be at least 1.");
        }

        if (maxAttempts < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxAttempts), maxAttempts, "Max attempts must be at least 1.");
        }

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            var password = AnsiConsole.Prompt(
                new TextPrompt<string>(Markup.Escape(prompt))
                    .Secret());

            if (password.Length >= minLength)
            {
                return password;
            }

            var attemptsLeft = maxAttempts - attempt;
            if (attemptsLeft > 0)
            {
                AnsiConsole.MarkupLine($"[red]Password must be at least {minLength} characters. Attempts left: {attemptsLeft}.[/]");
            }
        }

        throw new InvalidOperationException("Password prompt attempts exceeded.");
    }
}
