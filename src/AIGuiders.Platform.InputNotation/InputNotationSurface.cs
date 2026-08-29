#nullable enable

namespace AIGuiders.Platform.InputNotation;

/// <summary>Text surface for input notation parsers (quarry from CIDE <c>Services/ChordNotation</c>).</summary>
public enum InputNotationSurface
{
    /// <summary>Vim-style document notation, e.g. <c>&lt;C-k&gt; s p</c>.</summary>
    VimDocument,

    /// <summary>Hotkeys / UI wire, e.g. <c>Ctrl+K s p</c>.</summary>
    KeyGestureConfig,

    /// <summary>Single melody note step (plain key token).</summary>
    MelodyNote,
}
