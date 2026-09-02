#nullable enable

using System.Globalization;
using System.Text;

namespace AIGuiders.Platform.Execution.CommandPlane;

public static class LocaleDateParser
{
    public static bool TryParse(
        string input,
        LocaleInputProfile profile,
        out LocaleDateParts parts,
        out LocaleDateCompleteness completeness)
    {
        parts = new LocaleDateParts();
        completeness = LocaleDateCompleteness.Empty;
        var text = input.Trim();
        if (text.Length == 0)
        {
            return false;
        }

        if (TrySplitRange(text, out var left, out var right))
        {
            if (!TryParseSingle(left, profile, out var fromParts, out var fromComplete)
                || !TryParseSingle(right, profile, out var toParts, out var toComplete)
                || fromComplete != LocaleDateCompleteness.CompleteDate
                || toComplete != LocaleDateCompleteness.CompleteDate)
            {
                return false;
            }

            parts = new LocaleDateParts
            {
                Day = fromParts.Day,
                Month = fromParts.Month,
                Year = fromParts.Year,
                RangeEnd = toParts,
            };
            completeness = LocaleDateCompleteness.CompleteRange;
            return true;
        }

        return TryParseSingle(text, profile, out parts, out completeness);
    }

    public static bool TryParseSingle(
        string input,
        LocaleInputProfile profile,
        out LocaleDateParts parts,
        out LocaleDateCompleteness completeness)
    {
        parts = new LocaleDateParts();
        completeness = LocaleDateCompleteness.Empty;
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
            completeness = LocaleDateCompleteness.MonthYear;
            return true;
        }

        var values = new int?[3];
        for (var i = 0; i < tokens.Count && i < profile.FieldOrder.Count; i++)
        {
            if (!int.TryParse(tokens[i], NumberStyles.None, CultureInfo.InvariantCulture, out var number))
            {
                completeness = LocaleDateCompleteness.Partial;
                return true;
            }

            values[(int)profile.FieldOrder[i]] = number;
        }

        parts = new LocaleDateParts
        {
            Day = values[(int)LocaleDateField.Day],
            Month = values[(int)LocaleDateField.Month],
            Year = values[(int)LocaleDateField.Year],
        };

        if (parts is { HasDay: true, HasMonth: true, HasYear: true })
        {
            completeness = LocaleDateCompleteness.CompleteDate;
            return true;
        }

        if (parts is { HasMonth: true, HasYear: true } && !parts.HasDay)
        {
            completeness = LocaleDateCompleteness.MonthYear;
            return true;
        }

        completeness = LocaleDateCompleteness.Partial;
        return true;
    }

    static bool TryParseMonthYearTokens(IReadOnlyList<string> tokens, out LocaleDateParts parts)
    {
        parts = new LocaleDateParts();
        if (tokens.Count != 2)
        {
            return false;
        }

        if (tokens[1].Length == 4
            && int.TryParse(tokens[0], NumberStyles.None, CultureInfo.InvariantCulture, out var month)
            && int.TryParse(tokens[1], NumberStyles.None, CultureInfo.InvariantCulture, out var year)
            && month is >= 1 and <= 12)
        {
            parts = new LocaleDateParts { Month = month, Year = year };
            return true;
        }

        if (tokens[0].Length == 4
            && int.TryParse(tokens[0], NumberStyles.None, CultureInfo.InvariantCulture, out year)
            && int.TryParse(tokens[1], NumberStyles.None, CultureInfo.InvariantCulture, out month)
            && month is >= 1 and <= 12)
        {
            parts = new LocaleDateParts { Month = month, Year = year };
            return true;
        }

        return false;
    }

    public static string FormatDate(LocaleDateParts parts, LocaleInputProfile profile)
    {
        var values = new Dictionary<LocaleDateField, string>();
        if (parts.Day is > 0)
        {
            values[LocaleDateField.Day] = parts.Day.Value.ToString("00", profile.Culture);
        }

        if (parts.Month is > 0)
        {
            values[LocaleDateField.Month] = parts.Month.Value.ToString("00", profile.Culture);
        }

        if (parts.Year is > 0)
        {
            values[LocaleDateField.Year] = parts.Year.Value.ToString("0000", profile.Culture);
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

    public static string FormatRange(LocaleDateParts from, LocaleDateParts to, LocaleInputProfile profile) =>
        FormatDate(from, profile) + LocaleInputProfile.RangeSeparator + FormatDate(to, profile);

    public static bool TryToDayWire(LocaleDateParts parts, out string wire)
    {
        wire = "";
        if (parts is not { HasDay: true, HasMonth: true, HasYear: true })
        {
            return false;
        }

        wire = string.Create(CultureInfo.InvariantCulture, $"{parts.Year:0000}-{parts.Month:00}-{parts.Day:00}");
        return true;
    }

    public static bool TryToMonthWire(LocaleDateParts parts, out string wire)
    {
        wire = "";
        if (parts is not { HasMonth: true, HasYear: true })
        {
            return false;
        }

        wire = string.Create(CultureInfo.InvariantCulture, $"{parts.Year:0000}-{parts.Month:00}");
        return true;
    }

    public static bool TryToRangeWire(LocaleDateParts parts, out string wire)
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
        var index = text.IndexOf(LocaleInputProfile.RangeSeparator, StringComparison.Ordinal);
        if (index < 0)
        {
            return false;
        }

        left = text[..index].Trim();
        right = text[(index + LocaleInputProfile.RangeSeparator.Length)..].Trim();
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
