using System.Text;

namespace AIGuiders.Platform.Authoring.Command.Catalog;

public static class CatalogSummary
{
    public static string Format(CatalogDocument document)
    {
        var sb = new StringBuilder();
        sb.Append($"catalog {document.Planet}");
        if (!string.IsNullOrWhiteSpace(document.Defaults.NotationKeyboardBinding))
        {
            sb.Append($" | bindings: {document.Defaults.NotationKeyboardBinding}");
        }

        if (!string.IsNullOrWhiteSpace(document.Defaults.NotationKeyboardMelody))
        {
            sb.Append($" | melodies: {document.Defaults.NotationKeyboardMelody}");
        }

        foreach (var channel in document.Channels.Where(c => !string.IsNullOrWhiteSpace(c.CommandNotation)))
        {
            var tag = string.IsNullOrWhiteSpace(channel.Sub) ? channel.Surface : $"{channel.Surface}.{channel.Sub}";
            sb.Append($" | {tag}: {channel.CommandNotation}+{channel.ArgumentNotation}");
        }

        return sb.ToString();
    }
}

public static class CatalogDocumentExtensions
{
    public static string WireCommandId(this CatalogDocument document, string localCommand) =>
        $"{document.Planet}.{localCommand.Replace(' ', '.')}";

    public static IReadOnlyList<string> FederationSurfaces(this CatalogDocument document) =>
        document.Defaults.CommandSurfaces;
}
