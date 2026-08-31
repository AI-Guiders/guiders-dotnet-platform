#nullable enable

namespace AIGuiders.Platform.CommandPlane;

public static class SlashPrefixArmSite
{
    public static PrefixArmSite ToPrefixArmSite(this SlashRouteEntry route) =>
        PrefixArmSite.FromBindings(
            route.ResolvedConstructors,
            route.ArgHint,
            route.Help,
            route.ArgTailKind.ToString());
}
