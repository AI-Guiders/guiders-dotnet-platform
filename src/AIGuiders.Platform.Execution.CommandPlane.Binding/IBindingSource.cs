using AIGuiders.Platform.IntermediateRepresentation.Binding;
#nullable enable

using AIGuiders.Platform.Execution.Sources;

namespace AIGuiders.Platform.Execution.CommandPlane.Binding;

public interface IBindingSource : ISource<IReadOnlyList<BindingDescriptor>>
{
}

public interface IBindingFormatReader
{
    string FormatName { get; }

    IReadOnlyList<BindingDescriptor> Read(string text);
}
