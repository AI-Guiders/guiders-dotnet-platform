#nullable enable
using System.Text.Json;
using AIGuiders.Platform.Combinations;
using AIGuiders.Platform.Combinations.Binding;
using AIGuiders.Platform.Combinations.Catalog;
using AIGuiders.Platform.Combinations.Workspace;
using AIGuiders.Platform.CommandPlane;
using AIGuiders.Platform.CommandPlane.Binding;
using AIGuiders.Platform.Configurations.Workspace;

namespace AIGuiders.Platform.Conformance.Policies;

public static class PolicySpecConformance
{
    public static IReadOnlyList<string> ValidateDocument(PolicySpecDocument spec)
    {
        if (!string.Equals(spec.Kind, "policy", StringComparison.Ordinal))
            return [$"Expected kind \"policy\", got \"{spec.Kind}\"."];

        if (!TryResolveCombinator(spec, out var combinator, out var resolveError))
            return [resolveError];

        var errors = new List<string>();
        foreach (var vector in spec.Vectors)
        {
            if (!TryValidateVector(spec, combinator!, vector, out var error))
                errors.Add(error);
        }

        return errors;
    }

    public static bool TryValidateVector(
        PolicySpecDocument spec,
        object combinator,
        PolicySpecVector vector,
        out string error)
    {
        error = "";
        return spec.Semantics switch
        {
            CombinationSemantics.ShipFirst => TryValidateSlashVector(
                (Combinator<CommandCatalogIndex>)combinator, vector, out error),
            CombinationSemantics.OverlayWins => TryValidateBindingVector(
                (Combinator<BindingCatalogIndex>)combinator, vector, out error),
            CombinationSemantics.FieldOverlay or CombinationSemantics.SectionReplace =>
                TryValidateWorkspaceVector(
                    (Combinator<WorkspaceDocument>)combinator, vector, out error),
            _ => Fail(vector.Id, $"Unsupported semantics \"{spec.Semantics}\".", out error),
        };
    }

    static bool TryResolveCombinator(PolicySpecDocument spec, out object? combinator, out string error)
    {
        combinator = null;
        error = "";
        combinator = spec.Policy switch
        {
            "slash.ship-first" when spec.Semantics == CombinationSemantics.ShipFirst
                => CommandCatalogCombinators.ShipFirst,
            "binding.overlay-wins" when spec.Semantics == CombinationSemantics.OverlayWins
                => BindingCombinators.OverlayWins,
            "workspace.field-overlay" when spec.Semantics == CombinationSemantics.FieldOverlay
                => WorkspaceCombinators.FieldOverlay,
            _ => null,
        };

        if (combinator is null)
        {
            error = $"No combinator registered for policy \"{spec.Policy}\" / semantics \"{spec.Semantics}\".";
            return false;
        }

        return true;
    }

    static bool TryValidateSlashVector(
        Combinator<CommandCatalogIndex> combinator,
        PolicySpecVector vector,
        out string error)
    {
        error = "";
        if (vector.Baseline is null || vector.Overlay is null || vector.Expect is null)
            return Fail(vector.Id, "Slash vectors require baseline, overlay, and expect.", out error);

        var baseline = BuildSlashIndex(vector.Baseline.Value);
        var overlay = BuildSlashIndex(vector.Overlay.Value);
        var merged = combinator(baseline, overlay);
        var expect = vector.Expect.Value.Deserialize<SlashExpectWire>(PolicySpecLoader.JsonOptions);
        if (expect?.Paths is null)
            return Fail(vector.Id, "expect.paths is required.", out error);

        foreach (var (path, commandId) in expect.Paths)
        {
            if (!merged.TryGet(path, out var route))
                return Fail(vector.Id, $"expected path \"{path}\" missing after merge.", out error);

            if (!string.Equals(route.CommandId, commandId, StringComparison.Ordinal))
            {
                return Fail(
                    vector.Id,
                    $"path \"{path}\": expected commandId \"{commandId}\", got \"{route.CommandId}\".",
                    out error);
            }
        }

        return true;
    }

    static bool TryValidateBindingVector(
        Combinator<BindingCatalogIndex> combinator,
        PolicySpecVector vector,
        out string error)
    {
        error = "";
        if (vector.Baseline is null || vector.Overlay is null || vector.Expect is null)
            return Fail(vector.Id, "Binding vectors require baseline, overlay, and expect.", out error);

        var baseline = BuildBindingIndex(vector.Baseline.Value);
        var overlay = BuildBindingIndex(vector.Overlay.Value);
        var merged = combinator(baseline, overlay);
        var expect = vector.Expect.Value.Deserialize<BindingExpectWire>(PolicySpecLoader.JsonOptions);
        if (expect?.Bindings is null)
            return Fail(vector.Id, "expect.bindings is required.", out error);

        foreach (var (key, gesture) in expect.Bindings)
        {
            if (!merged.TryGetByKey(key, out var entry))
                return Fail(vector.Id, $"expected binding key \"{key}\" missing after merge.", out error);

            if (!string.Equals(entry.Descriptor.GestureWire, gesture, StringComparison.Ordinal))
            {
                return Fail(
                    vector.Id,
                    $"key \"{key}\": expected gesture \"{gesture}\", got \"{entry.Descriptor.GestureWire}\".",
                    out error);
            }
        }

        return true;
    }

    static bool TryValidateWorkspaceVector(
        Combinator<WorkspaceDocument> combinator,
        PolicySpecVector vector,
        out string error)
    {
        error = "";
        if (vector.Baseline is null || vector.Overlay is null || vector.Expect is null)
            return Fail(vector.Id, "Workspace vectors require baseline, overlay, and expect.", out error);

        var baseline = vector.Baseline.Value.Deserialize<WorkspaceDocument>(PolicySpecLoader.JsonOptions)
            ?? new WorkspaceDocument();
        var overlay = vector.Overlay.Value.Deserialize<WorkspaceDocument>(PolicySpecLoader.JsonOptions)
            ?? new WorkspaceDocument();
        var expect = vector.Expect.Value.Deserialize<WorkspaceDocument>(PolicySpecLoader.JsonOptions)
            ?? new WorkspaceDocument();

        var merged = combinator(baseline, overlay);
        if (!WorkspaceExpectMatches(merged, expect, out var mismatch))
            return Fail(vector.Id, mismatch, out error);

        return true;
    }

    static CommandCatalogIndex BuildSlashIndex(JsonElement layer)
    {
        var wire = layer.Deserialize<SlashLayerWire>(PolicySpecLoader.JsonOptions);
        var descriptors = (wire?.Paths ?? [])
            .Select(path => new CommandDescriptor
            {
                Domain = "",
                Object = "",
                Intent = "",
                Path = path.Path,
                CommandId = path.CommandId,
            })
            .ToArray();

        return CommandCatalogIndex.FromDescriptors(descriptors);
    }

    static BindingCatalogIndex BuildBindingIndex(JsonElement layer)
    {
        var wire = layer.Deserialize<BindingLayerWire>(PolicySpecLoader.JsonOptions);
        var descriptors = (wire?.Bindings ?? [])
            .Select(entry => new BindingDescriptor
            {
                BindingKey = entry.Key,
                GestureWire = entry.Gesture,
                TargetKind = BindingTargetKind.Command,
            })
            .ToArray();

        return BindingCatalogIndex.FromDescriptors(descriptors);
    }

    static bool WorkspaceExpectMatches(WorkspaceDocument actual, WorkspaceDocument expect, out string mismatch)
    {
        mismatch = "";
        var actualSection = actual.Workspace;
        var expectSection = expect.Workspace;
        if (expectSection is null)
            return true;

        if (actualSection is null)
        {
            mismatch = "expected workspace section missing after merge.";
            return false;
        }

        if (expectSection.Adr is not null)
        {
            var actualAdr = actualSection.Adr;
            if (actualAdr is null)
            {
                mismatch = "expected workspace.adr missing after merge.";
                return false;
            }

            if (expectSection.Adr.RootDir is not null
                && !string.Equals(actualAdr.RootDir, expectSection.Adr.RootDir, StringComparison.Ordinal))
            {
                mismatch = $"workspace.adr.root_dir: expected \"{expectSection.Adr.RootDir}\", got \"{actualAdr.RootDir}\".";
                return false;
            }

            if (expectSection.Adr.MaxRelated is not null && actualAdr.MaxRelated != expectSection.Adr.MaxRelated)
            {
                mismatch = $"workspace.adr.max_related: expected {expectSection.Adr.MaxRelated}, got {actualAdr.MaxRelated}.";
                return false;
            }
        }

        if (expectSection.Features is not null)
        {
            var actualFeatures = actualSection.Features?.Feature ?? [];
            var expectFeatures = expectSection.Features.Feature;
            if (actualFeatures.Count != expectFeatures.Count)
            {
                mismatch = $"workspace.features.feature count: expected {expectFeatures.Count}, got {actualFeatures.Count}.";
                return false;
            }

            for (var i = 0; i < expectFeatures.Count; i++)
            {
                if (!string.Equals(actualFeatures[i].Id, expectFeatures[i].Id, StringComparison.Ordinal))
                {
                    mismatch = $"workspace.features.feature[{i}].id: expected \"{expectFeatures[i].Id}\", got \"{actualFeatures[i].Id}\".";
                    return false;
                }
            }
        }

        return true;
    }

    static bool Fail(string vectorId, string message, out string error)
    {
        error = $"vector \"{vectorId}\": {message}";
        return false;
    }
}
