using AIGuiders.Platform.Authoring.Command.Catalog.Parsing.Sections;
using AIGuiders.Platform.Authoring.Core;

namespace AIGuiders.Platform.Authoring.Command.Catalog.Parsing;

public static class CatalogSectionRegistry
{
    static readonly IReadOnlyDictionary<string, ICatalogSectionHandler> Handlers =
        new ICatalogSectionHandler[]
        {
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
        }.ToDictionary(static h => h.Keyword, StringComparer.OrdinalIgnoreCase);

    public static bool TryGet(string keyword, out ICatalogSectionHandler handler) =>
        Handlers.TryGetValue(keyword, out handler!);

    public static void Apply(
        CatalogParseContext context,
        AuthoringSectionOpener opener,
        IReadOnlyList<AuthoringLine> body)
    {
        var surfaceKind = BlockReader.ResolveSurfaceKind(opener);
        var block = new CatalogSectionBlock(opener.Keyword, surfaceKind, body);

        if (!TryGet(opener.Keyword, out var handler))
        {
            context.Diagnostics.Add(new(
                AuthoringDiagnosticCode.UnknownSection,
                $"Unknown section `{opener.Keyword}`.",
                body.Count > 0 ? body[0].LineNumber : 1,
                Section: opener.Keyword));
            return;
        }

        handler.Apply(context, block);
    }
}
