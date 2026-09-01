using System.Text.RegularExpressions;

namespace AIGuiders.Platform.Authoring.Core;

/// <summary>Strip nested <c>end keyword</c> closers before indented-tree parse (e.g. <c>end grammar</c>).</summary>
public static partial class InnerBlockFilter
{
    public static IReadOnlyList<AuthoringLine> StripEndMarkers(IEnumerable<AuthoringLine> lines) =>
        lines.Where(static line => !EndMarker().IsMatch(line.Text)).ToList();

    [GeneratedRegex(@"^\s*end\s+\w+\s*$", RegexOptions.CultureInvariant)]
    private static partial Regex EndMarker();
}
