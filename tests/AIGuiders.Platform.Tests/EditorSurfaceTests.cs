#nullable enable
using AIGuiders.Platform.CommandPlane;
using AIGuiders.Platform.CommandPlane.Commands;
using AIGuiders.Platform.CommandPlane.Editor;
using AIGuiders.Platform.CommandPlane.Editor.Commands;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class EditorSurfaceTests
{
    [Fact]
    public void Apply_bold_wraps_selection()
    {
        var bold = MarkdownTextDialectCatalog.TryGetFormat("bold")!;
        var result = EditorTextTransform.ApplyFormat(
            "say hello",
            new EditorSelectionSpan(4, 9),
            bold);
        Assert.Equal("say **hello**", result.Text);
        Assert.Equal(6, result.SelectionStart);
        Assert.Equal(11, result.SelectionEnd);
    }

    [Fact]
    public void Line_range_parser_accepts_colon_and_spaces()
    {
        Assert.True(EditorLineRangeParser.TryParse("5 10", out var a));
        Assert.Equal(new EditorLineRange(5, 10), a);
        Assert.True(EditorLineRangeParser.TryParse("5:10", out var b));
        Assert.Equal(new EditorLineRange(5, 10), b);
    }

    [Fact]
    public void Delete_line_range_removes_lines()
    {
        var text = "a\nb\nc\nd";
        var result = EditorLineTextOps.DeleteLineRange(text, new EditorLineRange(2, 3));
        Assert.Equal("a\nd", result.Text);
    }

    [Fact]
    public void Implicit_arg_tail_kinds_parse()
    {
        Assert.Equal(SlashArgTailKind.ImplicitSelection, SlashArgTailPolicy.Parse("implicit:selection"));
        Assert.Equal(SlashArgTailKind.ImplicitLineRange, SlashArgTailPolicy.Parse("implicit:line_range"));
    }

    [Fact]
    public void Platform_command_registry_executes_bold_format()
    {
        var ctx = new EditorBufferContext
        {
            Text = "hi",
            Selection = new EditorSelectionSpan(0, 0),
        };
        Assert.True(EditorCommandRegistry.TryExecute("bold", ctx, out var outcome));
        Assert.True(outcome.Success);
        Assert.Equal("**text**hi", outcome.EditorBuffer!.Text);
    }

    [Fact]
    public void Platform_command_registry_deletes_line_range()
    {
        var ctx = new EditorBufferContext
        {
            Text = "a\nb\nc",
            Selection = new EditorSelectionSpan(0, 0),
            ArgsTail = "2",
        };
        Assert.True(EditorCommandRegistry.TryExecute(EditorLineDeleteCommand.Id, ctx, out var outcome));
        Assert.Equal("a\nc", outcome.EditorBuffer!.Text);
    }

    [Fact]
    public void Editor_format_insert_command_is_platform_command()
    {
        var bold = MarkdownTextDialectCatalog.TryGetFormat("bold")!;
        IPlatformCommand<EditorBufferContext> command = new EditorFormatInsertCommand(bold);
        Assert.Equal("bold", command.CommandId);
        Assert.True(command.CanExecute(new EditorBufferContext { Text = "x", Selection = default }));
    }
}
