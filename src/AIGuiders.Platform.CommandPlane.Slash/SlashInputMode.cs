#nullable enable

namespace AIGuiders.Platform.CommandPlane;

/// <summary>What the user should do next in the slash input (ADR-0012 §input guidance).</summary>
public enum SlashInputMode
{
    /// <summary>Completing command path segments (domain / object / intent).</summary>
    Path = 0,

    /// <summary>Pick from closed list (<see cref="CommandPickerChoice"/>).</summary>
    Picker = 1,

    /// <summary>Type a required free-text argument.</summary>
    FreeText = 2,

    /// <summary>Optional argument — Enter runs without arg.</summary>
    Optional = 3,

    /// <summary>Command is runnable — Enter executes.</summary>
    Ready = 4,

    /// <summary>Inside a value constructor tree (GUIDERS-ADR-0035).</summary>
    Constructor = 5,

    /// <summary>Typing a locale value after path completion (GUIDERS-ADR-0037).</summary>
    TypedInput = 6,
}
