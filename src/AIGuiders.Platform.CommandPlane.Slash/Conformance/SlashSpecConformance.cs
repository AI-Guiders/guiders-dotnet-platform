using AIGuiders.Platform.IntermediateRepresentation.Command;
#nullable enable

using AIGuiders.Platform.CommandPlane.ArgSuggestions;

namespace AIGuiders.Platform.CommandPlane.Conformance;

public static class SlashSpecConformance
{
    public static IReadOnlyList<string> ValidateDocument(SlashSpecDocument spec)
    {
        var errors = new List<string>();
        var suggestionBroker = BuildStubBroker(spec.PickerStubs);

        foreach (var vector in spec.Vectors)
        {
            if (!TryValidateVector(spec, vector, suggestionBroker, out var error))
                errors.Add($"[{vector.Id}] {error}");
        }

        return errors;
    }

    public static bool TryValidateVector(
        SlashSpecDocument spec,
        SlashSpecVector vector,
        ICommandArgSuggestionBroker suggestionBroker,
        out string error)
    {
        error = "";

        if (!spec.Catalogs.TryGetValue(vector.Catalog, out var catalogEntries))
        {
            error = $"unknown catalog \"{vector.Catalog}\".";
            return false;
        }

        var catalog = SlashSpecLoader.BuildCatalog(catalogEntries);

        if (vector.Expect.Suggestions is not null)
        {
            var items = SlashStepCompletion.GetSuggestions(catalog, vector.Body, suggestionBroker);
            if (!TryValidateSuggestions(vector.Expect.Suggestions, items, out error))
                return false;
        }

        if (vector.Expect.Guidance is not null)
        {
            var result = SlashCompletion.GetResult(catalog, vector.Body, suggestionBroker);
            if (!TryValidateGuidance(vector.Expect.Guidance, result.Guidance, out error))
                return false;
        }

        if (vector.Expect.Suggestions is null && vector.Expect.Guidance is null)
        {
            error = "expect must include suggestions and/or guidance.";
            return false;
        }

        return true;
    }

    static bool TryValidateSuggestions(
        SlashSpecSuggestionsExpectation expect,
        IReadOnlyList<SlashCompletionItem> actual,
        out string error)
    {
        error = "";

        if (expect.Items is { Count: > 0 })
        {
            if (actual.Count != expect.Items.Count)
            {
                error = $"expected {expect.Items.Count} suggestion items, got {actual.Count}.";
                return false;
            }

            for (var i = 0; i < expect.Items.Count; i++)
            {
                if (!ItemsEqual(expect.Items[i], actual[i], out error))
                {
                    error = $"item {i}: {error}";
                    return false;
                }
            }

            return true;
        }

        if (expect.StepSegments is { Count: > 0 })
        {
            var actualSegments = actual
                .Select(i => i.StepSegment ?? "")
                .Where(s => s.Length > 0)
                .OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
                .ToList();
            var expectedSegments = expect.StepSegments
                .OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!actualSegments.SequenceEqual(expectedSegments, StringComparer.OrdinalIgnoreCase))
            {
                error = $"expected step segments [{string.Join(", ", expectedSegments)}], got [{string.Join(", ", actualSegments)}].";
                return false;
            }
        }

        return true;
    }

    static bool ItemsEqual(SlashSpecCompletionItem expected, SlashCompletionItem actual, out string error)
    {
        error = "";

        if (!string.Equals(expected.Kind, actual.Kind.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            error = $"kind expected {expected.Kind}, got {actual.Kind}.";
            return false;
        }

        if (!string.Equals(expected.InsertText, actual.InsertText, StringComparison.Ordinal))
        {
            error = $"insertText expected \"{expected.InsertText}\", got \"{actual.InsertText}\".";
            return false;
        }

        if (!string.Equals(expected.SlashPath, actual.SlashPath, StringComparison.Ordinal))
        {
            error = $"slashPath expected \"{expected.SlashPath}\", got \"{actual.SlashPath}\".";
            return false;
        }

        if (!string.Equals(expected.Help, actual.Help, StringComparison.Ordinal))
        {
            error = $"help expected \"{expected.Help}\", got \"{actual.Help}\".";
            return false;
        }

        if (!NullableEquals(expected.Group, actual.Group))
        {
            error = $"group expected \"{expected.Group}\", got \"{actual.Group}\".";
            return false;
        }

        if (!NullableEquals(expected.StepSegment, actual.StepSegment))
        {
            error = $"stepSegment expected \"{expected.StepSegment}\", got \"{actual.StepSegment}\".";
            return false;
        }

        if (!NullableEquals(expected.PickValue, actual.PickValue))
        {
            error = $"pickValue expected \"{expected.PickValue}\", got \"{actual.PickValue}\".";
            return false;
        }

        return true;
    }

    static bool TryValidateGuidance(
        SlashSpecGuidanceExpectation expect,
        SlashInputGuidance actual,
        out string error)
    {
        error = "";

        if (!string.Equals(expect.Mode, actual.Mode.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            error = $"mode expected {expect.Mode}, got {actual.Mode}.";
            return false;
        }

        if (expect.BreadcrumbContains is not null
            && !actual.Breadcrumb.Contains(expect.BreadcrumbContains, StringComparison.OrdinalIgnoreCase))
        {
            error = $"breadcrumb \"{actual.Breadcrumb}\" does not contain \"{expect.BreadcrumbContains}\".";
            return false;
        }

        if (expect.PlaceholderContains is not null
            && !actual.Placeholder.Contains(expect.PlaceholderContains, StringComparison.OrdinalIgnoreCase))
        {
            error = $"placeholder \"{actual.Placeholder}\" does not contain \"{expect.PlaceholderContains}\".";
            return false;
        }

        if (expect.Hint is not null && !string.Equals(expect.Hint, actual.Hint, StringComparison.Ordinal))
        {
            error = $"hint expected \"{expect.Hint}\", got \"{actual.Hint}\".";
            return false;
        }

        return true;
    }

    static bool NullableEquals(string? expected, string? actual) =>
        string.Equals(expected ?? "", actual ?? "", StringComparison.Ordinal);

    static ICommandArgSuggestionBroker BuildStubBroker(
        IReadOnlyDictionary<string, SlashSpecPickerStub>? stubs)
    {
        if (stubs is null || stubs.Count == 0)
        {
            return new StubArgSuggestionBroker(new Dictionary<string, IReadOnlyList<CommandPickerChoice>>());
        }

        var map = stubs.ToDictionary(
            static kv => kv.Key,
            static kv => (IReadOnlyList<CommandPickerChoice>)kv.Value.Choices
                .Select(static c => new CommandPickerChoice
                {
                    Value = c.Value,
                    Label = c.Label,
                    Hint = c.Hint,
                })
                .ToList());

        return new StubArgSuggestionBroker(map);
    }
}
