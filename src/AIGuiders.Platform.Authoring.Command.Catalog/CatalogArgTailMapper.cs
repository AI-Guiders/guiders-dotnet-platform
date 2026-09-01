using AIGuiders.Platform.IntermediateRepresentation.Command;

namespace AIGuiders.Platform.Authoring.Command.Catalog;

public static class CatalogArgTailMapper
{
    public static ArgTailProfile ToArgTailProfile(CatalogProfile profile) =>
        new()
        {
            Name = profile.Name,
            Menu = profile.Entries
                .Select(static e => new ArgTailMenuEntry(e.Arg, MapKind(e.Entry), e.Ref))
                .ToList(),
        };

    static ArgTailEntryKind MapKind(string entry) =>
        entry.ToLowerInvariant() switch
        {
            "preset" => ArgTailEntryKind.Preset,
            "constructor" => ArgTailEntryKind.Constructor,
            "free-text" => ArgTailEntryKind.FreeText,
            "picker-for-slot" => ArgTailEntryKind.PickerForSlot,
            _ => ArgTailEntryKind.Preset,
        };
}
