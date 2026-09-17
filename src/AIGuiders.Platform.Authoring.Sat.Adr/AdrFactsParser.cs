using GdlCorrespondence = AIGuiders.Platform.Modeling.Documentation.Correspondence;
using Microsoft.FSharp.Core;

namespace AIGuiders.Platform.Authoring.Sat;

/// <summary>
/// Thin C# bridge to F# SSOT parser in <c>Modeling.Gdl.Correspondence</c> (SAT-002).
/// </summary>
public static class AdrFactsParser
{
    public static bool ContainsFactsBlock(string markdown) =>
        GdlCorrespondence.AdrFactsParser.containsFactsBlock(markdown);

    public static AdrFactsBlock? TryParse(string sourcePath, string markdown)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);
        ArgumentNullException.ThrowIfNull(markdown);

        var parsed = GdlCorrespondence.AdrFactsParser.tryParse(sourcePath, markdown);
        if (FSharpOption<GdlCorrespondence.AdrFactsBlock>.get_IsNone(parsed))
        {
            return null;
        }

        return Map(parsed!.Value);
    }

    private static AdrFactsBlock Map(GdlCorrespondence.AdrFactsBlock facts) =>
        new()
        {
            SourcePath = facts.SourcePath,
            AdrId = facts.AdrId is not null && FSharpOption<string>.get_IsSome(facts.AdrId) ? facts.AdrId.Value : null,
            GoldenIds = facts.GoldenIds,
            HoareObligations = facts.HoareObligations.Select(MapHoare).ToArray(),
            WellFormednessIds = facts.WellFormednessIds,
            VerifiedBy = facts.VerifiedBy.Select(r => new AdrVerifiedByRow(r.Subject, r.Evidence)).ToArray(),
        };

    private static HoareObligation MapHoare(GdlCorrespondence.HoareObligation h) =>
        new(h.Id, h.Precondition, h.Transform, h.Postcondition, h.RawExpression);
}
