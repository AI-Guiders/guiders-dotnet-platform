#nullable enable
using System.Text.Json;

namespace AIGuiders.Platform.Execution.Cockpit.Cds;

/// <summary>CDS go-verb catalog entry: organ tool + default args (ADR 0036).</summary>
public readonly record struct DeskGoMapEntry(
    string Tool,
    IReadOnlyDictionary<string, JsonElement>? Defaults);
