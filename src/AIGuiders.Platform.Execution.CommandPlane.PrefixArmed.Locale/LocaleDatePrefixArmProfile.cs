using AIGuiders.Platform.IntermediateRepresentation.Command;
#nullable enable

namespace AIGuiders.Platform.Execution.CommandPlane;

/// <summary>PAC profile: locale date/range input (GUIDERS-ADR-0037 adapter).</summary>
public sealed class LocaleDatePrefixArmProfile(ICultureAmbient culture) : IPrefixArmProfile
{
    public string ProfileId => "locale-date";

    public bool TryMatch(string partial, PrefixArmSite site, out PrefixArmMatch match)
    {
        match = PrefixArmMatch.NoMatch;
        if (site.Constructors.Count == 0)
        {
            return false;
        }

        var localeProfile = LocaleInputProfile.FromCulture(culture);
        if (!LocaleDateParser.TryParse(partial, localeProfile, out var parts, out var completeness))
        {
            return false;
        }

        if (completeness == LocaleDateCompleteness.CompleteRange
            && LocaleDateParser.TryToRangeWire(parts, out var rangeWire))
        {
            match = new PrefixArmMatch(
                PrefixArmDisposition.Ready,
                rangeWire,
                partial);
            return true;
        }

        if (completeness == LocaleDateCompleteness.CompleteDate
            && LocaleDateParser.TryToDayWire(parts, out var dayWire))
        {
            match = new PrefixArmMatch(
                PrefixArmDisposition.Ready,
                dayWire,
                partial);
            return true;
        }

        if (completeness == LocaleDateCompleteness.MonthYear
            && LocaleDateParser.TryToMonthWire(parts, out var monthWire))
        {
            match = new PrefixArmMatch(
                PrefixArmDisposition.Ready,
                monthWire,
                partial);
            return true;
        }

        if (!TryResolveRootConstructor(site.Constructors, completeness, out var rootId))
        {
            return false;
        }

        match = new PrefixArmMatch(
            PrefixArmDisposition.ArmConstructor,
            RootConstructorId: rootId,
            DisplayTail: partial,
            Segments: parts.ToWireSegments());
        return true;
    }

    static bool TryResolveRootConstructor(
        IReadOnlyList<ArgConstructorBinding> bindings,
        LocaleDateCompleteness completeness,
        out string rootId)
    {
        rootId = "";
        string? candidate = completeness switch
        {
            LocaleDateCompleteness.MonthYear => FindBinding(bindings, "month"),
            LocaleDateCompleteness.Partial => FindBinding(bindings, "month")
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

    static string? FindBinding(IReadOnlyList<ArgConstructorBinding> bindings, string token) =>
        bindings.FirstOrDefault(binding =>
            binding.ConstructorId.Contains(token, StringComparison.OrdinalIgnoreCase)
            || binding.Label.Contains(token, StringComparison.OrdinalIgnoreCase))?.ConstructorId;
}
