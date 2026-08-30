namespace AIGuiders.Platform.Notations;

public static class InvocationNotation
{
    public static NormalizedCommandLine FromPathSegments(IReadOnlyList<string> segments)
    {
        var list = segments.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
        return new NormalizedCommandLine(string.Join(' ', list), list);
    }

    public static bool PathsEqual(NormalizedCommandLine a, NormalizedCommandLine b) =>
        string.Equals(a.CanonicalPath, b.CanonicalPath, StringComparison.OrdinalIgnoreCase);
}
