#nullable enable

namespace AIGuiders.Platform.CommandPlane;

public sealed class SlashCompletionOptions
{
    public SlashValueConstructorRegistry? ConstructorRegistry { get; init; }
    public ISlashCultureAmbient? Culture { get; init; }
    public ISlashConstructorSegmentProvider? SegmentProvider { get; init; }
    public DateOnly? AnchorDate { get; init; }

    /// <summary>Product PAC profiles (GUIDERS-ADR-0038). Domain-agnostic prefix lexers.</summary>
    public IReadOnlyList<ISlashPrefixArmProfile> PrefixArmProfiles { get; init; } = [];
}
