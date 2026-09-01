namespace AIGuiders.Platform.Authoring.Core;

public static class AuthoringImportGraph
{
    public static IReadOnlyList<AuthoringImportDirective> ScanText(string text) =>
        ScanLines(text.Replace("\r\n", "\n").Split('\n'));

    public static IReadOnlyList<AuthoringImportDirective> ScanLines(IEnumerable<string> lines)
    {
        var directives = new List<AuthoringImportDirective>();
        foreach (var line in lines)
        {
            if (AuthoringImportLine.TryParse(line, out var directive) && directive is not null)
            {
                directives.Add(directive);
            }
        }

        return directives;
    }
}
