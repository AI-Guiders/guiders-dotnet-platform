using AIGuiders.Platform.Authoring.Core;

namespace AIGuiders.Platform.Authoring.Command.Catalog.Parsing.Sections;

public sealed class DefaultsSectionHandler : ICatalogSectionHandler
{
    public string Keyword => "defaults";

    public void Apply(CatalogParseContext context, CatalogSectionBlock block) =>
        KvSurface.MergeInto(context.DefaultsKv, block.Body);
}

public sealed class ExecutorsSectionHandler : ICatalogSectionHandler
{
    public string Keyword => "executors";

    public void Apply(CatalogParseContext context, CatalogSectionBlock block) =>
        KvSurface.MergeInto(context.Executors, block.Body);
}

public sealed class VariablesSectionHandler : ICatalogSectionHandler
{
    public string Keyword => "variables";

    public void Apply(CatalogParseContext context, CatalogSectionBlock block)
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

public sealed class HelpsSectionHandler : ICatalogSectionHandler
{
    public string Keyword => "helps";

    public void Apply(CatalogParseContext context, CatalogSectionBlock block)
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

public sealed class PhrasesSectionHandler : ICatalogSectionHandler
{
    public string Keyword => "phrases";

    public void Apply(CatalogParseContext context, CatalogSectionBlock block)
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

public sealed class CommandsSectionHandler : ICatalogSectionHandler
{
    public string Keyword => "commands";

    public void Apply(CatalogParseContext context, CatalogSectionBlock block)
    {
        foreach (var map in TableSurface.ParseMaps(block.Body))
        {
            var command = map.GetValueOrDefault("command") ?? "";
            context.Commands.Add(new CatalogCommandRow { Command = command, Columns = map });
        }
    }
}

public sealed class BindingsSectionHandler : ICatalogSectionHandler
{
    public string Keyword => "bindings";

    public void Apply(CatalogParseContext context, CatalogSectionBlock block)
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

public sealed class MelodiesSectionHandler : ICatalogSectionHandler
{
    public string Keyword => "melodies";

    public void Apply(CatalogParseContext context, CatalogSectionBlock block)
    {
        foreach (var map in TableSurface.ParseMaps(block.Body))
        {
            context.Melodies.Add(new(
                map.GetValueOrDefault("slug") ?? "",
                map.GetValueOrDefault("command") ?? ""));
        }
    }
}

public sealed class McpSectionHandler : ICatalogSectionHandler
{
    public string Keyword => "mcp";

    public void Apply(CatalogParseContext context, CatalogSectionBlock block)
    {
        foreach (var map in TableSurface.ParseMaps(block.Body))
        {
            context.Mcp.Add(new(
                map.GetValueOrDefault("command") ?? "",
                map.GetValueOrDefault("expose") ?? "yes"));
        }
    }
}
