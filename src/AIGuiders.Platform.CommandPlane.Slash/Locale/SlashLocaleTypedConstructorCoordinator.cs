#nullable enable

using System.Globalization;

namespace AIGuiders.Platform.CommandPlane;

public sealed class SlashLocaleTypedConstructorCoordinator(
    SlashValueConstructorNavigator navigator,
    SlashValueConstructorRegistry registry)
{
    public bool TryHandleArgTail(
        SlashLineResolver.SlashLineResolution line,
        SlashRouteEntry route,
        string typedArgTail,
        SlashConstructorSession session,
        SlashLocaleInputProfile profile,
        out SlashCompletionResult? result)
    {
        result = null;
        var partial = typedArgTail.Trim();
        if (partial.Length == 0 || route.ResolvedConstructors.Count == 0)
        {
            return false;
        }

        if (session.IsActive)
        {
            session.SetTypedArgTail(partial);
            result = session.GetCompletionResult(partial, profile);
            return true;
        }

        if (!SlashLocaleDateParser.TryParse(partial, profile, out var parts, out var completeness))
        {
            return false;
        }

        if (completeness == SlashLocaleDateCompleteness.CompleteRange
            && SlashLocaleDateParser.TryToRangeWire(parts, out var rangeWire))
        {
            result = BuildReadyResult(line, route, rangeWire, profile, partial);
            return true;
        }

        if (completeness == SlashLocaleDateCompleteness.CompleteDate
            && SlashLocaleDateParser.TryToDayWire(parts, out var dayWire))
        {
            result = BuildReadyResult(line, route, dayWire, profile, partial);
            return true;
        }

        if (completeness == SlashLocaleDateCompleteness.MonthYear
            && SlashLocaleDateParser.TryToMonthWire(parts, out var monthWire))
        {
            result = BuildReadyResult(line, route, monthWire, profile, partial);
            return true;
        }

        if (!TryResolveRootConstructor(route, completeness, out var rootId))
        {
            return false;
        }

        session.Start(rootId, line.CanonicalPath);
        session.SetTypedArgTail(partial);
        session.TryApplyLocaleParts(parts, registry, navigator, profile);
        result = session.GetCompletionResult(partial, profile);
        return true;
    }

    static bool TryResolveRootConstructor(
        SlashRouteEntry route,
        SlashLocaleDateCompleteness completeness,
        out string rootId)
    {
        rootId = "";
        var bindings = route.ResolvedConstructors;
        string? candidate = completeness switch
        {
            SlashLocaleDateCompleteness.MonthYear => FindBinding(bindings, "month"),
            SlashLocaleDateCompleteness.Partial => FindBinding(bindings, "month")
                ?? FindBinding(bindings, "range")
                ?? bindings.FirstOrDefault()?.ConstructorId,
            _ => FindBinding(bindings, "range") ?? bindings.FirstOrDefault()?.ConstructorId,
        };

        if (string.IsNullOrWhiteSpace(candidate))
        {
            return false;
        }

        rootId = candidate;
        return true;
    }

    static string? FindBinding(IReadOnlyList<SlashConstructorBinding> bindings, string token) =>
        bindings.FirstOrDefault(binding =>
            binding.ConstructorId.Contains(token, StringComparison.OrdinalIgnoreCase)
            || binding.Label.Contains(token, StringComparison.OrdinalIgnoreCase))?.ConstructorId;

    static SlashCompletionResult BuildReadyResult(
        SlashLineResolver.SlashLineResolution line,
        SlashRouteEntry route,
        string wire,
        SlashLocaleInputProfile profile,
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
