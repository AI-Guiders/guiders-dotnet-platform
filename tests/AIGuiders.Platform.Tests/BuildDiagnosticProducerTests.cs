#nullable enable

using AIGuiders.Platform.Execution.Ide.Session;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class BuildDiagnosticProducerTests
{
    [Fact]
    public void ParseMsBuildDiagnostics_parses_span_and_no_span_lines()
    {
        var output = """
            src/Foo.cs(12,3): error CS0246: The type or namespace name 'Missing' could not be found [D:\repo\App.csproj]
            src/Bar.cs : warning CS0168: The variable 'x' is declared but never used
            noise line without diagnostic shape
            """;

        var parsed = BuildDiagnosticProducer.ParseMsBuildDiagnostics(output, @"D:\repo");

        Assert.Equal(2, parsed.Count);
        Assert.Equal("D:/repo/src/Foo.cs", parsed[0].File);
        Assert.Equal(12, parsed[0].Line);
        Assert.Equal(3, parsed[0].Column);
        Assert.Equal("CS0246", parsed[0].Code);
        Assert.Contains("Missing", parsed[0].Message);

        Assert.Equal("D:/repo/src/Bar.cs", parsed[1].File);
        Assert.Equal(1, parsed[1].Line);
        Assert.Equal(1, parsed[1].Column);
        Assert.Equal("CS0168", parsed[1].Code);
    }

    [Fact]
    public void ParseMsBuildDiagnostics_deduplicates_identical_lines()
    {
        const string output = """
            src/Foo.cs(1,1): error CS0001: duplicate
            src/Foo.cs(1,1): error CS0001: duplicate
            """;

        var parsed = BuildDiagnosticProducer.ParseMsBuildDiagnostics(output, @"D:\repo");
        Assert.Single(parsed);
    }
}
