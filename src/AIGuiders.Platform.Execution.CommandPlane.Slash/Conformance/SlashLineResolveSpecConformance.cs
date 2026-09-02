#nullable enable
using System.Text.Json;

namespace AIGuiders.Platform.Execution.CommandPlane.Conformance;

public static class SlashLineResolveSpecConformance
{
    public static SlashLineResolveSpecDocument Load(string json) =>
        JsonSerializer.Deserialize<SlashLineResolveSpecDocument>(json, JsonOptions)
        ?? throw new InvalidOperationException("Slash line-resolve spec JSON deserialized to null.");

    public static IReadOnlyList<string> ValidateDocument(SlashLineResolveSpecDocument spec)
    {
        var errors = new List<string>();
        foreach (var vector in spec.Vectors)
        {
            if (!TryValidateVector(spec, vector, out var error))
                errors.Add($"[{vector.Id}] {error}");
        }

        return errors;
    }

    public static bool TryValidateVector(
        SlashLineResolveSpecDocument spec,
        SlashLineResolveVector vector,
        out string error)
    {
        error = "";

        if (!spec.Catalogs.TryGetValue(vector.Catalog, out var catalogEntries))
        {
            error = $"unknown catalog \"{vector.Catalog}\".";
            return false;
        }

        var catalog = SlashSpecLoader.BuildCatalog(catalogEntries);
        if (!SlashLineResolver.TryResolveSlashLine(vector.SlashLine, catalog, out var actual))
        {
            error = $"TryResolveSlashLine returned false for \"{vector.SlashLine}\".";
            return false;
        }

        var expect = vector.Expect;
        if (!string.Equals(expect.CanonicalPath, actual.CanonicalPath, StringComparison.Ordinal))
        {
            error = $"canonicalPath expected \"{expect.CanonicalPath}\", got \"{actual.CanonicalPath}\".";
            return false;
        }

        if (!string.Equals(expect.ArgTail ?? "", actual.ArgTail ?? "", StringComparison.Ordinal))
        {
            error = $"argTail expected \"{expect.ArgTail}\", got \"{actual.ArgTail}\".";
            return false;
        }

        if (expect.IsRunnable != actual.IsRunnable)
        {
            error = $"isRunnable expected {expect.IsRunnable}, got {actual.IsRunnable}.";
            return false;
        }

        if (expect.IsCatalogMatch != actual.IsCatalogMatch)
        {
            error = $"isCatalogMatch expected {expect.IsCatalogMatch}, got {actual.IsCatalogMatch}.";
            return false;
        }

        if (expect.ArgTailKind is not null
            && !string.Equals(expect.ArgTailKind, actual.ArgTailKind.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            error = $"argTailKind expected {expect.ArgTailKind}, got {actual.ArgTailKind}.";
            return false;
        }

        return true;
    }

    static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };
}
