#nullable enable
using System.Reflection;
using AIGuiders.Platform.Modeling.Notations.Bracket;
using AIGuiders.Platform.Notations.Bracket.Conformance;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class BracketDocSymbolConformanceTests
{
    [Fact]
    public void Bracket_doc_symbol_spec_vectors_pass()
    {
        var json = LoadEmbedded("AIGuiders.Platform.Tests.Fixtures.Notation.bracket-doc-symbol.spec.json");
        var spec = BracketSpecConformance.Load(json);
        var profile = BracketProfiles.DocSymbol;
        var errors = new List<string>();
        foreach (var vector in spec.Vectors)
        {
            if (!BracketSpecConformance.TryValidateVector(vector, profile, out var error))
                errors.Add($"[{vector.Id}] {error}");
        }

        Assert.Empty(errors);
    }

    static string LoadEmbedded(string resourceName)
    {
        var asm = Assembly.GetExecutingAssembly();
        using var stream = asm.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Missing embedded resource: {resourceName}");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
