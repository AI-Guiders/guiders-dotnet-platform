using AIGuiders.Platform.IntermediateRepresentation.Command;
#nullable enable

namespace AIGuiders.Platform.CommandPlane;

public static class CatalogRoutePrefixArmExtensions
{
    public static PrefixArmSite ToPrefixArmSite(this CatalogRouteEntry route) =>
        PrefixArmSite.FromBindings(
            route.ResolvedConstructors,
            route.ArgHint,
            route.Help,
            route.ArgTailKind.ToString());
}
