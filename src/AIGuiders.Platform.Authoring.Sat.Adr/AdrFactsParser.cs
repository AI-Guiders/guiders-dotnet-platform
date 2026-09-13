using System.Text.RegularExpressions;

namespace AIGuiders.Platform.Authoring.Sat;

public static partial class AdrFactsParser
{
    private static readonly Regex FactsFenceRegex = new(
        @"```text\s*\r?\nfacts:\s*\r?\n(?<body>.*?)\r?\nend facts\s*\r?\n```",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

    private static readonly Regex InlineFactsRegex = new(
        @"\bfacts:\s*\r?\n(?<body>.*?)\r?\nend facts\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

    private static readonly Regex HoareLineRegex = new(
        @"^\s*-\s*(?<id>[A-Za-z0-9_-]+)\s*:\s*\{\s*(?<pre>.*?)\s*\}\s*(?<transform>.*?)\s*\{\s*(?<post>.*?)\s*\}\s*$",
        RegexOptions.Compiled);

    private static readonly Regex GoldenInlineRegex = new(
        @"\bgolden:\s*(?<ids>[A-Za-z0-9_,\s*-]+)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex GoldenListItemRegex = new(
        @"^\s*-\s*(?<id>[A-Za-z0-9_-]+)\s*:",
        RegexOptions.Compiled);

    private static readonly Regex WfListItemRegex = new(
        @"^\s*-\s*(?<id>[A-Za-z0-9_-]+)\s*:",
        RegexOptions.Compiled);

    private static readonly Regex VerifiedByRowRegex = new(
        @"^\s*\|\s*(?<subject>[^|]+?)\s*\|\s*(?<evidence>[^|]+?)\s*\|\s*$",
        RegexOptions.Compiled);

    public static bool ContainsFactsBlock(string markdown) =>
        !string.IsNullOrWhiteSpace(markdown)
        && (markdown.Contains("facts:", StringComparison.OrdinalIgnoreCase)
            && markdown.Contains("end facts", StringComparison.OrdinalIgnoreCase));

    public static AdrFactsBlock? TryParse(string sourcePath, string markdown)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);
        ArgumentNullException.ThrowIfNull(markdown);

        if (!TryExtractFactsBody(markdown, out var body))
        {
            return null;
        }

        var goldenIds = new List<string>();
        var hoare = new List<HoareObligation>();
        var wfIds = new List<string>();
        var verifiedBy = new List<AdrVerifiedByRow>();

        var section = FactsSection.None;
        foreach (var rawLine in body.Split('\n'))
        {
            var line = rawLine.TrimEnd('\r');
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (TryEnterSection(line, goldenIds, ref section))
            {
                continue;
            }

            switch (section)
            {
                case FactsSection.Golden:
                    AddGoldenFromLine(line, goldenIds);
                    break;
                case FactsSection.Hoare:
                    AddHoareFromLine(line, hoare);
                    break;
                case FactsSection.WellFormedness:
                    AddWfFromLine(line, wfIds);
                    break;
                case FactsSection.Table:
                    AddVerifiedByRow(line, verifiedBy, goldenIds);
                    break;
            }
        }

        return new AdrFactsBlock
        {
            SourcePath = sourcePath,
            AdrId = TryExtractAdrId(sourcePath, markdown),
            GoldenIds = goldenIds,
            HoareObligations = hoare,
            WellFormednessIds = wfIds,
            VerifiedBy = verifiedBy,
        };
    }

    private static bool TryExtractFactsBody(string markdown, out string body)
    {
        var fenceMatch = FactsFenceRegex.Match(markdown);
        if (fenceMatch.Success)
        {
            body = fenceMatch.Groups["body"].Value;
            return true;
        }

        var inlineMatch = InlineFactsRegex.Match(markdown);
        if (inlineMatch.Success)
        {
            body = inlineMatch.Groups["body"].Value;
            return true;
        }

        body = string.Empty;
        return false;
    }

    private static bool TryEnterSection(string line, List<string> goldenIds, ref FactsSection section)
    {
        if (line.Equals("golden:", StringComparison.OrdinalIgnoreCase)
            || line.StartsWith("golden:", StringComparison.OrdinalIgnoreCase))
        {
            section = FactsSection.Golden;
            AddGoldenInline(line, goldenIds);
            return true;
        }

        if (line.Equals("hoare:", StringComparison.OrdinalIgnoreCase))
        {
            section = FactsSection.Hoare;
            return true;
        }

        if (line.Equals("wf:", StringComparison.OrdinalIgnoreCase))
        {
            section = FactsSection.WellFormedness;
            return true;
        }

        if (line.StartsWith("facts table", StringComparison.OrdinalIgnoreCase))
        {
            section = FactsSection.Table;
            return true;
        }

        return false;
    }

    private static void AddGoldenFromLine(string line, List<string> goldenIds)
    {
        var listMatch = GoldenListItemRegex.Match(line);
        if (listMatch.Success)
        {
            AddGoldenId(goldenIds, listMatch.Groups["id"].Value);
            return;
        }

        AddGoldenInline(line, goldenIds);
    }

    private static void AddGoldenInline(string line, List<string> goldenIds)
    {
        var inlineMatch = GoldenInlineRegex.Match(line);
        if (!inlineMatch.Success)
        {
            return;
        }

        foreach (var token in inlineMatch.Groups["ids"].Value.Split([',', ' '], StringSplitOptions.RemoveEmptyEntries))
        {
            var id = token.Trim();
            if (id is "*" or "..*")
            {
                continue;
            }

            AddGoldenId(goldenIds, id);
        }
    }

    private static void AddHoareFromLine(string line, List<HoareObligation> hoare)
    {
        var match = HoareLineRegex.Match(line);
        if (!match.Success)
        {
            return;
        }

        hoare.Add(new HoareObligation(
            match.Groups["id"].Value,
            match.Groups["pre"].Value.Trim(),
            match.Groups["transform"].Value.Trim(),
            match.Groups["post"].Value.Trim(),
            line.Trim()));
    }

    private static void AddWfFromLine(string line, List<string> wfIds)
    {
        var match = WfListItemRegex.Match(line);
        if (!match.Success)
        {
            return;
        }

        var id = match.Groups["id"].Value;
        if (!wfIds.Contains(id, StringComparer.OrdinalIgnoreCase))
        {
            wfIds.Add(id);
        }
    }

    private static void AddVerifiedByRow(string line, List<AdrVerifiedByRow> verifiedBy, List<string> goldenIds)
    {
        if (line.Contains("---", StringComparison.Ordinal)
            || line.Contains("contract", StringComparison.OrdinalIgnoreCase)
            || line.Contains("verified_by", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var match = VerifiedByRowRegex.Match(line);
        if (!match.Success)
        {
            return;
        }

        var subject = match.Groups["subject"].Value.Trim();
        var evidence = match.Groups["evidence"].Value.Trim();
        verifiedBy.Add(new AdrVerifiedByRow(subject, evidence));
        AddGoldenInline(evidence, goldenIds);
    }

    private static void AddGoldenId(List<string> goldenIds, string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return;
        }

        if (!goldenIds.Contains(id, StringComparer.OrdinalIgnoreCase))
        {
            goldenIds.Add(id);
        }
    }

    private static string? TryExtractAdrId(string sourcePath, string markdown)
    {
        var fileName = Path.GetFileNameWithoutExtension(sourcePath);
        if (!string.IsNullOrWhiteSpace(fileName))
        {
            var fileMatch = AdrIdRegex().Match(fileName);
            if (fileMatch.Success)
            {
                return fileMatch.Value.ToUpperInvariant();
            }
        }

        var headingMatch = AdrHeadingRegex().Match(markdown);
        return headingMatch.Success ? headingMatch.Groups["id"].Value.ToUpperInvariant() : null;
    }

    [GeneratedRegex(@"GUIDERS-[A-Z0-9]+-ADR-\d{4}", RegexOptions.IgnoreCase)]
    private static partial Regex AdrIdRegex();

    [GeneratedRegex(@"^#\s+(?<id>GUIDERS-[A-Z0-9]+-ADR-\d{4})\b", RegexOptions.IgnoreCase)]
    private static partial Regex AdrHeadingRegex();

    private enum FactsSection
    {
        None,
        Golden,
        Hoare,
        WellFormedness,
        Table,
    }
}
