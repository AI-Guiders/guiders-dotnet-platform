#nullable enable

namespace AIGuiders.Platform.Navigation.Code;

public sealed record NavigationRelatedItem(
    string Path,
    string Kind,
    string? Rationale = null,
    string? RelativePath = null);
