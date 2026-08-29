#nullable enable

namespace AIGuiders.Platform.CommandPlane;

/// <summary>Resolved catalog row: path + command_id (CIDE quarry, headless).</summary>
public readonly record struct SlashRouteEntry(
    string SlashPath,
    string CommandId,
    string Help,
    SlashArgTailKind ArgTailKind,
    string Domain = "",
    string Object = "",
    string Intent = "",
    SlashPathRole PathRole = SlashPathRole.Canonical,
    string? Group = null,
    string ArgTail = "",
    IReadOnlyList<SlashPickerChoice>? ArgPickerChoices = null)
{
    public IReadOnlyList<SlashPickerChoice> ResolvedPickerChoices => ArgPickerChoices ?? [];

    public static SlashRouteEntry FromDescriptor(SlashCommandDescriptor d, string path) =>
        FromDescriptor(d, path, ResolvePathRole(d, path));

    public static SlashRouteEntry FromDescriptor(SlashCommandDescriptor d, string path, SlashPathRole pathRole) =>
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
            d.ArgPickerChoices);

    static SlashPathRole ResolvePathRole(SlashCommandDescriptor d, string path) =>
        string.Equals(NormalizePath(path), NormalizePath(d.Path), StringComparison.OrdinalIgnoreCase)
            ? SlashPathRole.Canonical
            : SlashPathRole.Alias;

    static string NormalizePath(string path)
    {
        var p = path.Trim();
        if (p.StartsWith('/'))
            p = p[1..];
        return p.Trim();
    }

    public SlashSemanticFields SemanticFields => new(Domain, Object, Intent, PathRole);
}
