#nullable enable

using AIGuiders.Platform.Modeling.Notations.Argument;
using AIGuiders.Platform.Notations.Command.Console;
using Microsoft.FSharp.Core;
using SlashCommandNotation = AIGuiders.Platform.Modeling.Notations.Command.Slash.SlashCommandNotation;
using SlashWireBody = AIGuiders.Platform.Modeling.Notations.Command.SlashWireBody;

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
            CommandNotationSurface.Slash => TryParseSlash(line, out pathWire),
            CommandNotationSurface.Console => ConsoleCommandNotation.TryParse(line, out pathWire, out args),
            _ => false,
        };
    }

    static bool TryParseSlash(string line, out SlashWireBody pathWire)
    {
        var opt = SlashCommandNotation.tryParseLine(line);
        if (FSharpOption<AIGuiders.Platform.Modeling.Notations.Command.SlashWireBody>.get_IsSome(opt))
        {
            pathWire = opt.Value;
            return true;
        }

        pathWire = new SlashWireBody([], false);
        return false;
    }
}
