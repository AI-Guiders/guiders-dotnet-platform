using AIGuiders.Platform.Authoring.Core;

namespace AIGuiders.Platform.Authoring.Command.Catalog.Parsing.Sections;

public sealed class ChannelsSectionHandler : IAuthoringSectionHandler<CatalogParseContext>
{
    public string Keyword => "channels";

    public void Apply(CatalogParseContext context, AuthoringSectionBlock block) =>
        context.Channels.AddRange(Parse(block.Body));

    public static IReadOnlyList<CatalogChannel> Parse(IReadOnlyList<AuthoringLine> body) =>
        body.Count > 0 && body[0].Text.TrimStart().StartsWith('|')
            ? ParseTable(body)
            : ParseTree(body);

    static IReadOnlyList<CatalogChannel> ParseTree(IReadOnlyList<AuthoringLine> body)
    {
        var filtered = InnerBlockFilter.StripEndMarkers(body);
        var treeLines = filtered.Select(static l => (l.LineNumber, l.Text));
        var list = new List<CatalogChannel>();
        foreach (var surfaceNode in IndentedTreeParser.Parse(treeLines))
        {
            var lineGrammar = ReadLineGrammar(surfaceNode);

            if (surfaceNode.Value is not null)
            {
                list.Add(new CatalogChannel
                {
                    Surface = surfaceNode.Key,
                    PlanetId = surfaceNode.Value,
                    CommandGrammar = lineGrammar.Command,
                    ArgumentGrammar = lineGrammar.Argument,
                });
                continue;
            }

            foreach (var child in surfaceNode.Children)
            {
                if (child.Key.Equals("grammar", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                list.Add(new CatalogChannel
                {
                    Surface = surfaceNode.Key,
                    Sub = child.Key,
                    PlanetId = child.Value,
                    CommandGrammar = lineGrammar.Command,
                    ArgumentGrammar = lineGrammar.Argument,
                });
            }
        }

        return list;
    }

    static (string? Command, string? Argument) ReadLineGrammar(IndentedNode surfaceNode)
    {
        var grammarNode = surfaceNode.Children.FirstOrDefault(static c =>
            c.Key.Equals("grammar", StringComparison.OrdinalIgnoreCase) && c.Value is null);

        if (grammarNode is null)
        {
            return (null, null);
        }

        string? command = null;
        string? argument = null;
        foreach (var child in grammarNode.Children)
        {
            if (child.Key.Equals("command", StringComparison.OrdinalIgnoreCase))
            {
                command = child.Value;
            }
            else if (child.Key.Equals("argument", StringComparison.OrdinalIgnoreCase))
            {
                argument = child.Value;
            }
        }

        return (command, argument);
    }

    static IReadOnlyList<CatalogChannel> ParseTable(IReadOnlyList<AuthoringLine> body)
    {
        var list = new List<CatalogChannel>();
        foreach (var map in TableSurface.ParseMaps(body))
        {
            list.Add(new CatalogChannel
            {
                Surface = map.GetValueOrDefault("surface") ?? "",
                Sub = TableSurface.NullIfEmpty(map.GetValueOrDefault("sub")),
                PlanetId = TableSurface.NullIfEmpty(map.GetValueOrDefault("planet-id")),
                CommandGrammar = TableSurface.NullIfEmpty(map.GetValueOrDefault("grammar.command")),
                ArgumentGrammar = TableSurface.NullIfEmpty(map.GetValueOrDefault("grammar.argument")),
            });
        }

        return list;
    }
}
