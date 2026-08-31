#nullable enable

namespace AIGuiders.Platform.IntermediateRepresentation.Command;

/// <summary>Resolved catalog row: path + command_id (CIDE quarry, headless).</summary>
public readonly record struct CatalogRouteEntry(
    string Path,
    string CommandId,
    string Help,
    CommandArgTailKind ArgTailKind,
    string Domain = "",
    string Object = "",
    string Intent = "",
    CatalogPathRole PathRole = CatalogPathRole.Canonical,
    string? Group = null,
    string ArgTail = "",
    IReadOnlyList<CommandPickerChoice>? ArgPickerChoices = null,
    string? ArgHint = null,
    IReadOnlyList<ArgConstructorBinding>? ArgConstructors = null)
{
    public IReadOnlyList<CommandPickerChoice> ResolvedPickerChoices => ArgPickerChoices ?? [];
    public IReadOnlyList<ArgConstructorBinding> ResolvedConstructors => ArgConstructors ?? [];

    public static CatalogRouteEntry FromDescriptor(CommandDescriptor d, string path) =>
        FromDescriptor(d, path, ResolvePathRole(d, path));

    public static CatalogRouteEntry FromDescriptor(CommandDescriptor d, string path, CatalogPathRole pathRole) =>
        new(
            path,
            d.CommandId,
            d.Help ?? "",
            d.ArgTailKind,
            d.Domain,
            d.Object,
            d.Intent,
            pathRole,
            d.Group,
            d.ArgTail,
            d.ArgPickerChoices,
            d.ArgHint,
            d.ArgConstructors);

    static CatalogPathRole ResolvePathRole(CommandDescriptor d, string path) =>
        string.Equals(NormalizePath(path), NormalizePath(d.Path), StringComparison.OrdinalIgnoreCase)
            ? CatalogPathRole.Canonical
            : CatalogPathRole.Alias;

    static string NormalizePath(string path)
    {
        var p = path.Trim();
        if (p.StartsWith('/'))
            p = p[1..];
        return p.Trim();
    }

    public CatalogSemanticFields SemanticFields => new(Domain, Object, Intent, PathRole);
}
