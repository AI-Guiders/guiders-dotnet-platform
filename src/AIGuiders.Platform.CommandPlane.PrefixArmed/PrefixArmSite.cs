#nullable enable

namespace AIGuiders.Platform.CommandPlane;

/// <summary>Surface-neutral arg site for PAC profiles (GUIDERS-ADR-0038).</summary>
public sealed record PrefixArmSite(
    IReadOnlyList<SlashConstructorBinding> Constructors,
    string? ArgHint,
    string Help,
    string ArgTailKind)
{
    public static PrefixArmSite FromBindings(
        IReadOnlyList<SlashConstructorBinding> constructors,
        string? argHint,
        string help,
        string argTailKind) =>
        new(constructors, argHint, help, argTailKind);
}
