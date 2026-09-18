#nullable enable

using AIGuiders.Platform.Notations.Bracket;

namespace AIGuiders.Platform.Execution.LanguageIntelligence.Relations;

/// <summary>Legacy Family:navigation wire ingest — flatten to <see cref="NavResolveAxes"/> (plan §10).</summary>
public static class LegacyNavWireIngest
{
    public static bool TryParse(string bracketOrInner, out NavResolveAxes axes, out string? error)
    {
        axes = default!;
        error = null;
        if (string.IsNullOrWhiteSpace(bracketOrInner))
            return false;

        if (!BracketReader.Default.TryRead(
                bracketOrInner,
                BracketProfiles.CdpSquareKeyValue,
                out var wire,
                out error)
            || wire is null)
            return false;

        try
        {
            var probe = ParseNavProbe(wire);
            if (WireFamilyClassifier.Classify(probe, out error) != BracketAxisFamily.Navigation)
                return false;

            axes = Flatten(probe);
            return !string.IsNullOrWhiteSpace(axes.File)
                   || !string.IsNullOrWhiteSpace(axes.Command)
                   || !string.IsNullOrWhiteSpace(axes.Go);
        }
        catch (ArgumentException ex)
        {
            error = ex.Message;
            return false;
        }
    }

    static NavResolveAxes Flatten(WireFamilyClassifier.Probe probe)
    {
        var file = probe.File;
        int? line = probe.LineStart;
        string? member = probe.MemberKey;
        if (probe.NestedAnchor is { } nested)
        {
            file ??= nested.File;
            line ??= nested.LineStart;
            member ??= nested.MemberKey;
        }

        return new NavResolveAxes(
            File: file,
            Line: line,
            Command: probe.Command,
            Go: probe.Go,
            Member: member);
    }

    static WireFamilyClassifier.Probe ParseNavProbe(NormalizedBracketWire wire) =>
        ParseNavProbe(wire, out _);

    static WireFamilyClassifier.Probe ParseNavProbe(NormalizedBracketWire wire, out bool legacyNavigate)
    {
        string? file = null;
        string? member = null;
        int? lineStart = null;
        string? family = null;
        string? command = null;
        string? go = null;
        WireFamilyClassifier.Probe? nested = null;
        legacyNavigate = false;

        foreach (var axis in wire.Axes)
        {
            if (!RelationWireBoundary.AxisAlias.TryGetValue(axis.Key, out var canon))
                throw new ArgumentException($"unknown_axis:{axis.Key}");

            var val = axis.Value.Trim();
            switch (canon)
            {
                case "Family":
                    family = NormalizeFamilyName(val);
                    break;
                case "Navigate":
                    if (IsTruthy(val))
                        legacyNavigate = true;
                    break;
                case "File":
                    file = val;
                    break;
                case "Member":
                    member = val;
                    break;
                case "Line":
                    RelationWireBoundary.ParseLine(val, out lineStart, out _);
                    break;
                case "Command":
                    command = val.ToLowerInvariant();
                    break;
                case "Go":
                    go = val;
                    break;
                case "Anchor":
                    nested = axis.Nested is not null
                        ? ParseNavProbe(axis.Nested, out _)
                        : ParseNavProbe(RelationWireBoundary.ReadWire(val), out _);
                    break;
                default:
                    throw new ArgumentException($"nav_ingest_unsupported_axis:{axis.Key}");
            }
        }

        if (legacyNavigate && string.IsNullOrWhiteSpace(family))
            family = "navigation";

        return new WireFamilyClassifier.Probe(
            file,
            member,
            lineStart,
            ScopeKind: null,
            Role: null,
            XmlPath: null,
            Attr: null,
            family,
            command,
            go,
            nested,
            TextNeedle: null,
            TypeKey: null);
    }

    static string? NormalizeFamilyName(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;
        var v = raw.Trim().ToLowerInvariant();
        return v switch
        {
            "c#" or "csharp" or "cs" => "code",
            "nav" => "navigation",
            _ => v
        };
    }

    static bool IsTruthy(string val) =>
        val.Equals("true", StringComparison.OrdinalIgnoreCase)
        || val.Equals("1", StringComparison.OrdinalIgnoreCase)
        || val.Equals("yes", StringComparison.OrdinalIgnoreCase);
}
