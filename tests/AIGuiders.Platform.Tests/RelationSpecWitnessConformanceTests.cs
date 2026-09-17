#nullable enable
using System.Text.Json;
using AIGuiders.Platform.Modeling.LanguageIntelligence.Relations;
using AIGuiders.Platform.Modeling.Notations.Bracket;
using AIGuiders.Platform.Notations.Bracket;
using Microsoft.FSharp.Core;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class RelationSpecWitnessConformanceTests
{
    sealed record RelationSpecWitnessVector(string Id, string Wire, string? ExpectCase);

    sealed record RelationSpecWitnessSpec(string Kind, IReadOnlyList<RelationSpecWitnessVector> Vectors);

    static RelationSpecWitnessSpec LoadSpec()
    {
        var json = ConformanceFixture.LoadEmbedded(
            "AIGuiders.Platform.Tests.Fixtures.LanguageIntelligence.relation-spec-witness.spec.json");
        return JsonSerializer.Deserialize<RelationSpecWitnessSpec>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        })!;
    }

    [Fact]
    public void RelationSpec_witness_spec_vectors_match_Kind_canon()
    {
        var spec = LoadSpec();
        Assert.Equal("language-intelligence.relation-spec-witness", spec.Kind);

        foreach (var vector in spec.Vectors)
        {
            Assert.True(
                BracketReader.Default.TryRead(
                    vector.Wire,
                    BracketProfiles.CdpSquareKeyValue,
                    out var wire,
                    out var error),
                $"{vector.Id}: {error}");

            var parsed = BracketRelationWire.tryParseRelationSpec(wire!);
            if (vector.ExpectCase is null)
            {
                Assert.True(parsed is null || !FSharpOption<RelationSpec>.get_IsSome(parsed), vector.Id);
                continue;
            }

            Assert.NotNull(parsed);
            Assert.True(FSharpOption<RelationSpec>.get_IsSome(parsed!), vector.Id);
            Assert.Equal(vector.ExpectCase, RelationSpecCaseName(parsed!.Value), ignoreCase: true);
        }
    }

    static string RelationSpecCaseName(RelationSpec spec) =>
        spec switch
        {
            RelationSpec.CodeEdit _ => "CodeEdit",
            RelationSpec.DocToCode _ => "DocToCode",
            RelationSpec.Diag _ => "Diag",
            RelationSpec.Address _ => "Address",
            RelationSpec.Nav _ => "Nav",
            RelationSpec.Resource _ => "Resource",
            _ => throw new InvalidOperationException("unknown RelationSpec case"),
        };
}
