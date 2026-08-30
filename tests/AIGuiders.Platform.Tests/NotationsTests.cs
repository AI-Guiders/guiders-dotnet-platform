using AIGuiders.Platform.Notations;
using AIGuiders.Platform.Notations.Argument.Delimited;
using AIGuiders.Platform.Notations.Argument.Kv;
using AIGuiders.Platform.Notations.Command.Console;
using AIGuiders.Platform.Notations.Command.Slash;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class NotationsTests
{
    [Fact]
    public void Slash_body_tokenizes_with_trailing_space_flag()
    {
        var wire = SlashCommandNotation.ParseBody("buffer open ");
        Assert.Equal(["buffer", "open"], wire.Tokens);
        Assert.True(wire.EndsWithSpaceAfterTokens);
    }

    [Fact]
    public void Kv_parses_slots()
    {
        var tail = KvArgumentNotation.Parse("doc=README.md op=scene");
        Assert.NotNull(tail.Slots);
        Assert.Equal("README.md", tail.Slots!["doc"]);
        Assert.Equal("scene", tail.Slots["op"]);
    }

    [Fact]
    public void Console_splits_path_before_first_kv_token()
    {
        Assert.True(ConsoleCommandNotation.TryParse("buffer open doc=README.md", out var path, out var args));
        Assert.Equal(["buffer", "open"], path.Tokens);
        Assert.Equal("README.md", args.Slots!["doc"]);
    }

    [Fact]
    public void Delimited_parses_colon_slots()
    {
        var tail = DelimitedArgumentNotation.Parse("5:10");
        Assert.Equal(DelimitedArgumentNotation.WireClassColon, tail.WireClass);
        Assert.NotNull(tail.Slots);
        Assert.Equal("5", tail.Slots!["0"]);
        Assert.Equal("10", tail.Slots!["1"]);
    }

    [Theory]
    [InlineData("/buffer open", "buffer open")]
    [InlineData("/scene focus", "scene focus")]
    public void Invocation_parity_slash_and_console_paths(string slashLine, string consoleLine)
    {
        Assert.True(SlashCommandNotation.TryParseLine(slashLine, out var slashWire));
        Assert.True(ConsoleCommandNotation.TryParse(consoleLine, out var consoleWire, out _));

        var slashPath = InvocationNotation.FromPathSegments(slashWire.Tokens);
        var consolePath = InvocationNotation.FromPathSegments(consoleWire.Tokens);

        Assert.True(InvocationNotation.PathsEqual(slashPath, consolePath));
    }
}
