#nullable enable

namespace AIGuiders.Platform.CommandPlane;

/// <summary>ADR-0154 domain · object · intent triple.</summary>
public readonly record struct SlashSemanticFields(
    string Domain,
    string Object,
    string Intent,
    SlashPathRole PathRole = SlashPathRole.Canonical)
{
    public bool DomainOmittedInPath =>
        PathRole == SlashPathRole.Alias && !string.IsNullOrEmpty(Domain);
}
