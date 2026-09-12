#nullable enable

using ModelingNavPolicy = AIGuiders.Platform.Modeling.Navigation.Policy;

namespace AIGuiders.Platform.Navigation.Policy;

/// <summary>
/// GUIDERS-FSHARP-ADR-0003 §4.8 cutover: preset merge SSOT via <see cref="ModelingNavPolicy.PresetMerge"/> (Modeling.Navigation.Policy).
/// Merges a named preset from <see cref="NavigationPresets"/> with MCP/host request overrides.
/// </summary>
public static class NavigationPresetMerge
{
    /// <summary>
    /// Non-null <paramref name="requestInclude"/> / <paramref name="requestExclude"/> override the preset side;
    /// when both preset and request specify exclude, lists are unioned (deduped by canonical kind).
    /// </summary>
    public static (IReadOnlyList<string>? Include, IReadOnlyList<string>? Exclude, string? Error) Merge(
        string? presetName,
        IReadOnlyList<string>? requestInclude,
        IReadOnlyList<string>? requestExclude)
    {
        var (include, exclude, error) = ModelingNavPolicy.PresetMerge.merge(
            FSharpInterop.OptString(presetName),
            FSharpInterop.OptFSharpStringList(requestInclude),
            FSharpInterop.OptFSharpStringList(requestExclude));

        return (
            FSharpInterop.OptReadOnlyStringList(include),
            FSharpInterop.OptReadOnlyStringList(exclude) ?? Array.Empty<string>(),
            FSharpInterop.OptString(error));
    }
}
