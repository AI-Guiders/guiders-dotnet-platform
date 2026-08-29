#nullable enable

namespace AIGuiders.Platform.CommandPlane.Binding;

public interface IBindingSource
{
    string SourceId { get; }

    IReadOnlyList<BindingDescriptor> Load();
}

public interface IBindingFormatReader
{
    string FormatName { get; }

    IReadOnlyList<BindingDescriptor> Read(string text);
}
