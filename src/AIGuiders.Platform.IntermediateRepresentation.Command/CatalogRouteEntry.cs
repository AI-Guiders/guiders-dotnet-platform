#nullable enable

using GdlCommand = AIGuiders.Platform.Modeling.Gdl.Command;

namespace AIGuiders.Platform.IntermediateRepresentation.Command;

/// <summary>
/// GUIDERS-FSHARP-ADR-0003 §4.3 cutover factories — row SSOT: Modeling.Gdl.Command.CatalogRouteEntry.
/// </summary>
public static class CatalogRouteEntry
{
    public static GdlCommand.CatalogRouteEntry FromDescriptor(CommandDescriptor d, string path) =>
        GdlCommand.CatalogRouteEntryModule.fromDescriptor(d.ToModel(), path);

    public static GdlCommand.CatalogRouteEntry FromDescriptor(CommandDescriptor d, string path, CatalogPathRole pathRole) =>
        GdlCommand.CatalogRouteEntryModule.fromDescriptorRole(d.ToModel(), path, pathRole);

    public static string NormalizePath(string path) => GdlCommand.CatalogRouteEntryModule.normalizePath(path);

    public static GdlCommand.CatalogRouteEntry WithPath(GdlCommand.CatalogRouteEntry entry, string path) =>
        new(
            path,
            entry.CommandId,
            entry.Help,
            entry.ArgTailKind,
            entry.Domain,
            entry.Object,
            entry.Intent,
            entry.PathRole,
            entry.Group,
            entry.ArgTail,
            entry.ArgPickerChoices,
            entry.ArgHint,
            entry.ArgConstructors);
}
