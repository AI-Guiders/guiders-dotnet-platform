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
    string? Group = null)
{
    public static SlashRouteEntry FromDescriptor(SlashCommandDescriptor d, string path) =>
        new(
            path,
            d.CommandId,
            d.Help ?? "",
            d.ArgTailKind,
            d.Domain,
            d.Object,
            d.Intent,
            SlashPathRole.Canonical,
            d.Group);

    public SlashSemanticFields SemanticFields => new(Domain, Object, Intent, PathRole);
}
