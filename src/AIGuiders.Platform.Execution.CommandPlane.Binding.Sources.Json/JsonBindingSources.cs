#nullable enable

namespace AIGuiders.Platform.Execution.CommandPlane.Binding.Sources;

public static class JsonBindingSources
{
    public static IBindingSource FromJson(string content, string? sourceId = null) =>
        BindingSource.FromText(content, JsonBindingFormatReader.Instance, sourceId ?? "json");

    public static IBindingSource FromFile(string path, string? sourceId = null) =>
        BindingSource.FromFile(path, JsonBindingFormatReader.Instance, sourceId);
}
