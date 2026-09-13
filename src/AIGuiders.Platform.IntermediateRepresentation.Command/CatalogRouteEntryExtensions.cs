#nullable enable

using GdlCommand = AIGuiders.Platform.Modeling.Gdl.Command;

namespace AIGuiders.Platform.IntermediateRepresentation.Command;

/// <summary>Execution-side helpers for F# <see cref="GdlCommand.CatalogRouteEntry"/> (ADR-0003 cutover).</summary>
public static class CatalogRouteEntryExtensions
{
    public static IReadOnlyList<ArgConstructorBinding> ResolvedConstructors(this GdlCommand.CatalogRouteEntry entry) =>
        entry.ArgConstructors;

    public static IReadOnlyList<CommandPickerChoice> ResolvedPickerChoices(this GdlCommand.CatalogRouteEntry entry) =>
        entry.ArgPickerChoices;

    public static CatalogSemanticFields SemanticFields(this GdlCommand.CatalogRouteEntry entry) =>
        GdlCommand.CatalogRouteEntryModule.semanticFields(entry);

    public static string? GroupOrNull(this GdlCommand.CatalogRouteEntry entry) =>
        FSharpInterop.OptString(entry.Group);

    public static string? ArgHintOrNull(this GdlCommand.CatalogRouteEntry entry) =>
        FSharpInterop.OptString(entry.ArgHint);
}
