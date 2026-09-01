using AIGuiders.Platform.Authoring.Core;

namespace AIGuiders.Platform.Authoring.Command.Catalog;

public enum GrammarAxis
{
    Command,
    Argument,
    Keyboard,
}

public sealed record GrammarDefinition(
    string Id,
    GrammarAxis Axis,
    string ConformanceSpec,
    string PackageHint);

/// <summary>Federation string-grammar ids (ADR-0021 §9) referenced from <c>.catalog</c> <c>grammar.*</c> keys.</summary>
public static class NotationGrammarRegistry
{
    static readonly Dictionary<string, GrammarDefinition> Definitions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["command-slash"] = new("command-slash", GrammarAxis.Command, "notation/command-slash", "Notations.Command.Slash"),
            ["command-console"] = new("command-console", GrammarAxis.Command, "notation/command-console", "Notations.Command.Console"),
            ["argument-slash"] = new("argument-slash", GrammarAxis.Argument, "notation/invocation-parity", "Notations.Argument.Slash"),
            ["argument-kv"] = new("argument-kv", GrammarAxis.Argument, "notation/argument-kv", "Notations.Argument.Kv"),
            ["argument-positional"] = new("argument-positional", GrammarAxis.Argument, "notation/argument-positional", "Notations.Argument.Positional"),
            ["argument-delimited"] = new("argument-delimited", GrammarAxis.Argument, "notation/argument-delimited", "Notations.Argument.Delimited"),
            ["argument-cli"] = new("argument-cli", GrammarAxis.Argument, "notation/argument-cli", "Notations.Argument.Cli"),
            ["keyboard-key-gesture"] = new("keyboard-key-gesture", GrammarAxis.Keyboard, "notation/key-gesture", "Notations.Keyboard.KeyGesture"),
            ["keyboard-vim"] = new("keyboard-vim", GrammarAxis.Keyboard, "notation/neovim-kbd", "Notations.Keyboard.Vim"),
            ["keyboard-neovim"] = new("keyboard-neovim", GrammarAxis.Keyboard, "notation/neovim-kbd", "Notations.Keyboard.Vim"),
        };

    public static bool IsKnown(string? grammarId) =>
        !string.IsNullOrWhiteSpace(grammarId) && Definitions.ContainsKey(grammarId);

    public static bool TryGet(string grammarId, out GrammarDefinition definition) =>
        Definitions.TryGetValue(grammarId, out definition!);

    public static void ValidateDocument(CatalogDocument document, List<AuthoringDiagnostic> diagnostics)
    {
        foreach (var channel in document.Channels)
        {
            if (string.IsNullOrWhiteSpace(channel.Surface)
                || channel.Surface.Equals("palette", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            ValidateId(channel.CommandGrammar, GrammarAxis.Command, diagnostics, $"channels.{channel.Surface}.{channel.Sub}.grammar.command");
            ValidateId(channel.ArgumentGrammar, GrammarAxis.Argument, diagnostics, $"channels.{channel.Surface}.{channel.Sub}.grammar.argument");
        }

        ValidateId(document.Defaults.GrammarKeyboardBinding, GrammarAxis.Keyboard, diagnostics, "defaults.grammar.keyboard.binding");
        ValidateId(document.Defaults.GrammarKeyboardMelody, GrammarAxis.Keyboard, diagnostics, "defaults.grammar.keyboard.melody");
    }

    static void ValidateId(string? grammarId, GrammarAxis axis, List<AuthoringDiagnostic> diagnostics, string where)
    {
        if (string.IsNullOrWhiteSpace(grammarId))
        {
            return;
        }

        if (!Definitions.TryGetValue(grammarId, out var definition))
        {
            diagnostics.Add(new(
                AuthoringDiagnosticCode.UnknownGrammarId,
                $"unknown-grammar-id: {where} — '{grammarId}' is not a federation grammar id.",
                1,
                Section: where));
            return;
        }

        if (definition.Axis != axis)
        {
            diagnostics.Add(new(
                AuthoringDiagnosticCode.UnknownGrammarId,
                $"unknown-grammar-id: {where} — '{grammarId}' is a {definition.Axis} grammar, expected {axis}.",
                1,
                Section: where));
        }
    }
}
