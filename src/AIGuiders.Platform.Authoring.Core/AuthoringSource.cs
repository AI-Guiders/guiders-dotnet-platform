using System.Text;

namespace AIGuiders.Platform.Authoring.Core;

public static class AuthoringSource
{
    public static IReadOnlyList<AuthoringLine> FromText(string text) =>
        FromRawLines(text.Replace("\r\n", "\n").Split('\n'));

    public static IReadOnlyList<AuthoringLine> FromFile(string path)
    {
        var text = File.ReadAllText(path, Encoding.UTF8);
        return FromText(text);
    }

    public static IReadOnlyList<AuthoringLine> FromRawLines(IReadOnlyList<string> rawLines) =>
        rawLines
            .Select((text, index) => new AuthoringLine(index + 1, StripComment(text)))
            .ToList();

    public static string StripComment(string line)
    {
        var hash = line.IndexOf('#');
        return hash >= 0 ? line[..hash].TrimEnd() : line;
    }
}
