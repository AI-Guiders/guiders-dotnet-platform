#nullable enable
using System.Reflection;
using AIGuiders.Platform.Notations.Bracket.Conformance;
using AIGuiders.Platform.Notations.Conformance;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class NotationConformanceTests
{
    [Fact]
    public void Command_slash_vectors_conform()
    {
        var spec = LoadSpec("AIGuiders.Platform.Tests.Fixtures.Notation.command-slash.spec.json");
        Assert.Equal("command-slash", spec.Surface);
        Assert.Empty(NotationSpecConformance.ValidateDocument(spec));
    }

    [Fact]
    public void Argument_kv_vectors_conform()
    {
        var spec = LoadSpec("AIGuiders.Platform.Tests.Fixtures.Notation.argument-kv.spec.json");
        Assert.Equal("argument-kv", spec.Surface);
        Assert.Empty(NotationSpecConformance.ValidateDocument(spec));
    }

    [Fact]
    public void Invocation_parity_vectors_conform()
    {
        var spec = LoadSpec("AIGuiders.Platform.Tests.Fixtures.Notation.invocation-parity.spec.json");
        Assert.Equal("invocation-parity", spec.Surface);
        Assert.Empty(NotationSpecConformance.ValidateDocument(spec));
    }

    [Fact]
    public void Argument_delimited_vectors_conform()
    {
        var spec = LoadSpec("AIGuiders.Platform.Tests.Fixtures.Notation.argument-delimited.spec.json");
        Assert.Equal("argument-delimited", spec.Surface);
        Assert.Empty(NotationSpecConformance.ValidateDocument(spec));
    }

    [Fact]
    public void Command_console_vectors_conform()
    {
        var spec = LoadSpec("AIGuiders.Platform.Tests.Fixtures.Notation.command-console.spec.json");
        Assert.Equal("command-console", spec.Surface);
        Assert.Empty(NotationSpecConformance.ValidateDocument(spec));
    }

    [Fact]
    public void Argument_positional_vectors_conform()
    {
        var spec = LoadSpec("AIGuiders.Platform.Tests.Fixtures.Notation.argument-positional.spec.json");
        Assert.Equal("argument-positional", spec.Surface);
        Assert.Empty(NotationSpecConformance.ValidateDocument(spec));
    }

    [Fact]
    public void Bracket_cdp_square_kv_vectors_conform()
    {
        var json = LoadEmbedded("AIGuiders.Platform.Tests.Fixtures.Notation.bracket-cdp-square-kv.spec.json");
        var spec = BracketSpecConformance.Load(json);
        Assert.Equal("bracket-cdp-square-kv", spec.Surface);
        Assert.Empty(BracketSpecConformance.ValidateDocument(spec));
    }

    [Fact]
    public void Argument_cli_vectors_conform()
    {
        var spec = LoadSpec("AIGuiders.Platform.Tests.Fixtures.Notation.argument-cli.spec.json");
        Assert.Equal("argument-cli", spec.Surface);
        Assert.Empty(NotationSpecConformance.ValidateDocument(spec));
    }

    static NotationSpecDocument LoadSpec(string resourceName) =>
        NotationSpecConformance.Load(LoadEmbedded(resourceName));

    static string LoadEmbedded(string resourceName)
    {
        var asm = Assembly.GetExecutingAssembly();
        using var stream = asm.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Missing embedded resource: {resourceName}");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
