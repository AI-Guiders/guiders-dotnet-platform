using AIGuiders.Platform.Authoring.Command.Catalog.Parsing;
using AIGuiders.Platform.Authoring.Core;

namespace AIGuiders.Platform.Authoring.Command.Catalog;

public static class CatalogParser
{
    public static CatalogParseResult Parse(string text, string? sourcePath = null) =>
        ParseLines(AuthoringSource.FromText(text), sourcePath);

    public static CatalogParseResult ParseFile(string path) =>
        ParseLines(AuthoringSource.FromFile(path), path);

    internal static CatalogParseResult ParseLines(IReadOnlyList<AuthoringLine> lines, string? sourcePath)
    {
        var context = new CatalogParseContext();

        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line.Text))
            {
                continue;
            }

            if (TryApplyHeaderLine(line, context))
            {
                continue;
            }

            if (!BlockReader.TryParseOpener(line.Text, out var opener))
            {
                continue;
            }

            var block = BlockReader.Read(lines, i + 1, opener.Keyword, context.Diagnostics);
            i = block.EndLineIndex;
            if (!block.IsClosed)
            {
                continue;
            }

            CatalogSectionRegistry.Apply(context, opener, block.Body);
        }

        if (string.IsNullOrWhiteSpace(context.Planet))
        {
            context.Diagnostics.Add(new(AuthoringDiagnosticCode.MissingCatalogHeader, "Missing `catalog <planet>` header.", 1));
            return new() { Diagnostics = context.Diagnostics };
        }

        var document = context.BuildDocument();
        CatalogGrammarValidator.Validate(document, context.Diagnostics);
        context.ValidateChannels();

        return new() { Document = document, Diagnostics = context.Diagnostics };
    }

    static bool TryApplyHeaderLine(AuthoringLine line, CatalogParseContext context)
    {
        if (line.Text.StartsWith("catalog ", StringComparison.Ordinal))
        {
            context.Planet = line.Text["catalog ".Length..].Trim();
            return true;
        }

        if (line.Text.StartsWith("import ", StringComparison.Ordinal))
        {
            context.Imports.Add(line.Text["import ".Length..].Trim().Trim('<', '>'));
            return true;
        }

        return false;
    }
}
