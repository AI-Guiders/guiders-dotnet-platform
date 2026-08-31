#nullable enable

namespace AIGuiders.Platform.CommandPlane;

public sealed class SlashPrefixArmedCompletionCoordinator(
    SlashValueConstructorNavigator navigator,
    SlashValueConstructorRegistry registry)
{
    public bool TryHandleArgTail(
        SlashLineResolver.SlashLineResolution line,
        SlashRouteEntry route,
        string typedArgTail,
        SlashConstructorSession session,
        IReadOnlyList<ISlashPrefixArmProfile> profiles,
        SlashLocaleInputProfile? localeProfile,
        out SlashCompletionResult? result)
    {
        result = null;
        var partial = typedArgTail.Trim();
        if (partial.Length == 0)
        {
            return false;
        }

        if (session.IsActive)
        {
            session.SetTypedArgTail(partial);
            result = session.GetCompletionResult(partial, localeProfile);
            return true;
        }

        if (profiles.Count == 0)
        {
            return false;
        }

        foreach (var profile in profiles)
        {
            if (!profile.TryMatch(partial, route, out var match)
                || match.Disposition == SlashPrefixArmDisposition.NoMatch)
            {
                continue;
            }

            if (match.Disposition == SlashPrefixArmDisposition.Ready
                && !string.IsNullOrWhiteSpace(match.Wire))
            {
                result = BuildReadyResult(line, route, match.Wire, match.DisplayTail ?? partial);
                return true;
            }

            if (match.Disposition == SlashPrefixArmDisposition.ArmConstructor
                && !string.IsNullOrWhiteSpace(match.RootConstructorId))
            {
                session.Start(match.RootConstructorId, line.CanonicalPath);
                session.SetTypedArgTail(partial);
                if (match.Segments is { Count: > 0 })
                {
                    navigator.TryApplySegments(session.Draft!, match.Segments, registry, localeProfile);
                }

                result = session.GetCompletionResult(partial, localeProfile);
                return true;
            }
        }

        return false;
    }

    public static bool AnyProfileMatches(
        IReadOnlyList<ISlashPrefixArmProfile> profiles,
        string partial,
        SlashRouteEntry route) =>
        profiles.Any(profile =>
            profile.TryMatch(partial, route, out var match)
            && match.Disposition != SlashPrefixArmDisposition.NoMatch);

    static SlashCompletionResult BuildReadyResult(
        SlashLineResolver.SlashLineResolution line,
        SlashRouteEntry route,
        string wire,
        string displayTail)
    {
        var breadcrumb = "/" + line.CanonicalPath + " › " + displayTail;
        return new SlashCompletionResult(
            [],
            new SlashInputGuidance(
                SlashInputMode.Ready,
                breadcrumb,
                "Press Enter to run",
                route.Help,
                line.CanonicalPath,
                route.ArgTailKind.ToString(),
                wire,
                displayTail));
    }
}
