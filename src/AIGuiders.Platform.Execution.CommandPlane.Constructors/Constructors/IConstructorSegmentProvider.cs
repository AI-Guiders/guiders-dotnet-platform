#nullable enable

namespace AIGuiders.Platform.Execution.CommandPlane;

public interface IConstructorSegmentProvider
{
    IReadOnlyList<ArgCompletionItem> GetSegmentSuggestions(
        LeafConstructorDefinition leaf,
        int segmentIndex,
        ArgConstructorDraft draft,
        string partial);
}
