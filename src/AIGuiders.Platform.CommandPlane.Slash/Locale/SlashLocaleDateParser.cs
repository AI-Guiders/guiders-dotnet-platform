#nullable enable

using System.Globalization;
using System.Text;

namespace AIGuiders.Platform.CommandPlane;

public static class SlashLocaleDateParser
{
    public static bool TryParse(
        string input,
        SlashLocaleInputProfile profile,
        out SlashLocaleDateParts parts,
        out SlashLocaleDateCompleteness completeness)
    {
        parts = new SlashLocaleDateParts();
        completeness = SlashLocaleDateCompleteness.Empty;
        var text = input.Trim();
        if (text.Length == 0)
        {
            return false;
        }

        if (TrySplitRange(text, out var left, out var right))
        {
            if (!TryParseSingle(left, profile, out var fromParts, out var fromComplete)
                || !TryParseSingle(right, profile, out var toParts, out var toComplete)
                || fromComplete != SlashLocaleDateCompleteness.CompleteDate
                || toComplete != SlashLocaleDateCompleteness.CompleteDate)
            {
                return false;
            }

            parts = new SlashLocaleDateParts
            {
                Day = fromParts.Day,
                Month = fromParts.Month,
                Year = fromParts.Year,
                RangeEnd = toParts,
            };
            completeness = SlashLocaleDateCompleteness.CompleteRange;
            return true;
        }

        return TryParseSingle(text, profile, out parts, out completeness);
    }

    public static bool TryParseSingle(
        string input,
        SlashLocaleInputProfile profile,
        out SlashLocaleDateParts parts,
        out SlashLocaleDateCompleteness completeness)
    {
        parts = new SlashLocaleDateParts();
        completeness = SlashLocaleDateCompleteness.Empty;
        var text = input.Trim();
        if (text.Length == 0)
        {
            return false;
        }

        var tokens = Tokenize(text, profile.Separators);
        if (tokens.Count == 0)
        {
            return false;
        }

        if (TryParseMonthYearTokens(tokens, out parts))
        {
            completeness = SlashLocaleDateCompleteness.MonthYear;
            return true;
        }

        var values = new int?[3];
        for (var i = 0; i < tokens.Count && i < profile.FieldOrder.Count; i++)
        {
            if (!int.TryParse(tokens[i], NumberStyles.None, CultureInfo.InvariantCulture, out var number))
            {
                completeness = SlashLocaleDateCompleteness.Partial;
                return true;
            }

            values[(int)profile.FieldOrder[i]] = number;
        }

        parts = new SlashLocaleDateParts
        {
            Day = values[(int)SlashLocaleDateField.Day],
            Month = values[(int)SlashLocaleDateField.Month],
            Year = values[(int)SlashLocaleDateField.Year],
        };

        if (parts is { HasDay: true, HasMonth: true, HasYear: true })
        {
            completeness = SlashLocaleDateCompleteness.CompleteDate;
            return true;
        }

        if (parts is { HasMonth: true, HasYear: true } && !parts.HasDay)
        {
            completeness = SlashLocaleDateCompleteness.MonthYear;
            return true;
        }

        completeness = SlashLocaleDateCompleteness.Partial;
        return true;
    }

    static bool TryParseMonthYearTokens(IReadOnlyList<string> tokens, out SlashLocaleDateParts parts)
    {
        parts = new SlashLocaleDateParts();
        if (tokens.Count != 2)
        {
            return false;
        }

        if (tokens[1].Length == 4
            && int.TryParse(tokens[0], NumberStyles.None, CultureInfo.InvariantCulture, out var month)
            && int.TryParse(tokens[1], NumberStyles.None, CultureInfo.InvariantCulture, out var year)
            && month is >= 1 and <= 12)
        {
            parts = new SlashLocaleDateParts { Month = month, Year = year };
            return true;
        }

        if (tokens[0].Length == 4
            && int.TryParse(tokens[0], NumberStyles.None, CultureInfo.InvariantCulture, out year)
            && int.TryParse(tokens[1], NumberStyles.None, CultureInfo.InvariantCulture, out month)
            && month is >= 1 and <= 12)
        {
            parts = new SlashLocaleDateParts { Month = month, Year = year };
            return true;
        }

        return false;
    }

    public static string FormatDate(SlashLocaleDateParts parts, SlashLocaleInputProfile profile)
    {
        var values = new Dictionary<SlashLocaleDateField, string>();
        if (parts.Day is > 0)
        {
            values[SlashLocaleDateField.Day] = parts.Day.Value.ToString("00", profile.Culture);
        }

        if (parts.Month is > 0)
        {
            values[SlashLocaleDateField.Month] = parts.Month.Value.ToString("00", profile.Culture);
        }

        if (parts.Year is > 0)
        {
            values[SlashLocaleDateField.Year] = parts.Year.Value.ToString("0000", profile.Culture);
        }

        var builder = new StringBuilder();
        for (var i = 0; i < profile.FieldOrder.Count; i++)
        {
            var field = profile.FieldOrder[i];
            if (!values.TryGetValue(field, out var token))
            {
                break;
            }

            if (builder.Length > 0)
            {
                builder.Append(profile.PrimarySeparator);
            }

            builder.Append(token);
        }

        return builder.ToString();
    }

    public static string FormatRange(SlashLocaleDateParts from, SlashLocaleDateParts to, SlashLocaleInputProfile profile) =>
        FormatDate(from, profile) + SlashLocaleInputProfile.RangeSeparator + FormatDate(to, profile);

    public static bool TryToDayWire(SlashLocaleDateParts parts, out string wire)
    {
        wire = "";
        if (parts is not { HasDay: true, HasMonth: true, HasYear: true })
        {
            return false;
        }

        wire = string.Create(CultureInfo.InvariantCulture, $"{parts.Year:0000}-{parts.Month:00}-{parts.Day:00}");
        return true;
    }

    public static bool TryToMonthWire(SlashLocaleDateParts parts, out string wire)
    {
        wire = "";
        if (parts is not { HasMonth: true, HasYear: true })
        {
            return false;
        }

        wire = string.Create(CultureInfo.InvariantCulture, $"{parts.Year:0000}-{parts.Month:00}");
        return true;
    }

    public static bool TryToRangeWire(SlashLocaleDateParts parts, out string wire)
    {
        wire = "";
        if (parts.RangeEnd is null
            || !TryToDayWire(parts, out var from)
            || !TryToDayWire(parts.RangeEnd, out var to))
        {
            return false;
        }

        wire = $"{from}..{to}";
        return true;
    }

    static bool TrySplitRange(string text, out string left, out string right)
    {
        left = "";
        right = "";
        var index = text.IndexOf(SlashLocaleInputProfile.RangeSeparator, StringComparison.Ordinal);
        if (index < 0)
        {
            return false;
        }

        left = text[..index].Trim();
        right = text[(index + SlashLocaleInputProfile.RangeSeparator.Length)..].Trim();
        return left.Length > 0 && right.Length > 0;
    }

    static List<string> Tokenize(string text, IReadOnlyList<char> separators)
    {
        var tokens = new List<string>();
        var current = new StringBuilder();
        foreach (var ch in text)
        {
            if (separators.Contains(ch))
            {
                if (current.Length > 0)
                {
                    tokens.Add(current.ToString());
                    current.Clear();
                }

                continue;
            }

            if (char.IsDigit(ch))
            {
                current.Append(ch);
            }
        }

        if (current.Length > 0)
        {
            tokens.Add(current.ToString());
        }

        return tokens;
    }
}
