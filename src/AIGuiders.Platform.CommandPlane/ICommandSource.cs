#nullable enable

namespace AIGuiders.Platform.CommandPlane;

/// <summary>Loads slash command descriptors from a product-owned backend (code, file, DB, HTTP).</summary>
public interface ICommandSource
{
    /// <summary>Stable id for diagnostics and merge tracing (e.g. <c>json:bundled</c>, <c>db:PortalDB</c>).</summary>
    string SourceId { get; }

    IReadOnlyList<SlashCommandDescriptor> Load();
}
