#nullable enable

namespace AIGuiders.Platform.CommandPlane;

public interface ISlashConstructorSegmentProvider
{
    IReadOnlyList<SlashCompletionItem> GetSegmentSuggestions(
        SlashLeafConstructorDefinition leaf,
        int segmentIndex,
        SlashConstructorDraft draft,
        string partial);
}
