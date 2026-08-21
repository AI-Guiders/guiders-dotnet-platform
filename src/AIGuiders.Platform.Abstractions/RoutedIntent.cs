namespace AIGuiders.Platform.Abstractions;

/// <summary>
/// Parsed intent before host execute — minimal cross-product envelope.
/// Products extend with typed verbs (e.g. CDP <c>CitizenIntentRouter.Verb</c>).
/// </summary>
public sealed record RoutedIntent(
    string Verb,
    string Raw,
    bool Ok,
    string? Go = null,
    string? Organ = null,
    string? Path = null,
    string? Detail = null,
    string? Scene = null,
    string? Cmd = null,
    string? OldString = null,
    string? NewString = null,
    string? Op = null,
    string? Reason = null);
