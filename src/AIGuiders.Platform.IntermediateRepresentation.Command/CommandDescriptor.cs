#nullable enable

using System.Collections.Generic;
using AIGuiders.Platform.Modeling.Notations.Argument;
using GdlCommand = AIGuiders.Platform.Modeling.Gdl.Command;

namespace AIGuiders.Platform.IntermediateRepresentation.Command;

/// <summary>
/// GUIDERS-FSHARP-ADR-0003 §4.3 cutover: core fields SSOT via <see cref="ToModel"/>;
/// <see cref="ArgumentNotation"/> stays execution-side (Notations.Argument wire).
/// </summary>
public sealed class CommandDescriptor
{
    public required string Domain { get; init; }
    public required string Object { get; init; }
    public required string Intent { get; init; }
    public required string CommandId { get; init; }
    public required string Path { get; init; }
    public IReadOnlyList<string> PathAliases { get; init; } = [];
    public string? Help { get; init; }
    public string? Group { get; init; }
    public string ArgTail { get; init; } = GdlCommand.CommandDescriptorModule.DefaultArgTail;
    public ArgumentNotationProfile? ArgumentNotation { get; init; }
    public string? ArgHint { get; init; }
    public IReadOnlyList<CommandPickerChoice> ArgPickerChoices { get; init; } = [];
    public IReadOnlyList<ArgConstructorBinding> ArgConstructors { get; init; } = [];
    public IReadOnlyList<string> Surfaces { get; init; } = [];
    public IReadOnlyList<string> Scope { get; init; } = [];
    public IReadOnlyList<string> RequiredCapabilities { get; init; } = [];
    public string? Tier { get; init; }
    public string? PluginId { get; init; }
    public bool RequiresDestructiveConfirm { get; init; }

    public CommandArgTailKind ArgTailKind => CommandArgTailPolicy.Parse(ArgTail);

    public IEnumerable<string> AllPaths()
    {
        foreach (var path in GdlCommand.CommandDescriptorModule.allPaths(ToModel()))
            yield return path;
    }

    public GdlCommand.CommandDescriptor ToModel() => new(
        Domain,
        Object,
        Intent,
        CommandId,
        Path,
        PathAliases,
        FSharpInterop.OptString(Help),
        FSharpInterop.OptString(Group),
        ArgTail,
        FSharpInterop.OptString(ArgHint),
        ArgPickerChoices,
        ArgConstructors,
        Surfaces,
        Scope,
        RequiredCapabilities,
        FSharpInterop.OptString(Tier),
        FSharpInterop.OptString(PluginId),
        RequiresDestructiveConfirm);

    public static CommandDescriptor FromModel(GdlCommand.CommandDescriptor model, ArgumentNotationProfile? notation = null) => new()
    {
        Domain = model.Domain,
        Object = model.Object,
        Intent = model.Intent,
        CommandId = model.CommandId,
        Path = model.Path,
        PathAliases = model.PathAliases,
        Help = FSharpInterop.OptString(model.Help),
        Group = FSharpInterop.OptString(model.Group),
        ArgTail = model.ArgTail,
        ArgHint = FSharpInterop.OptString(model.ArgHint),
        ArgPickerChoices = model.ArgPickerChoices,
        ArgConstructors = model.ArgConstructors,
        Surfaces = model.Surfaces,
        Scope = model.Scope,
        RequiredCapabilities = model.RequiredCapabilities,
        Tier = FSharpInterop.OptString(model.Tier),
        PluginId = FSharpInterop.OptString(model.PluginId),
        RequiresDestructiveConfirm = model.RequiresDestructiveConfirm,
        ArgumentNotation = notation,
    };
}
