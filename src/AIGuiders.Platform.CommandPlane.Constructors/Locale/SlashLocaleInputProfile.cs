#nullable enable

using System.Globalization;
using System.Text;

namespace AIGuiders.Platform.CommandPlane;

public enum SlashLocaleDateField
{
    Day = 0,
    Month = 1,
    Year = 2,
}

/// <summary>Locale input profile derived from ambient culture — not imposed by Platform.</summary>
public sealed class SlashLocaleInputProfile
{
    public const string RangeSeparator = " .. ";

    public CultureInfo Culture { get; }
    public char PrimarySeparator { get; }
    public IReadOnlyList<char> Separators { get; }
    public IReadOnlyList<SlashLocaleDateField> FieldOrder { get; }
    public string ShortDatePattern { get; }

    SlashLocaleInputProfile(
        CultureInfo culture,
        char primarySeparator,
        IReadOnlyList<char> separators,
        IReadOnlyList<SlashLocaleDateField> fieldOrder,
        string shortDatePattern)
    {
        Culture = culture;
        PrimarySeparator = primarySeparator;
        Separators = separators;
        FieldOrder = fieldOrder;
        ShortDatePattern = shortDatePattern;
    }

    public static SlashLocaleInputProfile FromCulture(CultureInfo culture)
    {
        var format = culture.DateTimeFormat;
        var pattern = format.ShortDatePattern;
        var fieldOrder = ParseFieldOrder(pattern);
        var separators = CollectSeparators(pattern, format.DateSeparator);
        var primary = separators.Count > 0 ? separators[0] : '/';
        return new SlashLocaleInputProfile(culture, primary, separators, fieldOrder, pattern);
    }

    public static SlashLocaleInputProfile FromCulture(ISlashCultureAmbient ambient) =>
        FromCulture(ambient.Culture);

    public string InputPlaceholder =>
        ShortDatePattern.Replace("yyyy", "YYYY", StringComparison.Ordinal)
            .Replace("yy", "YY", StringComparison.Ordinal);

    static IReadOnlyList<SlashLocaleDateField> ParseFieldOrder(string pattern)
    {
        var fields = new List<SlashLocaleDateField>();
        for (var i = 0; i < pattern.Length; i++)
        {
            var ch = pattern[i];
            if (ch is 'd' or 'D')
            {
                if (i + 1 < pattern.Length && pattern[i + 1] is 'd' or 'D')
                {
                    i++;
                }

                fields.Add(SlashLocaleDateField.Day);
            }
            else if (ch is 'M')
            {
                if (i + 1 < pattern.Length && pattern[i + 1] is 'M')
                {
                    i++;
                }

                fields.Add(SlashLocaleDateField.Month);
            }
            else if (ch is 'y' or 'Y')
            {
                if (i + 1 < pattern.Length && pattern[i + 1] is 'y' or 'Y')
                {
                    i++;
                }

                fields.Add(SlashLocaleDateField.Year);
            }
        }

        if (fields.Count == 0)
        {
            return [SlashLocaleDateField.Day, SlashLocaleDateField.Month, SlashLocaleDateField.Year];
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

public enum SlashLocaleDateCompleteness
{
    Empty,
    Partial,
    MonthYear,
    CompleteDate,
    CompleteRange,
}

public sealed class SlashLocaleDateParts
{
    public int? Day { get; init; }
    public int? Month { get; init; }
    public int? Year { get; init; }
    public SlashLocaleDateParts? RangeEnd { get; init; }

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
