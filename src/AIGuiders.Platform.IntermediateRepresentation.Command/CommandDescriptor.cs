#nullable enable
using AIGuiders.Platform.Modeling.Notations.Argument;

namespace AIGuiders.Platform.IntermediateRepresentation.Command;

/// <summary>
/// Cross-product slash command descriptor (Forge capabilities + CIDE TOML + platform index).
/// ADR-0154 DOI + ADR-0150 arg_tail.
/// <para><see cref="ArgTail"/> — slash UI mechanics (optional/required/picker). Notation — <see cref="ArgumentNotation"/>.</para>
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
    public string ArgTail { get; init; } = "optional";
    /// <summary>Argument wire profile: alphabet + per-commandId slot schema.</summary>
    public ArgumentNotationProfile? ArgumentNotation { get; init; }
    public string? ArgHint { get; init; }
    public IReadOnlyList<CommandPickerChoice> ArgPickerChoices { get; init; } = [];
    public IReadOnlyList<ArgConstructorBinding> ArgConstructors { get; init; } = [];
    public IReadOnlyList<string> Surfaces { get; init; } = [];
    /// <summary>Catalog scope tags — empty = all scopes (GUIDERS-ADR-0044). Not invoker surfaces.</summary>
    public IReadOnlyList<string> Scope { get; init; } = [];
    public IReadOnlyList<string> RequiredCapabilities { get; init; } = [];
    public string? Tier { get; init; }
    public string? PluginId { get; init; }
    public bool RequiresDestructiveConfirm { get; init; }

    public CommandArgTailKind ArgTailKind => CommandArgTailPolicy.Parse(ArgTail);

    public IEnumerable<string> AllPaths()
    {
        yield return Path;
        foreach (var a in PathAliases)
        {
            if (!string.IsNullOrWhiteSpace(a))
                yield return a;
        }
    }
}

public sealed class CommandPickerChoice
{
    public required string Value { get; init; }
    public string? Label { get; init; }
    public string? Hint { get; init; }
    public CommandPickerChoiceKind Kind { get; init; } = CommandPickerChoiceKind.Value;
}
