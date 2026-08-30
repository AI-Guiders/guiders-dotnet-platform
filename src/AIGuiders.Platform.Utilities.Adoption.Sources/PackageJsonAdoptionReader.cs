using System.Text.Json;

namespace AIGuiders.Platform.Utilities.Adoption.Sources;

/// <summary>Format reader: npm package.json → federation scope pins.</summary>
public static class PackageJsonAdoptionReader
{
    public static AdoptionPartialFacts ReadFile(string path, string scopePrefix = "@aiguiders/")
    {
        var pins = new List<AdoptionPin>();
        try
        {
            using var stream = File.OpenRead(path);
            using var doc = JsonDocument.Parse(stream);
            CollectDeps(doc.RootElement, "dependencies", scopePrefix, pins);
            CollectDeps(doc.RootElement, "devDependencies", scopePrefix, pins);
        }
        catch
        {
            return AdoptionPartialFacts.Empty;
        }

        return new AdoptionPartialFacts(pins, []);
    }

    private static void CollectDeps(
        JsonElement root,
        string property,
        string scopePrefix,
        List<AdoptionPin> pins)
    {
        if (!root.TryGetProperty(property, out var deps))
            return;

        foreach (var entry in deps.EnumerateObject())
        {
            if (!entry.Name.StartsWith(scopePrefix, StringComparison.Ordinal))
                continue;

            pins.Add(new AdoptionPin(entry.Name, entry.Value.GetString(), AdoptionPortKind.NpmPackage));
        }
    }
}
