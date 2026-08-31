using AIGuiders.Platform.IntermediateRepresentation.Binding;
using AIGuiders.Platform.IntermediateRepresentation.Command;
#nullable enable
using AIGuiders.Platform.Combinations;
using AIGuiders.Platform.Combinations.Binding;
using AIGuiders.Platform.Combinations.Catalog;
using AIGuiders.Platform.Combinations.Sources;
using AIGuiders.Platform.Combinations.Workspace;
using AIGuiders.Platform.CommandPlane;
using AIGuiders.Platform.CommandPlane.Binding;
using AIGuiders.Platform.Configurations.Workspace;
using AIGuiders.Platform.Sources;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class CombinationTests
{
    [Fact]
    public void OrderedCombination_Fold_applies_combinator_in_order()
    {
        var result = OrderedCombination.Fold(
            new[] { 1, 10, 100 },
            static (baseline, overlay) => baseline + overlay);

        Assert.Equal(111, result);
    }

    [Fact]
    public void OrderedCombination_FoldLayers_projects_each_layer()
    {
        var result = OrderedCombination.FoldLayers(
            new[] { "a", "b", "c" },
            "",
            static s => s,
            static (baseline, overlay) => baseline + overlay);

        Assert.Equal("abc", result);
    }

    [Fact]
    public void SourceCombination_Merge_uses_workspace_field_overlay()
    {
        var baseline = SourceCatalog.From(
            new WorkspaceDocument
            {
                Workspace = new WorkspaceSection
                {
                    Adr = new WorkspaceAdrSettings { RootDir = "docs/adr", MaxRelated = 3 },
                },
            },
            "baseline");

        var overlay = SourceCatalog.From(
            new WorkspaceDocument
            {
                Workspace = new WorkspaceSection
                {
                    Adr = new WorkspaceAdrSettings { MaxRelated = 7 },
                },
            },
            "overlay");

        var merged = SourceCombination.Merge(baseline, overlay, WorkspaceCombinators.FieldOverlay, "merged");
        var doc = merged.Load();

        Assert.Equal("merged", merged.SourceId);
        Assert.Equal("docs/adr", doc.Workspace!.Adr!.RootDir);
        Assert.Equal(7, doc.Workspace.Adr.MaxRelated);
    }

    [Fact]
    public void WorkspaceCombinators_exposes_field_overlay_semantics()
    {
        Assert.Equal(CombinationSemantics.FieldOverlay, WorkspaceCombinators.Semantics);
    }

    [Fact]
    public void CommandCatalogCombinators_ship_first_keeps_baseline_path()
    {
        Assert.Equal(CombinationSemantics.ShipFirst, CommandCatalogCombinators.Semantics);

        var ship = CommandCatalogIndex.FromDescriptors(
        [
            new CommandDescriptor
            {
                Domain = "", Object = "", Intent = "",
                CommandId = "ship.cmd", Path = "ship",
            },
        ]);
        var user = CommandCatalogIndex.FromDescriptors(
        [
            new CommandDescriptor
            {
                Domain = "", Object = "", Intent = "",
                CommandId = "user.cmd", Path = "ship",
            },
            new CommandDescriptor
            {
                Domain = "", Object = "", Intent = "",
                CommandId = "extra.cmd", Path = "extra",
            },
        ]);

        var merged = CommandCatalogCombinators.ShipFirst(ship, user);

        Assert.True(merged.TryGet("ship", out var route));
        Assert.Equal("ship.cmd", route.CommandId);
        Assert.True(merged.TryGet("extra", out _));
    }

    [Fact]
    public void BindingCombinators_overlay_wins_on_key_collision()
    {
        Assert.Equal(CombinationSemantics.OverlayWins, BindingCombinators.Semantics);

        var ship = BindingCatalogIndex.FromDescriptors(
        [
            new BindingDescriptor
            {
                BindingKey = "toggle_command_palette",
                GestureWire = "Ctrl+Q",
                TargetKind = BindingTargetKind.Command,
            },
        ]);
        var user = BindingCatalogIndex.FromDescriptors(
        [
            new BindingDescriptor
            {
                BindingKey = "toggle_command_palette",
                GestureWire = "Ctrl+Shift+Q",
                TargetKind = BindingTargetKind.Command,
            },
        ]);

        var merged = BindingCombinators.OverlayWins(ship, user);

        Assert.True(merged.TryGetByKey("toggle_command_palette", out var entry));
        Assert.Equal("Ctrl+Shift+Q", entry.Descriptor.GestureWire);
    }

    [Fact]
    public void CommandCatalogCombination_compose_matches_composer()
    {
        var ship = CommandSource.From([
            new CommandDescriptor
            {
                Domain = "", Object = "", Intent = "",
                CommandId = "a", Path = "alpha",
            },
        ], "ship");
        var overlay = CommandSource.From([
            new CommandDescriptor
            {
                Domain = "", Object = "", Intent = "",
                CommandId = "b", Path = "beta",
            },
        ], "overlay");

        var viaCombination = CommandCatalogCombination.Compose(ship, overlay);
        var viaComposer = CommandCatalogComposer.Build(ship, overlay);

        Assert.True(viaCombination.TryGet("alpha", out _));
        Assert.True(viaCombination.TryGet("beta", out _));
        Assert.True(viaComposer.TryGet("alpha", out _));
        Assert.True(viaComposer.TryGet("beta", out _));
    }
}
