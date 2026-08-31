#nullable enable

namespace AIGuiders.Platform.IntermediateRepresentation.Command;

/// <summary>ADR-0154 domain · object · intent triple.</summary>
public readonly record struct CatalogSemanticFields(
    string Domain,
    string Object,
    string Intent,
    CatalogPathRole PathRole = CatalogPathRole.Canonical)
{
    public bool DomainOmittedInPath =>
        PathRole == CatalogPathRole.Alias && !string.IsNullOrEmpty(Domain);
}
