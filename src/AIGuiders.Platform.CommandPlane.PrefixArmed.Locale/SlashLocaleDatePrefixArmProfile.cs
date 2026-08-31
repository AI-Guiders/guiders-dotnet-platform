#nullable enable

namespace AIGuiders.Platform.CommandPlane;

/// <summary>PAC profile: locale date/range input (GUIDERS-ADR-0037 adapter).</summary>
public sealed class SlashLocaleDatePrefixArmProfile(ISlashCultureAmbient culture) : IPrefixArmProfile
{
    public string ProfileId => "locale-date";

    public bool TryMatch(string partial, PrefixArmSite site, out PrefixArmMatch match)
    {
        match = PrefixArmMatch.NoMatch;
        if (site.Constructors.Count == 0)
        {
            return false;
        }

        var localeProfile = SlashLocaleInputProfile.FromCulture(culture);
        if (!SlashLocaleDateParser.TryParse(partial, localeProfile, out var parts, out var completeness))
        {
            return false;
        }

        if (completeness == SlashLocaleDateCompleteness.CompleteRange
            && SlashLocaleDateParser.TryToRangeWire(parts, out var rangeWire))
        {
            match = new PrefixArmMatch(
                PrefixArmDisposition.Ready,
                rangeWire,
                partial);
            return true;
        }

        if (completeness == SlashLocaleDateCompleteness.CompleteDate
            && SlashLocaleDateParser.TryToDayWire(parts, out var dayWire))
        {
            match = new PrefixArmMatch(
                PrefixArmDisposition.Ready,
                dayWire,
                partial);
            return true;
        }

        if (completeness == SlashLocaleDateCompleteness.MonthYear
            && SlashLocaleDateParser.TryToMonthWire(parts, out var monthWire))
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
        IReadOnlyList<SlashConstructorBinding> bindings,
        SlashLocaleDateCompleteness completeness,
        out string rootId)
    {
        rootId = "";
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
}
