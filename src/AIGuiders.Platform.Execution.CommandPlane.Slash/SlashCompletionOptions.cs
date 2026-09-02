#nullable enable

using AIGuiders.Platform.Authoring.Command.Catalog;

namespace AIGuiders.Platform.Execution.CommandPlane;

public sealed class SlashCompletionOptions
{
    public ValueConstructorRegistry? ConstructorRegistry { get; init; }
    public ICultureAmbient? Culture { get; init; }
    public IConstructorSegmentProvider? SegmentProvider { get; init; }
    public DateOnly? AnchorDate { get; init; }

    /// <summary>Product PAC profiles (GUIDERS-ADR-0038). Domain-agnostic prefix lexers.</summary>
    public IReadOnlyList<IPrefixArmProfile> PrefixArmProfiles { get; init; } = [];

    /// <summary>Catalog phrase-slot index for path-phase enrichment (GUIDERS-ADR-0054).</summary>
    public CatalogPhraseSlotIndex? PhraseSlots { get; init; }
}
