#nullable enable

namespace AIGuiders.Platform.InputNotation;

/// <summary>Text surface for input notation parsers (GUIDERS-ADR-0016).</summary>
public enum InputNotationSurface
{
    /// <summary>Vim-style document notation, e.g. <c>&lt;C-k&gt; s p</c> (CIDE quarry).</summary>
    VimDocument,

    /// <summary>Hotkeys / UI wire, e.g. <c>Ctrl+K s p</c>.</summary>
    KeyGestureConfig,

    /// <summary>Neovim <c>:help key-notation</c> wire.</summary>
    NeovimKbd,

    /// <summary>Emacs <c>kbd</c> wire, e.g. <c>C-x C-f</c>.</summary>
    EmacsKbd,
}
