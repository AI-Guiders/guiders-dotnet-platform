namespace AIGuiders.Platform.Authoring.Core;

/// <summary>Dotted kv keys → table rows (GUIDERS-ADR-0048 §3).</summary>
public static class KvDesugar
{
    public static IReadOnlyList<Dictionary<string, string>> ProfileRows(IEnumerable<AuthoringLine> body)
    {
        var rows = new List<Dictionary<string, string>>();
        foreach (var line in body)
        {
            if (!KvSurface.TryParsePair(line.Text, out var key, out var value))
            {
                continue;
            }

            if (TryParseBundleShorthand(key, value, out var profile, out var bundleRef))
            {
                rows.Add(new(StringComparer.OrdinalIgnoreCase)
                {
                    ["profile"] = profile,
                    ["entry"] = "bundle",
                    ["ref"] = bundleRef,
                });
                continue;
            }

            var segments = key.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (segments.Length < 3)
            {
                continue;
            }

            rows.Add(new(StringComparer.OrdinalIgnoreCase)
            {
                ["profile"] = segments[0],
                ["arg"] = segments[1],
                ["entry"] = segments[2],
                ["ref"] = value,
            });
        }

        return rows;
    }

    public static IReadOnlyList<Dictionary<string, string>> HelpRows(IEnumerable<AuthoringLine> body)
    {
        var rows = new List<Dictionary<string, string>>();
        foreach (var line in body)
        {
            if (!KvSurface.TryParsePair(line.Text, out var key, out var value))
            {
                continue;
            }

            var dot = key.LastIndexOf('.');
            if (dot <= 0 || dot >= key.Length - 1)
            {
                continue;
            }

            rows.Add(new(StringComparer.OrdinalIgnoreCase)
            {
                ["target"] = key[..dot].Trim(),
                ["field"] = key[(dot + 1)..].Trim(),
                ["text"] = value,
            });
        }

        return rows;
    }

    static bool TryParseBundleShorthand(string key, string value, out string profile, out string bundleRef)
    {
        profile = key.Trim();
        bundleRef = "";
        if (!value.StartsWith("bundle ", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        bundleRef = value["bundle ".Length..].Trim();
        return profile.Length > 0 && bundleRef.Length > 0;
    }
}
