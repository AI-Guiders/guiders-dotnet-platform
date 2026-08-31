#nullable enable

namespace AIGuiders.Platform.LanguageIntelligence.Markup;

/// <summary>Bundled markdown insert SSOT (FORGE-ADR-0062 / 0063).</summary>
public static class MarkdownTextDialectCatalog
{
    public static IReadOnlyList<TextInsertFormatDefinition> InsertFormats { get; } = new[]
    {
        new TextInsertFormatDefinition("h1", "/h1", "Heading 1", Insert: "# ", HtmlWrapOpen: "<h1>", HtmlWrapClose: "</h1>"),
        new TextInsertFormatDefinition("h2", "/h2", "Heading 2", Insert: "## ", HtmlWrapOpen: "<h2>", HtmlWrapClose: "</h2>"),
        new TextInsertFormatDefinition("h3", "/h3", "Heading 3", Insert: "### ", HtmlWrapOpen: "<h3>", HtmlWrapClose: "</h3>"),
        new TextInsertFormatDefinition("bul", "/bul", "Bullet list", Insert: "- ", HtmlInsert: "<ul><li></li></ul>"),
        new TextInsertFormatDefinition("num", "/num", "Numbered list", Insert: "1. ", HtmlInsert: "<ol><li></li></ol>"),
        new TextInsertFormatDefinition("link", "/link", "Link", WrapOpen: "[", WrapClose: "](url)"),
        new TextInsertFormatDefinition("bold", "/bold", "Bold selection", WrapOpen: "**", WrapClose: "**", HtmlWrapOpen: "<strong>", HtmlWrapClose: "</strong>"),
        new TextInsertFormatDefinition("italic", "/italic", "Italic selection", WrapOpen: "*", WrapClose: "*", HtmlWrapOpen: "<em>", HtmlWrapClose: "</em>"),
        new TextInsertFormatDefinition("code", "/code", "Inline code", WrapOpen: "`", WrapClose: "`", HtmlWrapOpen: "<code>", HtmlWrapClose: "</code>"),
        new TextInsertFormatDefinition("quote", "/quote", "Block quote", Insert: "> ", HtmlWrapOpen: "<blockquote>", HtmlWrapClose: "</blockquote>"),
    };

    public static TextDialectDefinition Markdown { get; } = new(
        "markdown",
        "Markdown",
        InsertFormats,
        Default: true,
        ModeCommandPath: "/editor text markdown");

    public static TextInsertFormatDefinition? TryGetFormat(string id)
    {
        foreach (var f in InsertFormats)
        {
            if (string.Equals(f.Id, id, StringComparison.OrdinalIgnoreCase))
                return f;
        }

        return null;
    }
}
