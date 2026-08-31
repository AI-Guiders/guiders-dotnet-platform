#nullable enable
using AIGuiders.Platform.CommandPlane.Editor.Commands;

namespace AIGuiders.Platform.CommandPlane.Editor;

/// <summary>Slash catalog projections for bundled editor commands.</summary>
public static class EditorCatalogProjections
{
    public static CommandDescriptor ToSlashDescriptor(this TextInsertFormatDefinition format) =>
        new()
        {
            Domain = "editor",
            Object = "format",
            Intent = format.Id,
            CommandId = format.Id,
            Path = format.Path.TrimStart('/'),
            Help = format.Help,
            Group = format.Category,
            ArgTail = format.ArgTail,
            Surfaces = ["editor-ccl", "editor-inline"],
            RequiredCapabilities = ["write"],
            Tier = "core",
        };

    public static CommandDescriptor EditorLineSelectDescriptor() =>
        new()
        {
            Domain = "editor",
            Object = "line",
            Intent = "select",
            CommandId = EditorLineSelectCommand.Id,
            Path = "editor line select",
            Help = "Select line range (tail: start end or start:end)",
            Group = "Editor",
            ArgTail = CommandArgTailPolicy.ImplicitLineRange,
            ArgHint = "Line range — e.g. 5 10 or 5:10; empty uses current selection",
            Surfaces = ["editor-ccl", "editor-inline"],
            RequiredCapabilities = ["read"],
            Tier = "core",
        };

    public static CommandDescriptor EditorLineDeleteDescriptor() =>
        new()
        {
            Domain = "editor",
            Object = "line",
            Intent = "delete",
            CommandId = EditorLineDeleteCommand.Id,
            Path = "editor line delete",
            Help = "Delete line range",
            Group = "Editor",
            ArgTail = CommandArgTailPolicy.ImplicitLineRange,
            ArgHint = "Line range — empty uses current selection lines",
            Surfaces = ["editor-ccl", "editor-inline"],
            RequiredCapabilities = ["write"],
            Tier = "core",
        };
}
