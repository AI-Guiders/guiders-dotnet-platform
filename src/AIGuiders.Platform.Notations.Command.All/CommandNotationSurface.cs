#nullable enable

namespace AIGuiders.Platform.Notations.Command;

/// <summary>Text surface for command path notation parsers (GUIDERS-ADR-0021).</summary>
public enum CommandNotationSurface
{
    /// <summary>Slash wire, e.g. <c>/buffer open</c>.</summary>
    Slash,

    /// <summary>Console wire, e.g. <c>buffer open doc=README.md</c>.</summary>
    Console,
}
