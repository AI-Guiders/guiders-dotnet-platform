#nullable enable
using Tomlyn;
using Tomlyn.Model;

namespace AIGuiders.Platform.CommandPlane.Binding.Sources;

public sealed class TomlBindingFormatReader : IBindingFormatReader
{
    public static TomlBindingFormatReader Instance { get; } = new();

    public string FormatName => "toml";

    public IReadOnlyList<BindingDescriptor> Read(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return [];

        var model = Toml.ToModel(text);
        var bindings = new List<BindingDescriptor>();

        foreach (var pair in model)
        {
            if (pair.Value is not string gesture)
                continue;

            bindings.Add(BindingDescriptor.FromFlatEntry(pair.Key, gesture));
        }

        return bindings;
    }
}
