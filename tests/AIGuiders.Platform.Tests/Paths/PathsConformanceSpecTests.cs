using System.Text.Json;
using AIGuiders.Platform.Modeling.Paths;
using Xunit;

namespace AIGuiders.Platform.Tests.Paths;

public sealed class PathsConformanceSpecTests
{
    [Fact]
    public void Logical_normalize_matches_conformance_spec()
    {
        var root = FindConformanceRoot();
        var json = File.ReadAllText(Path.Combine(root, "paths", "logical-normalize.spec.json"));
        using var doc = JsonDocument.Parse(json);
        foreach (var row in doc.RootElement.GetProperty("cases").EnumerateArray())
        {
            var input = row.GetProperty("input").GetString()!;
            var expected = row.GetProperty("normalize").GetString()!;
            Assert.Equal(expected, LogicalPath.Normalize(input));

            if (row.TryGetProperty("doc", out var docPath))
                Assert.Equal(docPath.GetString(), new LogicalPath(input).AsDocPath().Value);
        }
    }

    static string FindConformanceRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "docs", "conformance");
            if (Directory.Exists(candidate))
                return candidate;
            dir = dir.Parent;
        }

        throw new InvalidOperationException("docs/conformance not found");
    }
}
