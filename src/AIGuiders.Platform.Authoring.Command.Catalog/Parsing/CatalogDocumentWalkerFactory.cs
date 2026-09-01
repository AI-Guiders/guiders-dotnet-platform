using AIGuiders.Platform.Authoring.Command.Catalog.Parsing.Sections;
using AIGuiders.Platform.Authoring.Core;

namespace AIGuiders.Platform.Authoring.Command.Catalog.Parsing;

public static class CatalogDocumentWalkerFactory
{
    static readonly AuthoringDocumentWalker<CatalogParseContext> Instance = Create();

    public static AuthoringDocumentWalker<CatalogParseContext> Shared => Instance;

    static AuthoringDocumentWalker<CatalogParseContext> Create()
    {
        var registry = new SectionHandlerRegistry<CatalogParseContext>(
        [
            new DefaultsSectionHandler(),
            new ChannelsSectionHandler(),
            new VariablesSectionHandler(),
            new HelpsSectionHandler(),
            new PhrasesSectionHandler(),
            new CommandsSectionHandler(),
            new BindingsSectionHandler(),
            new MelodiesSectionHandler(),
            new McpSectionHandler(),
            new ExecutorsSectionHandler(),
        ]);

        return new AuthoringDocumentWalker<CatalogParseContext>(
            registry,
            static ctx => ctx.Diagnostics,
            static (line, ctx) =>
            {
                if (line.Text.StartsWith("catalog ", StringComparison.Ordinal))
                {
                    ctx.Planet = line.Text["catalog ".Length..].Trim();
                    return true;
                }

                if (line.Text.StartsWith("import ", StringComparison.Ordinal))
                {
                    ctx.Imports.Add(line.Text["import ".Length..].Trim().Trim('<', '>'));
                    return true;
                }

                return false;
            },
            static (ctx, opener, body) => ctx.Diagnostics.Add(new(
                AuthoringDiagnosticCode.UnknownSection,
                $"Unknown section `{opener.Keyword}`.",
                body.Count > 0 ? body[0].LineNumber : 1,
                Section: opener.Keyword)));
    }
}
