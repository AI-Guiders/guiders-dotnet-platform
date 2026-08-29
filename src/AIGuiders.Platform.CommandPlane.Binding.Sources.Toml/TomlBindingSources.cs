#nullable enable

namespace AIGuiders.Platform.CommandPlane.Binding.Sources;

public static class TomlBindingSources
{
    public static IBindingSource FromToml(string content, string? sourceId = null) =>
        BindingSource.FromText(content, TomlBindingFormatReader.Instance, sourceId ?? "toml");

    public static IBindingSource FromFile(string path, string? sourceId = null) =>
        BindingSource.FromFile(path, TomlBindingFormatReader.Instance, sourceId);
}
