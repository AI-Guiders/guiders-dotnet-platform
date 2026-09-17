#nullable disable

namespace AIGuiders.Platform.Execution.CommandPlane.Catalog;

/// <summary>Federation bundled slash rows (attach + future federation verbs).</summary>
public static class FederationBundledCatalog
{
    public const string AttachSourceId = "federation:attach";

    public static ICommandSource AttachSource =>
        CommandSource.From(
            () => FederationAttachCatalog.AllDescriptors(),
            AttachSourceId);

    public static CommandCatalogIndex AttachIndex() =>
        CommandCatalogIndex.FromDescriptors(FederationAttachCatalog.AllDescriptors());
}
