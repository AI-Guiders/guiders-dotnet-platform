using AIGuiders.Platform.CommandPlane;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class ValueConstructorNavigatorTests
{
    [Fact]
    public void Navigator_emits_date_range_wire_for_full_tree_walk()
    {
        var registry = BuildDateRangeRegistry();
        var navigator = new ValueConstructorNavigator(registry, new StubSegmentProvider());
        var draft = new ArgConstructorDraft
        {
            RootConstructorId = "date_range",
            CanonicalPath = "select filter usage_date",
        };

        Assert.True(navigator.TryAdvance(draft, "2026"));
        Assert.True(navigator.TryAdvance(draft, "08"));
        Assert.True(navigator.TryAdvance(draft, "01"));
        Assert.True(navigator.TryAdvance(draft, "2026"));
        Assert.True(navigator.TryAdvance(draft, "09"));
        Assert.True(navigator.TryAdvance(draft, "15"));

        Assert.True(navigator.TryEmitWire(draft, out var wire, out var error), error);
        Assert.Equal("2026-08-01..2026-09-15", wire);
    }

    [Fact]
    public void Navigator_pads_segment_values_from_leaf_metadata()
    {
        var registry = BuildDateRangeRegistry();
        var navigator = new ValueConstructorNavigator(registry, new StubSegmentProvider());
        var draft = new ArgConstructorDraft
        {
            RootConstructorId = "date_range",
            CanonicalPath = "select filter usage_date",
        };

        Assert.True(navigator.TryAdvance(draft, "2026"));
        Assert.True(navigator.TryAdvance(draft, "8"));
        Assert.True(navigator.TryAdvance(draft, "1"));
        Assert.True(navigator.TryAdvance(draft, "2026"));
        Assert.True(navigator.TryAdvance(draft, "9"));
        Assert.True(navigator.TryAdvance(draft, "5"));

        Assert.True(navigator.TryEmitWire(draft, out var wire, out var error), error);
        Assert.Equal("2026-08-01..2026-09-05", wire);
    }

    static ValueConstructorRegistry BuildDateRangeRegistry()
    {
        var registry = new ValueConstructorRegistry();
        registry.Register(new LeafConstructorDefinition(
            "date",
            "Date",
            [
                new ConstructorSegmentDefinition("year", "Year"),
                new ConstructorSegmentDefinition("month", "Month", WireMinWidth: 2, DisplayMinWidth: 2),
                new ConstructorSegmentDefinition("day", "Day", WireMinWidth: 2, DisplayMinWidth: 2),
            ],
            WirePattern: "{year}-{month}-{day}",
            DisplayPattern: "{day}.{month}.{year}"));

        registry.Register(new CompositeConstructorDefinition(
            "date_range",
            "Range",
            [
                new ConstructorSlotDefinition("from", "date", "From"),
                new ConstructorSlotDefinition("to", "date", "To", SeparatorBefore: ".."),
            ],
            WirePattern: "{from}..{to}"));

        return registry;
    }

    sealed class StubSegmentProvider : IConstructorSegmentProvider
    {
        public IReadOnlyList<ArgCompletionItem> GetSegmentSuggestions(
            LeafConstructorDefinition leaf,
            int segmentIndex,
            ArgConstructorDraft draft,
            string partial) => [];
    }
}
