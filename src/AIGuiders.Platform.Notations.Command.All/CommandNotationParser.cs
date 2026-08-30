#nullable enable

using AIGuiders.Platform.Notations.Argument;
using AIGuiders.Platform.Notations.Command.Console;
using AIGuiders.Platform.Notations.Command.Slash;

namespace AIGuiders.Platform.Notations.Command;

/// <summary>Facade over command notation surfaces → wire path + optional arg tail (GUIDERS-ADR-0021).</summary>
public static class CommandNotationParser
{
    public static bool TryParse(
        string? line,
        CommandNotationSurface surface,
        out SlashWireBody pathWire,
        out NormalizedArguments args)
    {
        pathWire = new SlashWireBody([], false);
        args = NormalizedArguments.FromRaw("");

        if (string.IsNullOrWhiteSpace(line))
            return false;

        return surface switch
        {
            CommandNotationSurface.Slash => SlashCommandNotation.TryParseLine(line, out pathWire),
            CommandNotationSurface.Console => ConsoleCommandNotation.TryParse(line, out pathWire, out args),
            _ => false,
        };
    }
}
