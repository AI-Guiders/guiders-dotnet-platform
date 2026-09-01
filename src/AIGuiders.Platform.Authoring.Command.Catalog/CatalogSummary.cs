using System.Text;

namespace AIGuiders.Platform.Authoring.Command.Catalog;

public static class CatalogSummary
{
    public static string Format(CatalogDocument document)
    {
        var sb = new StringBuilder();
        sb.Append($"catalog {document.Planet}");
        if (!string.IsNullOrWhiteSpace(document.Defaults.GrammarKeyboardBinding))
        {
            sb.Append($" | bindings: {document.Defaults.GrammarKeyboardBinding}");
        }

        if (!string.IsNullOrWhiteSpace(document.Defaults.GrammarKeyboardMelody))
        {
            sb.Append($" | melodies: {document.Defaults.GrammarKeyboardMelody}");
        }

        foreach (var channel in document.Channels.Where(c => !string.IsNullOrWhiteSpace(c.CommandGrammar)))
        {
            var tag = string.IsNullOrWhiteSpace(channel.Sub) ? channel.Surface : $"{channel.Surface}.{channel.Sub}";
            sb.Append($" | {tag}: grammar({channel.CommandGrammar}+{channel.ArgumentGrammar})");
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
