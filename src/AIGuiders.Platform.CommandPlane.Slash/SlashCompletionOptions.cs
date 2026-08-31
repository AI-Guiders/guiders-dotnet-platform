#nullable enable

namespace AIGuiders.Platform.CommandPlane;

public sealed class SlashCompletionOptions
{
    public ValueConstructorRegistry? ConstructorRegistry { get; init; }
    public ICultureAmbient? Culture { get; init; }
    public IConstructorSegmentProvider? SegmentProvider { get; init; }
    public DateOnly? AnchorDate { get; init; }

    /// <summary>Product PAC profiles (GUIDERS-ADR-0038). Domain-agnostic prefix lexers.</summary>
    public IReadOnlyList<IPrefixArmProfile> PrefixArmProfiles { get; init; } = [];
}
