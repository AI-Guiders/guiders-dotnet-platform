using System.Text.Json;
using AIGuiders.Platform.Authoring.Command.Bundles;
using AIGuiders.Platform.Authoring.Command.Catalog;
using AIGuiders.Platform.Authoring.Core;
using Xunit;

namespace AIGuiders.Platform.Authoring.Tests;

public sealed class CatalogConformanceSpecTests
{
    [Fact]
    public void Profiles_bundle_spec_vector()
    {
        var root = FindConformanceRoot();
        var json = File.ReadAllText(Path.Combine(root, "catalog", "profiles-bundle.spec.json"));
        using var doc = JsonDocument.Parse(json);
        var vector = doc.RootElement.GetProperty("vectors")[0];
        var text = vector.GetProperty("catalog").GetString()!;
        var expect = vector.GetProperty("expect");

        var result = CatalogParser.Parse(text, bundleLibrary: CatalogBundleLibrary.Federation);
        Assert.Equal(expect.GetProperty("planet").GetString(), result.Document!.Planet);
        var profile = result.Document.Profiles.First(p => p.Name == expect.GetProperty("profile").GetString());
        Assert.Equal(expect.GetProperty("menuEntries").GetInt32(), profile.Entries.Count);
    }

    [Fact]
    public void Grammar_mismatch_spec_vector()
    {
        var root = FindConformanceRoot();
        var json = File.ReadAllText(Path.Combine(root, "catalog", "profiles-bundle.spec.json"));
        using var doc = JsonDocument.Parse(json);
        var vector = doc.RootElement.GetProperty("vectors")[1];
        var fixture = vector.GetProperty("catalogFile").GetString()!;
        var text = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "Authoring", fixture));
        var result = CatalogParser.Parse(text);
        var code = vector.GetProperty("expectError").GetString()!;
        Assert.Contains(result.Diagnostics, d => d.Code.ToString() == code);
    }

    static string FindConformanceRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "docs", "conformance", "authoring");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException("authoring conformance root not found");
    }
}
