#nullable enable

using AIGuiders.Platform.Execution.CommandPlane.Commands;
using AIGuiders.Platform.Execution.LanguageIntelligence.Bundled.Commands;
using AIGuiders.Platform.Execution.LanguageIntelligence.Edit;
using AIGuiders.Platform.Execution.LanguageIntelligence.Markup;

namespace AIGuiders.Platform.Execution.LanguageIntelligence.Bundled;

/// <summary>Bundled editor buffer commands for Forge View (GUIDERS-ADR-0009 GoF registry).</summary>
public static class EditorCommandRegistry
{
    private static readonly Lazy<PlatformCommandRegistry<EditorBufferContext>> Bundled =
        new(CreateBundled);

    public static PlatformCommandRegistry<EditorBufferContext> CreateBundled()
    {
        var registry = new PlatformCommandRegistry<EditorBufferContext>();
        foreach (var format in MarkdownTextDialectCatalog.InsertFormats)
            registry.Register(new EditorFormatInsertCommand(format));

        registry.Register(new EditorLineSelectCommand());
        registry.Register(new EditorLineDeleteCommand());
        return registry;
    }

    public static PlatformCommandRegistry<EditorBufferContext> BundledRegistry => Bundled.Value;

    public static bool TryExecute(
        string commandId,
        EditorBufferContext context,
        out CommandOutcome outcome) =>
        BundledRegistry.TryExecute(commandId, context, out outcome);
}
