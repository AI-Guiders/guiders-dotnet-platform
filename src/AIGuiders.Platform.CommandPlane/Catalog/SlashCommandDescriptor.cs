#nullable enable
using AIGuiders.Platform.Notations.Argument;

namespace AIGuiders.Platform.CommandPlane;

/// <summary>
/// Cross-product slash command descriptor (Forge capabilities + CIDE TOML + platform index).
/// ADR-0154 DOI + ADR-0150 arg_tail.
/// <para><see cref="ArgTail"/> — slash UI mechanics (optional/required/picker). Wire + slots — <see cref="TailWireClass"/> + <see cref="ArgParameters"/>.</para>
/// </summary>
public sealed class SlashCommandDescriptor
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
    /// <summary>Invocation tail wire alphabet: kv | cli | positional | delimited | raw.</summary>
    public string? TailWireClass { get; init; }
    /// <summary>Per-commandId arg slot schema; interpretation is application-owned.</summary>
    public IReadOnlyList<InvocationArgParameter> ArgParameters { get; init; } = [];
    public string? ArgHint { get; init; }
    public IReadOnlyList<SlashPickerChoice> ArgPickerChoices { get; init; } = [];
    public IReadOnlyList<string> Surfaces { get; init; } = [];
    public IReadOnlyList<string> RequiredCapabilities { get; init; } = [];
    public string? Tier { get; init; }
    public string? PluginId { get; init; }
    public bool RequiresDestructiveConfirm { get; init; }

    public SlashArgTailKind ArgTailKind => SlashArgTailPolicy.Parse(ArgTail);

    public InvocationArgDescriptor ToInvocationArgDescriptor() => new(TailWireClass, ArgParameters);

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

public sealed class SlashPickerChoice
{
    public required string Value { get; init; }
    public string? Label { get; init; }
    public string? Hint { get; init; }
}
