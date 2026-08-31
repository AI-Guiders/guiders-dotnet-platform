#nullable enable

using System.Globalization;
using System.Text;

namespace AIGuiders.Platform.CommandPlane;

public enum LocaleDateField
{
    Day = 0,
    Month = 1,
    Year = 2,
}

/// <summary>Locale input profile derived from ambient culture — not imposed by Platform.</summary>
public sealed class LocaleInputProfile
{
    public const string RangeSeparator = " .. ";

    public CultureInfo Culture { get; }
    public char PrimarySeparator { get; }
    public IReadOnlyList<char> Separators { get; }
    public IReadOnlyList<LocaleDateField> FieldOrder { get; }
    public string ShortDatePattern { get; }

    LocaleInputProfile(
        CultureInfo culture,
        char primarySeparator,
        IReadOnlyList<char> separators,
        IReadOnlyList<LocaleDateField> fieldOrder,
        string shortDatePattern)
    {
        Culture = culture;
        PrimarySeparator = primarySeparator;
        Separators = separators;
        FieldOrder = fieldOrder;
        ShortDatePattern = shortDatePattern;
    }

    public static LocaleInputProfile FromCulture(CultureInfo culture)
    {
        var format = culture.DateTimeFormat;
        var pattern = format.ShortDatePattern;
        var fieldOrder = ParseFieldOrder(pattern);
        var separators = CollectSeparators(pattern, format.DateSeparator);
        var primary = separators.Count > 0 ? separators[0] : '/';
        return new LocaleInputProfile(culture, primary, separators, fieldOrder, pattern);
    }

    public static LocaleInputProfile FromCulture(ICultureAmbient ambient) =>
        FromCulture(ambient.Culture);

    public string InputPlaceholder =>
        ShortDatePattern.Replace("yyyy", "YYYY", StringComparison.Ordinal)
            .Replace("yy", "YY", StringComparison.Ordinal);

    static IReadOnlyList<LocaleDateField> ParseFieldOrder(string pattern)
    {
        var fields = new List<LocaleDateField>();
        for (var i = 0; i < pattern.Length; i++)
        {
            var ch = pattern[i];
            if (ch is 'd' or 'D')
            {
                if (i + 1 < pattern.Length && pattern[i + 1] is 'd' or 'D')
                {
                    i++;
                }

                fields.Add(LocaleDateField.Day);
            }
            else if (ch is 'M')
            {
                if (i + 1 < pattern.Length && pattern[i + 1] is 'M')
                {
                    i++;
                }

                fields.Add(LocaleDateField.Month);
            }
            else if (ch is 'y' or 'Y')
            {
                if (i + 1 < pattern.Length && pattern[i + 1] is 'y' or 'Y')
                {
                    i++;
                }

                fields.Add(LocaleDateField.Year);
            }
        }

        if (fields.Count == 0)
        {
            return [LocaleDateField.Day, LocaleDateField.Month, LocaleDateField.Year];
        }

        return fields;
    }

    static IReadOnlyList<char> CollectSeparators(string pattern, string cultureSeparator)
    {
        var set = new HashSet<char>();
        foreach (var ch in pattern)
        {
            if (!char.IsLetter(ch))
            {
                set.Add(ch);
            }
        }

        foreach (var ch in cultureSeparator)
        {
            set.Add(ch);
        }

        set.Add('.');
        set.Add('/');
        set.Add('-');
        return set.OrderBy(ch => ch).ToArray();
    }
}

public enum LocaleDateCompleteness
{
    Empty,
    Partial,
    MonthYear,
    CompleteDate,
    CompleteRange,
}

public sealed class LocaleDateParts
{
    public int? Day { get; init; }
    public int? Month { get; init; }
    public int? Year { get; init; }
    public LocaleDateParts? RangeEnd { get; init; }

    public bool HasDay => Day is > 0;
    public bool HasMonth => Month is > 0;
    public bool HasYear => Year is > 0;

    public IReadOnlyDictionary<string, string> ToWireSegments()
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (Year is > 0)
        {
            map["year"] = Year.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Month is > 0)
        {
            map["month"] = Month.Value.ToString("00", CultureInfo.InvariantCulture);
        }

        if (Day is > 0)
        {
            map["day"] = Day.Value.ToString("00", CultureInfo.InvariantCulture);
        }

        return map;
    }
}
