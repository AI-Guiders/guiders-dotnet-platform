using AIGuiders.Platform.Authoring.Core;
using Xunit;

namespace AIGuiders.Platform.Authoring.Tests;

public sealed class AuthoringCoreTests
{
    [Fact]
    public void BlockReader_reads_defaults_until_end()
    {
        var lines = AuthoringSource.FromText(
            """
            defaults
              command.scope = dashboard
            end defaults
            """);

        Assert.True(BlockReader.TryParseOpener(lines[0].Text, out var opener));
        Assert.Equal("defaults", opener.Keyword);

        var block = BlockReader.Read(lines, 1, opener.Keyword);
        Assert.True(block.IsClosed);
        Assert.Single(block.Body);
        Assert.Equal("command.scope = dashboard", block.Body[0].Text.Trim());
    }

    [Fact]
    public void TableSurface_parses_pipe_rows()
    {
        var body = AuthoringSource.FromText(
            """
            | a | b |
            | --- | --- |
            | 1 | 2 |
            """);

        var maps = TableSurface.ParseMaps(body);
        Assert.Single(maps);
        Assert.Equal("1", maps[0]["a"]);
        Assert.Equal("2", maps[0]["b"]);
    }

    [Fact]
    public void InnerBlockFilter_strips_nested_end_markers()
    {
        var body = AuthoringSource.FromText(
            """
              grammar
                command = command-slash
              end grammar
            """);

        var filtered = InnerBlockFilter.StripEndMarkers(body);
        Assert.DoesNotContain(filtered, static l => l.Text.StartsWith("end ", StringComparison.Ordinal));
        Assert.Equal(2, filtered.Count);
    }

    [Fact]
    public void DocumentWalker_dispatches_section_to_handler()
    {
        var registry = new SectionHandlerRegistry<TestWalkContext>(
        [
            new TestDefaultsHandler(),
        ]);

        var walker = new AuthoringDocumentWalker<TestWalkContext>(
            registry,
            static ctx => ctx.Diagnostics,
            static (_, _) => false);

        var context = new TestWalkContext();
        var lines = AuthoringSource.FromText(
            """
            defaults
              command.scope = dashboard
            end defaults
            """);

        walker.Walk(lines, context);
        Assert.Equal("dashboard", context.Values["command.scope"]);
    }

    sealed class TestWalkContext
    {
        public Dictionary<string, string> Values { get; } = new(StringComparer.OrdinalIgnoreCase);
        public List<AuthoringDiagnostic> Diagnostics { get; } = [];
    }

    sealed class TestDefaultsHandler : IAuthoringSectionHandler<TestWalkContext>
    {
        public string Keyword => "defaults";

        public void Apply(TestWalkContext context, AuthoringSectionBlock block) =>
            KvSurface.MergeInto(context.Values, block.Body);
    }
}
