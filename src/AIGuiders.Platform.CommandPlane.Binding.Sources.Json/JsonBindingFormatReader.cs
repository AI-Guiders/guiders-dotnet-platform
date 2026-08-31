using AIGuiders.Platform.IntermediateRepresentation.Binding;
#nullable enable
using System.Text.Json;

namespace AIGuiders.Platform.CommandPlane.Binding.Sources;

public sealed class JsonBindingFormatReader : IBindingFormatReader
{
    public static JsonBindingFormatReader Instance { get; } = new();

    static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    public string FormatName => "json";

    public IReadOnlyList<BindingDescriptor> Read(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return [];

        using var document = JsonDocument.Parse(text);
        var root = document.RootElement;

        if (root.ValueKind == JsonValueKind.Object &&
            root.TryGetProperty("bindings", out var bindingsNode) &&
            bindingsNode.ValueKind == JsonValueKind.Object)
        {
            return ReadObject(bindingsNode);
        }

        if (root.ValueKind == JsonValueKind.Object)
            return ReadObject(root);

        throw new InvalidOperationException("JSON binding document must be a flat object or { \"bindings\": { } }.");
    }

    static List<BindingDescriptor> ReadObject(JsonElement node)
    {
        var bindings = new List<BindingDescriptor>();
        foreach (var property in node.EnumerateObject())
        {
            if (property.Value.ValueKind != JsonValueKind.String)
                continue;

            bindings.Add(BindingDescriptor.FromFlatEntry(property.Name, property.Value.GetString() ?? ""));
        }

        return bindings;
    }
}
