using AIGuiders.Platform.IntermediateRepresentation.Command;
#nullable enable

using AIGuiders.Platform.CommandPlane;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class VisualCommandTreeSlashProjectionTests
{
    [Fact]
    public void Slash_projector_maps_completion_rows_and_guidance()
    {
        var result = new SlashCompletionResult(
            [
                new ArgCompletionItem(
                    "select filter usage_date ",
                    "select filter usage_date",
                    "Date filter",
                    "Filter",
                    "usage_date",
                    ArgCompletionItemKind.Segment),
                new ArgCompletionItem(
                    "",
                    "select filter usage_date",
                    "Сегодня",
                    "Filter",
                    "Сегодня",
                    ArgCompletionItemKind.ConstructorEntry,
                    "date_today"),
            ],
            new SlashInputGuidance(
                "/select › filter › usage_date",
                "Pick a value",
                "Choose a value — Tab to insert",
                InvocationLinePhase.Arg,
                InvocationArgMechanic.Picker,
                "select filter usage_date",
                nameof(CommandArgTailKind.Picker)));

        var projection = SlashVisualCommandTreeProjector.Project(result);

        Assert.Equal(VisualCommandTreeEngageKind.SlashLine, projection.EngageKind);
        Assert.Equal("/select › filter › usage_date", projection.BreadcrumbDisplay);
        Assert.Equal("Pick a value", projection.Placeholder);
        Assert.Equal(2, projection.NextOptions.Count);
        Assert.Equal(VisualCommandTreeNodeKind.ConstructorEntry, projection.NextOptions[1].Kind);
        Assert.Equal("date_today", projection.NextOptions[1].PickValue);
    }

    [Fact]
    public void Constructor_mode_uses_constructor_engage_kind()
    {
        var result = new SlashCompletionResult(
            [],
            new SlashInputGuidance(
                "/select filter usage_date › Дата (с) › 31.08.",
                "Год",
                "Дата (с): Год",
                InvocationLinePhase.Arg,
                InvocationArgMechanic.Constructor,
                "select filter usage_date",
                nameof(InvocationArgMechanic.Constructor)));

        var projection = SlashVisualCommandTreeProjector.Project(result);

        Assert.Equal(VisualCommandTreeEngageKind.Constructor, projection.EngageKind);
        Assert.Equal(nameof(InvocationArgMechanic.Constructor), projection.InputMode);
    }
}
