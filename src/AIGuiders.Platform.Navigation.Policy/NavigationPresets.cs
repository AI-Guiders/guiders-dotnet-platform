#nullable enable
using System.Text.Json;
using ModelingNavPolicy = AIGuiders.Platform.Modeling.Navigation.Policy;

namespace AIGuiders.Platform.Navigation.Policy;

/// <summary>GUIDERS-FSHARP-ADR-0003 §4.8 cutover: preset catalog SSOT in Modeling.Navigation.Policy.</summary>
public sealed record NavigationPresetDefinition(
    IReadOnlyList<string>? IncludeKinds,
    IReadOnlyList<string>? ExcludeKinds)
{
    public static NavigationPresetDefinition FromModel(ModelingNavPolicy.PresetDefinition model) => new(
        FSharpInterop.OptReadOnlyStringList(model.IncludeKinds),
        FSharpInterop.OptReadOnlyStringList(model.ExcludeKinds));
}

public static class NavigationPresets
{
    public static bool TryGet(string? presetId, out NavigationPresetDefinition definition)
    {
        var result = ModelingNavPolicy.Presets.tryGet(FSharpInterop.OptString(presetId));
        if (result is null)
        {
            definition = null!;
            return false;
        }

        definition = NavigationPresetDefinition.FromModel(result.Value.Item1);
        return true;
    }

    public static bool AllowsKind(string? presetId, string kind) =>
        ModelingNavPolicy.Presets.allowsKind(FSharpInterop.OptString(presetId), kind);

    public static string CatalogJson() =>
        JsonSerializer.Serialize(
            FSharpInterop.ToReadOnlyList(ModelingNavPolicy.Presets.names).ToDictionary(
                name => name,
                name =>
                {
                    var (def, _) = ModelingNavPolicy.Presets.tryGet(
                        Microsoft.FSharp.Core.FSharpOption<string>.Some(name))!.Value;
                    return new
                    {
                        include_kinds = FSharpInterop.OptReadOnlyStringList(def.IncludeKinds),
                        exclude_kinds = FSharpInterop.OptReadOnlyStringList(def.ExcludeKinds),
                    };
                }),
            new JsonSerializerOptions { WriteIndented = true });
}
