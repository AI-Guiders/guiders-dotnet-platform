using AIGuiders.Platform.CommandPlane;
using AIGuiders.Platform.CommandPlane.Sources;
using AIGuiders.Platform.Notations.Argument;
using AIGuiders.Platform.Notations.Command.Console;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class DescriptorArgumentNotationTests
{
    static readonly InvocationArgParameter ConfigParam = new(
        "config",
        InvocationArgParameterKind.Value,
        LongOption: "--config");

    static readonly InvocationArgParameter VerboseParam = new(
        "verbose",
        InvocationArgParameterKind.Flag,
        LongOption: "--verbose",
        ShortOption: "-v");

    [Fact]
    public void ParseTail_cli_schema_assigns_spaced_value_flag()
    {
        var descriptor = new InvocationArgDescriptor(InvocationArgWireClasses.Cli, [ConfigParam, VerboseParam]);
        var tail = DescriptorArgumentNotation.ParseTail("--config release --verbose", descriptor);

        Assert.Equal("cli", tail.WireClass);
        Assert.Equal("release", tail.Slots["config"]);
        Assert.Equal("true", tail.Slots["verbose"]);
    }

    [Fact]
    public void ParseTail_uses_descriptor_wire_class_over_inference()
    {
        var descriptor = new InvocationArgDescriptor(InvocationArgWireClasses.Positional, []);
        var tail = DescriptorArgumentNotation.ParseTail("one two", descriptor);

        Assert.Equal("positional", tail.WireClass);
        Assert.Equal("one", tail.Slots!["0"]);
    }

    [Fact]
    public void SlashCommandDescriptor_maps_to_invocation_arg_descriptor()
    {
        var command = new SlashCommandDescriptor
        {
            Domain = "build",
            Object = "run",
            Intent = "execute",
            CommandId = "build.run",
            Path = "build run",
            TailWireClass = InvocationArgWireClasses.Cli,
            ArgParameters = [ConfigParam],
        };

        var descriptor = command.ToInvocationArgDescriptor();
        Assert.Equal(InvocationArgWireClasses.Cli, descriptor.TailWireClass);
        Assert.Single(descriptor.Parameters!);
        Assert.Equal("config", descriptor.Parameters![0].Name);
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

        Assert.Equal(InvocationArgWireClasses.Cli, command.TailWireClass);
        Assert.Equal("config", command.ArgParameters[0].Name);
        Assert.Equal("--config", command.ArgParameters[0].LongOption);
    }

    [Fact]
    public void Console_try_parse_with_cli_descriptor_splits_path_and_schema_tail()
    {
        var descriptor = new InvocationArgDescriptor(InvocationArgWireClasses.Cli, [ConfigParam]);
        Assert.True(ConsoleCommandNotation.TryParse(
            "build run --config release",
            descriptor,
            out var path,
            out var tail));

        Assert.Equal(["build", "run"], path.Tokens);
        Assert.Equal("release", tail.Slots["config"]);
    }
}
