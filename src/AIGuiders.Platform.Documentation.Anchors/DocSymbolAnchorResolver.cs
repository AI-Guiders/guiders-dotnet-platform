#nullable enable

using AIGuiders.Platform.IntermediateRepresentation.Bracket;

namespace AIGuiders.Platform.Documentation.Anchors;

/// <summary>Resolve <c>Family:doc</c> bracket wires against a symbol catalog (GUIDERS-ADR-0027).</summary>
public sealed class DocSymbolAnchorResolver
{
    readonly IDocSymbolCatalog _catalog;

    public DocSymbolAnchorResolver(IDocSymbolCatalog catalog) => _catalog = catalog;

    public bool TryResolve(NormalizedBracketWire wire, out string error)
    {
        error = "";
        if (!TryGetAxis(wire, "Family", out var family) || !family.Equals("doc", StringComparison.OrdinalIgnoreCase))
            return Fail("need_Family_doc", out error);

        if (!TryGetAxis(wire, "Type", out var typeName))
            return Fail("need_Type", out error);

        TryGetAxis(wire, "Package", out var packageHint);
        if (!_catalog.TypeExists(typeName, NullIfEmpty(packageHint)))
            return Fail($"type_not_found:{typeName}", out error);

        if (TryGetAxis(wire, "Member", out var member)
            && !_catalog.MemberExists(typeName, member, NullIfEmpty(packageHint)))
            return Fail($"member_not_found:{typeName}.{member}", out error);

        return true;
    }

    static bool TryGetAxis(NormalizedBracketWire wire, string key, out string value)
    {
        value = "";
        foreach (var axis in wire.Axes)
        {
            if (axis.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                value = axis.Value.Trim();
                return value.Length > 0;
            }
        }

        return false;
    }

    static string? NullIfEmpty(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    static bool Fail(string message, out string error)
    {
        error = message;
        return false;
    }
}
