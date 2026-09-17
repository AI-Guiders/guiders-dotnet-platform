#nullable enable

using System.Text.Json;
using AIGuiders.Platform.Execution.CommandPlane;
using AIGuiders.Platform.Execution.CommandPlane.ArgSuggestions;
using AIGuiders.Platform.Execution.CommandPlane.Catalog;
using AIGuiders.Platform.Modeling.CommandPlane;
using AIGuiders.Platform.Modeling.Notations.Bracket;
using AIGuiders.Platform.Notations.Bracket;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class AttachCommandTests
{
    [Fact]
    public void FederationAttachCatalog_registers_root_and_verbs()
    {
        var catalog = CommandCatalogIndex.FromDescriptors(FederationAttachCatalog.AllDescriptors());
        Assert.True(catalog.TryGet("attach", out var root));
        Assert.Equal(FederationAttachCatalog.AttachCommandId, root.CommandId);
        Assert.Equal(CommandArgTailKind.Picker, root.ArgTailKind);

        Assert.True(catalog.TryGet("attach code", out var code));
        Assert.Equal("federation.attach.code", code.CommandId);
    }

    [Fact]
    public void AttachVerbArgSuggestionProvider_lists_all_verbs()
    {
        var catalog = CommandCatalogIndex.FromDescriptors(FederationAttachCatalog.AllDescriptors());
        Assert.True(catalog.TryGet("attach", out var route));

        var broker = FederationAttachSuggestions.CreateBroker();
        var request = ArgSuggestionRequest.Create(
            AttachSchemaCatalog.VerbSuggestionId,
            "",
            route,
            "attach");

        var choices = broker.GetSuggestions(request);
        Assert.Equal(6, choices.Count);
        Assert.Contains(choices, c => c.Value == "error");
        Assert.Contains(choices, c => c.Value == "manual");
    }

    [Fact]
    public void AttachVerbArgSuggestionProvider_filters_by_partial()
    {
        var catalog = CommandCatalogIndex.FromDescriptors(FederationAttachCatalog.AllDescriptors());
        Assert.True(catalog.TryGet("attach", out var route));

        var broker = FederationAttachSuggestions.CreateBroker();
        var request = ArgSuggestionRequest.Create(
            AttachSchemaCatalog.VerbSuggestionId,
            "doc",
            route,
            "attach");

        var choices = broker.GetSuggestions(request);
        Assert.Single(choices);
        Assert.Equal("document", choices[0].Value);
    }
}

public sealed class BracketKindCanonSpecTests
{
    sealed record BracketKindCanonVector(string Id, string Wire, bool ExpectRelationSpec);

    sealed record BracketKindCanonSpec(string Kind, IReadOnlyList<BracketKindCanonVector> Vectors);

    static BracketKindCanonSpec LoadSpec()
    {
        var json = ConformanceFixture.LoadEmbedded(
            "AIGuiders.Platform.Tests.Fixtures.LanguageIntelligence.bracket-kind-canon.spec.json");
        return JsonSerializer.Deserialize<BracketKindCanonSpec>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        })!;
    }

    [Fact]
    public void BracketKindCanon_spec_vectors_match_runtime()
    {
        var spec = LoadSpec();
        Assert.Equal("language-intelligence.bracket-kind-canon", spec.Kind);

        foreach (var vector in spec.Vectors)
        {
            Assert.True(
                BracketReader.Default.TryRead(
                    vector.Wire,
                    BracketProfiles.CdpSquareKeyValue,
                    out var wire,
                    out var error),
                $"{vector.Id}: {error}");

            var specParsed = BracketRelationWire.tryParseRelationSpec(wire!);
            if (vector.ExpectRelationSpec)
                Assert.NotNull(specParsed);
            else
                Assert.Null(specParsed);
        }
    }
}
