#nullable enable

using System.Globalization;

namespace AIGuiders.Platform.CommandPlane;

public static class SlashLocaleDisplayFormatter
{
    public static string FormatLeaf(
        SlashLeafConstructorDefinition leaf,
        IReadOnlyDictionary<string, string> segments,
        SlashLocaleInputProfile profile)
    {
        var displayValues = segments.ToDictionary(
            pair => pair.Key,
            pair => FormatSegmentValue(leaf, pair.Key, pair.Value, forWire: false, profile),
            StringComparer.OrdinalIgnoreCase);
        return ApplyPattern(leaf.DisplayPattern, displayValues);
    }

    public static string FormatSegmentValue(
        SlashLeafConstructorDefinition leaf,
        string segmentId,
        string value,
        bool forWire,
        SlashLocaleInputProfile profile)
    {
        var segment = leaf.Segments.FirstOrDefault(s =>
            s.SegmentId.Equals(segmentId, StringComparison.OrdinalIgnoreCase));
        var minWidth = forWire ? segment?.WireMinWidth : segment?.DisplayMinWidth;
        if (minWidth is > 0 && value.Length < minWidth)
        {
            return value.PadLeft(minWidth.Value, '0');
        }

        if (!forWire
            && segmentId.Equals("month", StringComparison.OrdinalIgnoreCase)
            && int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var month)
            && month is >= 1 and <= 12)
        {
            return profile.Culture.DateTimeFormat.GetMonthName(month);
        }

        return value;
    }

    static string ApplyPattern(string pattern, IReadOnlyDictionary<string, string> values)
    {
        var result = pattern;
        foreach (var (key, value) in values)
        {
            result = result.Replace($"{{{key}}}", value, StringComparison.OrdinalIgnoreCase);
        }

        return result;
    }
}
