#nullable enable

namespace AIGuiders.Platform.Execution.CommandPlane;

public sealed class PrefixArmCoordinator(
    ValueConstructorNavigator navigator,
    ValueConstructorRegistry registry)
{
    public bool TryHandle(
        string canonicalPath,
        string typedArgTail,
        PrefixArmSite site,
        ArgConstructorSession session,
        IReadOnlyList<IPrefixArmProfile> profiles,
        LocaleInputProfile? localeProfile,
        out PrefixArmResult? result)
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
            result = PrefixArmContinuedResult.Instance;
            return true;
        }

        if (profiles.Count == 0)
        {
            return false;
        }

        foreach (var profile in profiles)
        {
            if (!profile.TryMatch(partial, site, out var match)
                || match.Disposition == PrefixArmDisposition.NoMatch)
            {
                continue;
            }

            if (match.Disposition == PrefixArmDisposition.Ready
                && !string.IsNullOrWhiteSpace(match.Wire))
            {
                result = new PrefixArmReadyResult(
                    canonicalPath,
                    match.Wire,
                    match.DisplayTail ?? partial,
                    site.Help,
                    site.ArgTailKind);
                return true;
            }

            if (match.Disposition == PrefixArmDisposition.ArmConstructor
                && !string.IsNullOrWhiteSpace(match.RootConstructorId))
            {
                session.Start(match.RootConstructorId, canonicalPath);
                session.SetTypedArgTail(partial);
                if (match.Segments is { Count: > 0 })
                {
                    navigator.TryApplySegments(session.Draft!, match.Segments, registry, localeProfile);
                }

                result = PrefixArmContinuedResult.Instance;
                return true;
            }
        }

        return false;
    }

    public static bool AnyProfileMatches(
        IReadOnlyList<IPrefixArmProfile> profiles,
        string partial,
        PrefixArmSite site) =>
        profiles.Any(profile =>
            profile.TryMatch(partial, site, out var match)
            && match.Disposition != PrefixArmDisposition.NoMatch);
}
