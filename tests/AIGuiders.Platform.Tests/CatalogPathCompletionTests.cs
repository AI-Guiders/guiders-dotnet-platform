#nullable enable

using AIGuiders.Platform.Execution.CommandPlane;
using AIGuiders.Platform.IntermediateRepresentation.Command;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class CatalogPathCompletionTests
{
    [Fact]
    public void Empty_body_lists_root_segments()
    {
        var catalog = Catalog([
            Descriptor("show dashboard", "dash.show"),
            Descriptor("select filter usage_date", "dash.date"),
            Descriptor("view card heatmap", "dash.view"),
        ]);

        var items = CatalogPathCompletion.GetSuggestions(catalog, "");

        Assert.Equal(3, items.Select(item => item.StepSegment).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.Contains(items, item => item.StepSegment == "show");
        Assert.Contains(items, item => item.StepSegment == "select");
        Assert.Contains(items, item => item.StepSegment == "view");
    }

    [Fact]
    public void Select_lists_branches()
    {
        var catalog = Catalog([
            Descriptor("select filter usage_date", "dash.date"),
            Descriptor("select report demo", "dash.report"),
        ]);

        var items = CatalogPathCompletion.GetSuggestions(catalog, "select ");

        Assert.Contains(items, item => item.StepSegment == "filter");
        Assert.Contains(items, item => item.StepSegment == "report");
    }

    [Fact]
    public void Branch_prefix_without_trailing_space_drills_down()
    {
        var catalog = Catalog([
            Descriptor("select filter usage_date", "dash.date", "Date"),
            Descriptor("select filter app_name", "dash.field", "App"),
        ]);

        var items = CatalogPathCompletion.GetSuggestions(catalog, "select filter");

        Assert.Equal(2, items.Count);
        Assert.Contains(items, item => item.StepSegment == "usage_date");
        Assert.Contains(items, item => item.StepSegment == "app_name");
    }

    static CommandCatalogIndex Catalog(IEnumerable<CommandDescriptor> descriptors) =>
        CommandCatalogIndex.FromDescriptors(descriptors);

    static CommandDescriptor Descriptor(string path, string commandId, string? help = null) =>
        CommandDescriptors.Describe(commandId)
            .Path(path)
            .Help(help ?? path)
            .Build();
}
