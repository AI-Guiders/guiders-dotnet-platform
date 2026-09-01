namespace AIGuiders.Platform.Authoring.Command.Catalog;

public sealed record CatalogImport(string Path, string? Alias = null);

public interface ICatalogBundleLibrary
{
    bool TryResolve(string importPath, out IReadOnlyList<CatalogProfile> profiles);
}
