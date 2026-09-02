#nullable enable

namespace AIGuiders.Platform.Execution.Cockpit.Ids;

/// <summary>IDS feature palette contract (CIDE ADR 0079) — orthogonal to CDS instrument channels.</summary>
public interface IIdsFeatureSearch
{
    IdsFeatureHit[] Search(string query, int max, IReadOnlyList<(string Go, string Tool)> catalog);
}

/// <summary>One IDS palette hit (go/tool + score).</summary>
public readonly record struct IdsFeatureHit(string Go, int Score, string Tool);
