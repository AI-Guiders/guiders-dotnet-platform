using AIGuiders.Platform.Modeling.Notations.Keyboard;
#nullable enable
using System.Text.Json.Serialization;

namespace AIGuiders.Platform.Notations.Keyboard.Quarry;

public sealed record QuarryOracleStepJson(
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("mods")] string? Mods,
    [property: JsonPropertyName("key")] string Key);

public sealed record QuarryOracleWireJson(
    [property: JsonPropertyName("wire")] string Wire,
    [property: JsonPropertyName("steps")] IReadOnlyList<QuarryOracleStepJson> Steps);

public static class QuarryOracleIrMapper
{
    public static NormalizedKeySequence ToNormalized(IReadOnlyList<QuarryOracleStepJson> steps)
    {
        if (steps.Count == 0)
            return NormalizedKeySequence.Empty;

        var list = new List<NormalizedSequenceStep>(steps.Count);
        foreach (var step in steps)
        {
            if (string.Equals(step.Kind, "plain", StringComparison.OrdinalIgnoreCase))
            {
                list.Add(new NormalizedPlainKeyStep(step.Key));
                continue;
            }

            if (string.Equals(step.Kind, "chord", StringComparison.OrdinalIgnoreCase))
            {
                list.Add(new NormalizedChordStep(ParseMods(step.Mods), step.Key));
                continue;
            }

            throw new InvalidOperationException($"Unknown oracle step kind \"{step.Kind}\".");
        }

        return new NormalizedKeySequence(list);
    }

    public static ChordModifierKeys ParseMods(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return ChordModifierKeys.None;

        ChordModifierKeys mods = 0;
        foreach (var token in raw.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            mods |= token switch
            {
                "Control" or "Ctrl" => ChordModifierKeys.Control,
                "Alt" => ChordModifierKeys.Alt,
                "Shift" => ChordModifierKeys.Shift,
                "Meta" or "Super" or "Command" or "D" => ChordModifierKeys.Meta,
                _ => throw new ArgumentException($"Unknown modifier token: {token}", nameof(raw)),
            };
        }

        return mods;
    }
}
