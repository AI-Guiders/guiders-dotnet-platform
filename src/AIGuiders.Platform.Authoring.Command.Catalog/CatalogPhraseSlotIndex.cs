#nullable enable

namespace AIGuiders.Platform.Authoring.Command.Catalog;

/// <summary>
/// Catalog projection for phrase-slot completion (GUIDERS-ADR-0054).
/// Built from <c>phrase</c> + <c>fills</c> + <c>helps</c> — platform SSOT for active-slot index.
/// </summary>
public sealed class CatalogPhraseSlotIndex
{
    readonly Dictionary<string, CatalogPhraseSlotCommand> _byKey;
    readonly IReadOnlyList<CatalogPhraseSlotCommand> _commands;

    CatalogPhraseSlotIndex(
        Dictionary<string, CatalogPhraseSlotCommand> byKey,
        IReadOnlyList<CatalogPhraseSlotCommand> commands)
    {
        _byKey = byKey;
        _commands = commands;
    }

    public IReadOnlyList<CatalogPhraseSlotCommand> Commands => _commands;

    public static CatalogPhraseSlotIndex FromDocument(CatalogDocument document)
    {
        var phraseByName = document.Phrases.ToDictionary(
            phrase => phrase.Name,
            phrase => phrase.Phrase,
            StringComparer.OrdinalIgnoreCase);

        var slotLabels = BuildSlotLabels(document);
        var byKey = new Dictionary<string, CatalogPhraseSlotCommand>(StringComparer.OrdinalIgnoreCase);
        var commands = new List<CatalogPhraseSlotCommand>();

        foreach (var row in document.Commands)
        {
            if (!row.Columns.TryGetValue("fills", out var fillsRaw)
                || string.IsNullOrWhiteSpace(fillsRaw))
            {
                continue;
            }

            if (!row.Columns.TryGetValue("phrase", out var phraseName)
                || string.IsNullOrWhiteSpace(phraseName)
                || !phraseByName.TryGetValue(phraseName, out var template))
            {
                continue;
            }

            var fills = ParseCsv(fillsRaw);
            if (fills.Count == 0)
            {
                continue;
            }

            var entry = new CatalogPhraseSlotCommand(
                row.Command,
                document.WireCommandId(row.Command),
                ReadLiteralPrefix(template),
                fills,
                slotLabels);

            commands.Add(entry);
            Register(byKey, entry.CatalogCommand, entry);
            Register(byKey, entry.WireCommandId, entry);
        }

        return new CatalogPhraseSlotIndex(byKey, commands);
    }

    public static CatalogPhraseSlotIndex FromEmitted(
        IReadOnlyList<CatalogPhraseSlotEmit> commands,
        IReadOnlyDictionary<string, string> slotLabels)
    {
        var byKey = new Dictionary<string, CatalogPhraseSlotCommand>(StringComparer.OrdinalIgnoreCase);
        var built = new List<CatalogPhraseSlotCommand>(commands.Count);
        foreach (var emit in commands)
        {
            var entry = new CatalogPhraseSlotCommand(
                emit.CatalogCommand,
                emit.WireCommandId,
                emit.LiteralPrefix,
                emit.Fills,
                slotLabels);
            built.Add(entry);
            Register(byKey, entry.CatalogCommand, entry);
            Register(byKey, entry.WireCommandId, entry);
        }

        return new CatalogPhraseSlotIndex(byKey, built);
    }

    public bool TryResolveCommand(string typedBody, string? commandKey, out CatalogPhraseSlotCommand command)
    {
        if (!string.IsNullOrWhiteSpace(commandKey) && _byKey.TryGetValue(commandKey, out command!))
        {
            return true;
        }

        var matches = _commands
            .Where(entry => typedBody.StartsWith(entry.LiteralPrefix, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (matches.Count == 1)
        {
            command = matches[0];
            return true;
        }

        command = null!;
        return false;
    }

    public string? ResolveActiveSlot(string typedBody, string? commandKey = null) =>
        TryResolveCommand(typedBody, commandKey, out var command)
            ? command.ResolveActiveSlot(typedBody)
            : null;

    public string? ReadBoundSlotValue(string typedBody, string? commandKey, string slotName) =>
        TryResolveCommand(typedBody, commandKey, out var command)
            ? command.ReadBoundSlotValue(typedBody, slotName)
            : null;

    public string? GetSlotLabel(string slotName) =>
        _commands.Select(command => command.GetSlotLabel(slotName)).FirstOrDefault(label => label is not null);

    static void Register(
        IDictionary<string, CatalogPhraseSlotCommand> byKey,
        string key,
        CatalogPhraseSlotCommand command)
    {
        if (!string.IsNullOrWhiteSpace(key))
        {
            byKey[key] = command;
        }
    }

    static IReadOnlyDictionary<string, string> BuildSlotLabels(CatalogDocument document)
    {
        var labels = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var help in document.Helps)
        {
            if (!help.Target.StartsWith("variable ", StringComparison.OrdinalIgnoreCase)
                || !help.Field.Equals("label", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var slotName = help.Target["variable ".Length..].Trim();
            if (slotName.Length > 0)
            {
                labels[slotName] = help.Text;
            }
        }

        return labels;
    }

    static IReadOnlyList<string> ParseCsv(string raw) =>
        raw.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

    static string ReadLiteralPrefix(string template)
    {
        var slotStart = template.IndexOf('{');
        return slotStart < 0 ? template.Trim() : template[..slotStart].TrimEnd();
    }
}

/// <summary>One catalog command with phrase-slot metadata.</summary>
public sealed class CatalogPhraseSlotCommand
{
    public CatalogPhraseSlotCommand(
        string catalogCommand,
        string wireCommandId,
        string literalPrefix,
        IReadOnlyList<string> fills,
        IReadOnlyDictionary<string, string> slotLabels)
    {
        CatalogCommand = catalogCommand;
        WireCommandId = wireCommandId;
        LiteralPrefix = literalPrefix;
        Fills = fills;
        SlotLabels = slotLabels;
    }

    public string CatalogCommand { get; }
    public string WireCommandId { get; }
    public string LiteralPrefix { get; }
    public IReadOnlyList<string> Fills { get; }
    public IReadOnlyDictionary<string, string> SlotLabels { get; }

    public string? ResolveActiveSlot(string typedBody)
    {
        if (!typedBody.StartsWith(LiteralPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var boundSlotCount = CountBoundSlots(typedBody);
        return boundSlotCount >= Fills.Count ? null : Fills[boundSlotCount];
    }

    public string? ReadBoundSlotValue(string typedBody, string slotName)
    {
        var slotIndex = IndexOfFill(slotName);
        if (slotIndex < 0)
        {
            return null;
        }

        var tokens = ReadBoundTokens(typedBody);
        return slotIndex < tokens.Count ? tokens[slotIndex] : null;
    }

    public string? GetSlotLabel(string slotName) =>
        SlotLabels.TryGetValue(slotName, out var label) ? label : null;

    int CountBoundSlots(string typedBody)
    {
        var tail = typedBody[LiteralPrefix.Length..].TrimStart();
        return tail.Length == 0
            ? 0
            : tail.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Length;
    }

    IReadOnlyList<string> ReadBoundTokens(string typedBody)
    {
        if (!typedBody.StartsWith(LiteralPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return [];
        }

        var tail = typedBody[LiteralPrefix.Length..].TrimStart();
        return tail.Length == 0
            ? []
            : tail.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    int IndexOfFill(string slotName)
    {
        for (var i = 0; i < Fills.Count; i++)
        {
            if (Fills[i].Equals(slotName, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
    }
}

/// <summary>Codegen row for phrase-slot catalog commands (GUIDERS-ADR-0054).</summary>
public sealed record CatalogPhraseSlotEmit(
    string CatalogCommand,
    string WireCommandId,
    string LiteralPrefix,
    string[] Fills);
