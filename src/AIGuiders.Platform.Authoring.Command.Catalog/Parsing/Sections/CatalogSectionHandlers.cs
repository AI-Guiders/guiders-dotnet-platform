using AIGuiders.Platform.Authoring.Core;

namespace AIGuiders.Platform.Authoring.Command.Catalog.Parsing.Sections;

public sealed class DefaultsSectionHandler : IAuthoringSectionHandler<CatalogParseContext>
{
    public string Keyword => "defaults";

    public void Apply(CatalogParseContext context, AuthoringSectionBlock block) =>
        KvSurface.MergeInto(context.DefaultsKv, block.Body);
}

public sealed class ExecutorsSectionHandler : IAuthoringSectionHandler<CatalogParseContext>
{
    public string Keyword => "executors";

    public void Apply(CatalogParseContext context, AuthoringSectionBlock block) =>
        KvSurface.MergeInto(context.Executors, block.Body);
}

public sealed class VariablesSectionHandler : IAuthoringSectionHandler<CatalogParseContext>
{
    public string Keyword => "variables";

    public void Apply(CatalogParseContext context, AuthoringSectionBlock block)
    {
        if (block.SurfaceKind == AuthoringSurfaceKind.Table)
        {
            foreach (var map in TableSurface.ParseMaps(block.Body))
            {
                context.Variables.Add(new(
                    map.GetValueOrDefault("name") ?? "",
                    TableSurface.NullIfEmpty(map.GetValueOrDefault("kind"))));
            }

            return;
        }

        foreach (var entry in KvSurface.ParseNameOrPair(block.Body))
        {
            context.Variables.Add(new(
                entry.Name,
                entry.Value ?? context.DefaultsKv.GetValueOrDefault("variable.kind")));
        }
    }
}

public sealed class HelpsSectionHandler : IAuthoringSectionHandler<CatalogParseContext>
{
    public string Keyword => "helps";

    public void Apply(CatalogParseContext context, AuthoringSectionBlock block)
    {
        if (block.SurfaceKind != AuthoringSurfaceKind.Table)
        {
            return;
        }

        foreach (var map in TableSurface.ParseMaps(block.Body))
        {
            context.Helps.Add(new(
                map.GetValueOrDefault("target") ?? "",
                map.GetValueOrDefault("field") ?? "",
                map.GetValueOrDefault("text") ?? ""));
        }
    }
}

public sealed class PhrasesSectionHandler : IAuthoringSectionHandler<CatalogParseContext>
{
    public string Keyword => "phrases";

    public void Apply(CatalogParseContext context, AuthoringSectionBlock block)
    {
        if (block.SurfaceKind != AuthoringSurfaceKind.Table)
        {
            return;
        }

        foreach (var map in TableSurface.ParseMaps(block.Body))
        {
            context.Phrases.Add(new(
                map.GetValueOrDefault("name") ?? "",
                map.GetValueOrDefault("phrase") ?? ""));
        }
    }
}

public sealed class CommandsSectionHandler : IAuthoringSectionHandler<CatalogParseContext>
{
    public string Keyword => "commands";

    public void Apply(CatalogParseContext context, AuthoringSectionBlock block)
    {
        foreach (var map in TableSurface.ParseMaps(block.Body))
        {
            var command = map.GetValueOrDefault("command") ?? "";
            context.Commands.Add(new CatalogCommandRow { Command = command, Columns = map });
        }
    }
}

public sealed class BindingsSectionHandler : IAuthoringSectionHandler<CatalogParseContext>
{
    public string Keyword => "bindings";

    public void Apply(CatalogParseContext context, AuthoringSectionBlock block)
    {
        foreach (var map in TableSurface.ParseMaps(block.Body))
        {
            context.Bindings.Add(new(
                map.GetValueOrDefault("gesture") ?? "",
                map.GetValueOrDefault("command") ?? "",
                TableSurface.NullIfEmpty(map.GetValueOrDefault("role"))));
        }
    }
}

public sealed class MelodiesSectionHandler : IAuthoringSectionHandler<CatalogParseContext>
{
    public string Keyword => "melodies";

    public void Apply(CatalogParseContext context, AuthoringSectionBlock block)
    {
        foreach (var map in TableSurface.ParseMaps(block.Body))
        {
            context.Melodies.Add(new(
                map.GetValueOrDefault("slug") ?? "",
                map.GetValueOrDefault("command") ?? ""));
        }
    }
}

public sealed class McpSectionHandler : IAuthoringSectionHandler<CatalogParseContext>
{
    public string Keyword => "mcp";

    public void Apply(CatalogParseContext context, AuthoringSectionBlock block)
    {
        foreach (var map in TableSurface.ParseMaps(block.Body))
        {
            context.Mcp.Add(new(
                map.GetValueOrDefault("command") ?? "",
                map.GetValueOrDefault("expose") ?? "yes"));
        }
    }
}
