using AIGuiders.Platform.IntermediateRepresentation.Command;
using AIGuiders.Platform.Execution.CommandPlane;
using AIGuiders.Platform.Execution.CommandPlane.Catalog.Sources;
using AIGuiders.Platform.IntermediateRepresentation.Argument;
using AIGuiders.Platform.Notations.Command.Console;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class ArgumentNotationTests
{
    static readonly ArgumentSlot ConfigSlot = new(
        "config",
        ArgumentSlotKind.Value,
        LongOption: "--config");

    static readonly ArgumentSlot VerboseSlot = new(
        "verbose",
        ArgumentSlotKind.Flag,
        LongOption: "--verbose",
        ShortOption: "-v");

    [Fact]
    public void Parse_cli_schema_assigns_spaced_value_flag()
    {
        var profile = new ArgumentNotationProfile(ArgumentReaders.Cli, [ConfigSlot, VerboseSlot]);
        var args = ArgumentNotation.Parse("--config release --verbose", profile);

        Assert.Equal("cli", args.ReaderId);
        Assert.Equal("release", args.Slots!["config"]);
        Assert.Equal("true", args.Slots!["verbose"]);
    }

    [Fact]
    public void Parse_uses_profile_wire_class_over_inference()
    {
        var profile = new ArgumentNotationProfile(ArgumentReaders.Positional, []);
        var args = ArgumentNotation.Parse("one two", profile);

        Assert.Equal("positional", args.ReaderId);
        Assert.Equal("one", args.Slots!["0"]);
    }

    [Fact]
    public void CommandDescriptor_carries_argument_notation_profile()
    {
        var command = new CommandDescriptor
        {
            Domain = "build",
            Object = "run",
            Intent = "execute",
            CommandId = "build.run",
            Path = "build run",
            ArgumentNotation = new ArgumentNotationProfile(ArgumentReaders.Cli, [ConfigSlot]),
        };

        Assert.Equal(ArgumentReaders.Cli, command.ArgumentNotation!.ReaderId);
        Assert.Single(command.ArgumentNotation.Slots!);
        Assert.Equal("config", command.ArgumentNotation.Slots![0].Name);
    }

    [Fact]
    public void Json_catalog_reads_tail_wire_class_and_arg_parameters()
    {
        const string json = """
            [
              {
                "commandId": "build.run",
                "path": "build run",
                "tailWireClass": "cli",
                "argParameters": [
                  { "name": "config", "kind": "value", "longOption": "--config" }
                ]
              }
            ]
            """;

        var commands = JsonCommandFormatReader.Instance.Read(json);
        var command = Assert.Single(commands);

        Assert.Equal(ArgumentReaders.Cli, command.ArgumentNotation!.ReaderId);
        Assert.Equal("config", command.ArgumentNotation.Slots![0].Name);
        Assert.Equal("--config", command.ArgumentNotation.Slots![0].LongOption);
    }

    [Fact]
    public void Console_try_parse_with_cli_profile_splits_path_and_schema_tail()
    {
        var profile = new ArgumentNotationProfile(ArgumentReaders.Cli, [ConfigSlot]);
        Assert.True(ConsoleCommandNotation.TryParse(
            "build run --config release",
            profile,
            out var path,
            out var args));

        Assert.Equal(["build", "run"], path.Tokens);
        Assert.Equal("release", args.Slots!["config"]);
    }
}
