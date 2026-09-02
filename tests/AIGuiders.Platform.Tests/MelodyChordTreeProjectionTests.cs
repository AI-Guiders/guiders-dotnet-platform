#nullable enable
using AIGuiders.Platform.Execution.CommandPlane.Melody;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class MelodyChordTreeProjectionTests
{
    [Fact]
    public void Capture_stack_push_pop_tracks_depth()
    {
        var stack = new MelodyCaptureStack();
        stack.Push(new MelodyCaptureFrame("root", ""));
        stack.Push(new MelodyCaptureFrame("test", "t"));

        Assert.Equal(2, stack.Depth);
        Assert.Equal("test", stack.Current!.NodeId);

        Assert.True(stack.TryPop(out var popped));
        Assert.Equal("test", popped!.NodeId);
        Assert.Equal("root", stack.Current!.NodeId);
    }

    [Fact]
    public void Projector_filters_next_options_by_consumed_prefix()
    {
        var catalog = new TestMelodyGraphCatalog(
            "root",
            new Dictionary<string, IReadOnlyList<MelodyGraphEdge>>
            {
                ["root"] =
                [
                    new("ra", "Run all", MelodyGraphNodeKind.Command, "cmd.run-all"),
                    new("rb", "Run branch", MelodyGraphNodeKind.Command, "cmd.run-branch"),
                    new("t", "Test", MelodyGraphNodeKind.SubRoot, "test"),
                ],
                ["test"] =
                [
                    new("ra", "Run all", MelodyGraphNodeKind.Command, "cmd.test-run-all"),
                ],
            });

        var stack = new MelodyCaptureStack();
        stack.Push(new MelodyCaptureFrame("root", ""));
        stack.Push(new MelodyCaptureFrame("test", "r"));

        var projection = MelodyChordTreeProjector.Project(
            stack,
            catalog,
            MelodyChordTreeViewMode.Neighborhood);

        Assert.Equal(["root", "test"], projection.Breadcrumb);
        Assert.Equal("r", projection.ConsumedPrefix);
        Assert.Single(projection.NextOptions);
        Assert.Equal("ra", projection.NextOptions[0].Label);
    }

    [Fact]
    public void Empty_catalog_yields_empty_next_options()
    {
        var stack = new MelodyCaptureStack();
        stack.Push(new MelodyCaptureFrame(MelodyGraphCatalogEmpty.Instance.RootNodeId, ""));

        var projection = MelodyChordTreeProjector.Project(
            stack,
            MelodyGraphCatalogEmpty.Instance,
            MelodyChordTreeViewMode.Minimal);

        Assert.Empty(projection.NextOptions);
    }

    sealed class TestMelodyGraphCatalog(string rootNodeId, IReadOnlyDictionary<string, IReadOnlyList<MelodyGraphEdge>> edges)
        : IMelodyGraphCatalog
    {
        public string RootNodeId => rootNodeId;

        public bool TryGetEdges(string nodeId, out IReadOnlyList<MelodyGraphEdge> nodeEdges)
            => edges.TryGetValue(nodeId, out nodeEdges!);
    }
}
