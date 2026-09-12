#nullable enable

using System.Collections.Generic;
using GdlParseCatalog = AIGuiders.Platform.Modeling.Gdl.Parse.Catalog;

namespace AIGuiders.Platform.Authoring.Command.Catalog;

/// <summary>
/// GUIDERS-FSHARP-ADR-0003 §4.5 cutover: document SSOT via <see cref="ToModel"/> (Modeling.Gdl.Parse.Catalog).
/// </summary>
public sealed class CatalogDocument
{
    public required string Planet { get; init; }
    public IReadOnlyList<string> Imports { get; init; } = [];
    public CatalogDefaults Defaults { get; init; } = new();
    public IReadOnlyList<CatalogChannel> Channels { get; init; } = [];
    public IReadOnlyList<CatalogVariable> Variables { get; init; } = [];
    public IReadOnlyList<CatalogHelp> Helps { get; init; } = [];
    public IReadOnlyList<CatalogPhrase> Phrases { get; init; } = [];
    public IReadOnlyList<CatalogProfile> Profiles { get; init; } = [];
    public IReadOnlyList<CatalogCommandRow> Commands { get; init; } = [];
    public IReadOnlyList<CatalogBindingRow> Bindings { get; init; } = [];
    public IReadOnlyList<CatalogMelodyRow> Melodies { get; init; } = [];
    public IReadOnlyList<CatalogMcpRow> Mcp { get; init; } = [];
    public IReadOnlyDictionary<string, string> Executors { get; init; } =
        new Dictionary<string, string>(StringComparer.Ordinal);

    public GdlParseCatalog.CatalogDocument ToModel() => new(
        Planet,
        FSharpInterop.ToList(Imports),
        Defaults.ToModel(),
        FSharpInterop.ToList(Channels.Select(static c => c.ToModel())),
        FSharpInterop.ToList(Variables.Select(static v => v.ToModel())),
        FSharpInterop.ToList(Helps.Select(static h => h.ToModel())),
        FSharpInterop.ToList(Phrases.Select(static p => p.ToModel())),
        FSharpInterop.ToList(Profiles.Select(static p => p.ToModel())),
        FSharpInterop.ToList(Commands.Select(static c => c.ToModel())),
        FSharpInterop.ToList(Bindings.Select(static b => b.ToModel())),
        FSharpInterop.ToList(Melodies.Select(static m => m.ToModel())),
        FSharpInterop.ToList(Mcp.Select(static m => m.ToModel())),
        FSharpInterop.ToMap(Executors));

    public static CatalogDocument FromModel(GdlParseCatalog.CatalogDocument model) => new()
    {
        Planet = model.Planet,
        Imports = FSharpInterop.FromList(model.Imports),
        Defaults = CatalogDefaults.FromModel(model.Defaults),
        Channels = FSharpInterop.FromList(model.Channels).Select(CatalogChannel.FromModel).ToList(),
        Variables = FSharpInterop.FromList(model.Variables).Select(CatalogVariable.FromModel).ToList(),
        Helps = FSharpInterop.FromList(model.Helps).Select(CatalogHelp.FromModel).ToList(),
        Phrases = FSharpInterop.FromList(model.Phrases).Select(CatalogPhrase.FromModel).ToList(),
        Profiles = FSharpInterop.FromList(model.Profiles).Select(CatalogProfile.FromModel).ToList(),
        Commands = FSharpInterop.FromList(model.Commands).Select(CatalogCommandRow.FromModel).ToList(),
        Bindings = FSharpInterop.FromList(model.Bindings).Select(CatalogBindingRow.FromModel).ToList(),
        Melodies = FSharpInterop.FromList(model.Melodies).Select(CatalogMelodyRow.FromModel).ToList(),
        Mcp = FSharpInterop.FromList(model.Mcp).Select(CatalogMcpRow.FromModel).ToList(),
        Executors = new Dictionary<string, string>(FSharpInterop.FromMap(model.Executors), StringComparer.Ordinal),
    };
}

public sealed class CatalogDefaults
{
    public string? VariableKind { get; init; }
    public string? CommandScope { get; init; }
    public IReadOnlyList<string> CommandSurfaces { get; init; } = [];
    public string? GrammarKeyboardBinding { get; init; }
    public string? GrammarKeyboardMelody { get; init; }
    public string? BindingChordRoot { get; init; }
    /// <summary>Primary invoker wire flavor: <c>console</c> | <c>slash</c> (GUIDERS-ADR-0047).</summary>
    public string? CommandFlavor { get; init; }

    public GdlParseCatalog.CatalogDefaults ToModel() => new(
        FSharpInterop.OptString(VariableKind),
        FSharpInterop.OptString(CommandScope),
        FSharpInterop.ToList(CommandSurfaces),
        FSharpInterop.OptString(GrammarKeyboardBinding),
        FSharpInterop.OptString(GrammarKeyboardMelody),
        FSharpInterop.OptString(BindingChordRoot),
        FSharpInterop.OptString(CommandFlavor));

    public static CatalogDefaults FromModel(GdlParseCatalog.CatalogDefaults model) => new()
    {
        VariableKind = FSharpInterop.OptString(model.VariableKind),
        CommandScope = FSharpInterop.OptString(model.CommandScope),
        CommandSurfaces = FSharpInterop.FromList(model.CommandSurfaces),
        GrammarKeyboardBinding = FSharpInterop.OptString(model.GrammarKeyboardBinding),
        GrammarKeyboardMelody = FSharpInterop.OptString(model.GrammarKeyboardMelody),
        BindingChordRoot = FSharpInterop.OptString(model.BindingChordRoot),
        CommandFlavor = FSharpInterop.OptString(model.CommandFlavor),
    };
}

public sealed class CatalogChannel
{
    public required string Surface { get; init; }
    public string? Sub { get; init; }
    public string? PlanetId { get; init; }
    public string? CommandGrammar { get; init; }
    public string? ArgumentGrammar { get; init; }

    public GdlParseCatalog.CatalogChannel ToModel() => new(
        Surface,
        FSharpInterop.OptString(Sub),
        FSharpInterop.OptString(PlanetId),
        FSharpInterop.OptString(CommandGrammar),
        FSharpInterop.OptString(ArgumentGrammar));

    public static CatalogChannel FromModel(GdlParseCatalog.CatalogChannel model) => new()
    {
        Surface = model.Surface,
        Sub = FSharpInterop.OptString(model.Sub),
        PlanetId = FSharpInterop.OptString(model.PlanetId),
        CommandGrammar = FSharpInterop.OptString(model.CommandGrammar),
        ArgumentGrammar = FSharpInterop.OptString(model.ArgumentGrammar),
    };
}

public sealed record CatalogVariable(string Name, string? Kind)
{
    public GdlParseCatalog.CatalogVariable ToModel() => new(Name, FSharpInterop.OptString(Kind));

    public static CatalogVariable FromModel(GdlParseCatalog.CatalogVariable model) =>
        new(model.Name, FSharpInterop.OptString(model.Kind));
}

public sealed record CatalogHelp(string Target, string Field, string Text)
{
    public GdlParseCatalog.CatalogHelp ToModel() => new(Target, Field, Text);

    public static CatalogHelp FromModel(GdlParseCatalog.CatalogHelp model) =>
        new(model.Target, model.Field, model.Text);
}

public sealed record CatalogPhrase(string Name, string Phrase)
{
    public GdlParseCatalog.CatalogPhrase ToModel() => new(Name, Phrase);

    public static CatalogPhrase FromModel(GdlParseCatalog.CatalogPhrase model) =>
        new(model.Name, model.Phrase);
}

public sealed record CatalogProfileEntry(string Arg, string Entry, string Ref)
{
    public GdlParseCatalog.CatalogProfileEntry ToModel() => new(Arg, Entry, Ref);

    public static CatalogProfileEntry FromModel(GdlParseCatalog.CatalogProfileEntry model) =>
        new(model.Arg, model.Entry, model.Ref);
}

public sealed class CatalogProfile
{
    public required string Name { get; init; }
    public IReadOnlyList<CatalogProfileEntry> Entries { get; init; } = [];
    public string? BundleSource { get; init; }

    public GdlParseCatalog.CatalogProfile ToModel() => new(
        Name,
        FSharpInterop.ToList(Entries.Select(static e => e.ToModel())),
        FSharpInterop.OptString(BundleSource));

    public static CatalogProfile FromModel(GdlParseCatalog.CatalogProfile model) => new()
    {
        Name = model.Name,
        Entries = FSharpInterop.FromList(model.Entries).Select(CatalogProfileEntry.FromModel).ToList(),
        BundleSource = FSharpInterop.OptString(model.BundleSource),
    };
}

public sealed class CatalogCommandRow
{
    public required string Command { get; init; }
    public IReadOnlyDictionary<string, string> Columns { get; init; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public GdlParseCatalog.CatalogCommandRow ToModel() => new(Command, FSharpInterop.ToMap(Columns));

    public static CatalogCommandRow FromModel(GdlParseCatalog.CatalogCommandRow model) => new()
    {
        Command = model.Command,
        Columns = new Dictionary<string, string>(FSharpInterop.FromMap(model.Columns), StringComparer.OrdinalIgnoreCase),
    };
}

public sealed record CatalogBindingRow(string Gesture, string Command, string? Role)
{
    public GdlParseCatalog.CatalogBindingRow ToModel() => new(Gesture, Command, FSharpInterop.OptString(Role));

    public static CatalogBindingRow FromModel(GdlParseCatalog.CatalogBindingRow model) =>
        new(model.Gesture, model.Command, FSharpInterop.OptString(model.Role));
}

public sealed record CatalogMelodyRow(string Slug, string Command)
{
    public GdlParseCatalog.CatalogMelodyRow ToModel() => new(Slug, Command);

    public static CatalogMelodyRow FromModel(GdlParseCatalog.CatalogMelodyRow model) =>
        new(model.Slug, model.Command);
}

public sealed record CatalogMcpRow(string Command, string Expose)
{
    public GdlParseCatalog.CatalogMcpRow ToModel() => new(Command, Expose);

    public static CatalogMcpRow FromModel(GdlParseCatalog.CatalogMcpRow model) =>
        new(model.Command, model.Expose);
}
