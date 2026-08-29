#nullable enable

namespace AIGuiders.Platform.CommandPlane;

/// <summary>Human-facing mode + hints for slash input (breadcrumb, placeholder).</summary>
public sealed record SlashInputGuidance(
    SlashInputMode Mode,
    string Breadcrumb,
    string Placeholder,
    string Hint,
    string? CanonicalPath = null,
    string ArgTailKind = "");

/// <summary>Items + input guidance for slash surfaces.</summary>
public sealed record SlashCompletionResult(
    IReadOnlyList<SlashCompletionItem> Items,
    SlashInputGuidance Guidance);

public static class SlashCompletion
{
    public static SlashCompletionResult GetResult(
        SlashCatalogIndex catalog,
        string typedBody,
        ISlashPickerChoiceSource? pickerSource = null)
    {
        var items = SlashStepCompletion.GetSuggestions(catalog, typedBody, pickerSource);
        var guidance = SlashInputGuidanceResolver.Resolve(catalog, typedBody, pickerSource, items);
        return new SlashCompletionResult(items, guidance);
    }
}

static class SlashInputGuidanceResolver
{
    public static SlashInputGuidance Resolve(
        SlashCatalogIndex catalog,
        string typedBody,
        ISlashPickerChoiceSource? pickerSource,
        IReadOnlyList<SlashCompletionItem> items)
    {
        var body = typedBody.TrimStart();
        if (SlashLineResolver.TryResolveBody(body, catalog, out var line)
            && catalog.TryGet(line.CanonicalPath, out var route))
        {
            var breadcrumb = BuildBreadcrumb(line.CanonicalPath, line.ArgTail);
            var argTailKind = route.ArgTailKind.ToString();

            if (SlashArgCompletion.ShouldComplete(line, route) && AwaitingArgInput(line, route))
            {
                return ResolveArgGuidance(line, route, pickerSource, items, breadcrumb, argTailKind);
            }

            if (line.IsRunnable)
            {
                return new SlashInputGuidance(
                    SlashInputMode.Ready,
                    breadcrumb,
                    "Press Enter to run",
                    route.Help,
                    line.CanonicalPath,
                    argTailKind);
            }

            if (SlashArgCompletion.ShouldComplete(line, route))
            {
                return ResolveArgGuidance(line, route, pickerSource, items, breadcrumb, argTailKind);
            }
        }

        return ResolvePathGuidance(body, items);
    }

    static bool AwaitingArgInput(SlashLineResolver.SlashLineResolution line, SlashRouteEntry route) =>
        route.ArgTailKind switch
        {
            SlashArgTailKind.Required => !line.HasArgTailContent,
            SlashArgTailKind.Picker => !line.HasArgTailContent,
            SlashArgTailKind.Optional => line.EndsWithSpaceAfterPath && !line.HasArgTailContent,
            _ => false,
        };

    static SlashInputGuidance ResolveArgGuidance(
        SlashLineResolver.SlashLineResolution line,
        SlashRouteEntry route,
        ISlashPickerChoiceSource? pickerSource,
        IReadOnlyList<SlashCompletionItem> items,
        string breadcrumb,
        string argTailKind)
    {
        var hasPickerSurface = route.ArgTailKind == SlashArgTailKind.Picker
                               || route.ResolvedPickerChoices.Count > 0
                               || SlashArgTailPolicy.ExtractPickerId(route.ArgTail) is not null;

        if (hasPickerSurface)
        {
            var hasChoices = items.Count > 0
                             || SlashArgCompletion.HasChoices(route, line.ArgTail.Trim(), pickerSource);
            var hint = route.ArgHint
                       ?? (hasChoices
                           ? "Choose a value — Tab to insert"
                           : "Type to search choices");
            var placeholder = route.ArgHint
                              ?? (hasChoices ? "Pick a value or type to filter" : "Type to filter choices");
            return new SlashInputGuidance(
                SlashInputMode.Picker,
                breadcrumb,
                placeholder,
                hint,
                line.CanonicalPath,
                argTailKind);
        }

        return route.ArgTailKind switch
        {
            SlashArgTailKind.Required => new SlashInputGuidance(
                SlashInputMode.FreeText,
                breadcrumb,
                FormatFreeTextPlaceholder(route.ArgHint),
                route.ArgHint ?? "Type the required argument and press Enter",
                line.CanonicalPath,
                argTailKind),
            SlashArgTailKind.Optional => new SlashInputGuidance(
                SlashInputMode.Optional,
                breadcrumb,
                route.ArgHint ?? "Optional argument — Enter to run",
                route.ArgHint ?? "Add an argument or press Enter to run without it",
                line.CanonicalPath,
                argTailKind),
            _ => new SlashInputGuidance(
                SlashInputMode.Path,
                breadcrumb,
                "Continue typing the command path",
                route.Help,
                line.CanonicalPath,
                argTailKind),
        };
    }

    static SlashInputGuidance ResolvePathGuidance(string body, IReadOnlyList<SlashCompletionItem> items)
    {
        var breadcrumb = body.Length == 0 ? "/" : "/" + body.TrimEnd();
        if (items.Count > 0)
        {
            var next = items[0].StepSegment ?? items[0].SlashPath.TrimStart('/');
            return new SlashInputGuidance(
                SlashInputMode.Path,
                breadcrumb,
                $"Next: {next}",
                items[0].Help,
                items[0].SlashPath.TrimStart('/'),
                nameof(SlashArgTailKind.None));
        }

        return new SlashInputGuidance(
            SlashInputMode.Path,
            breadcrumb,
            "Type a command path",
            "Start typing — Tab completes the next segment",
            CanonicalPath: null,
            ArgTailKind: nameof(SlashArgTailKind.None));
    }

    static string FormatFreeTextPlaceholder(string? argHint) =>
        string.IsNullOrWhiteSpace(argHint)
            ? "Type value (free text)"
            : $"{argHint.Trim()} (free text)";

    static string BuildBreadcrumb(string canonicalPath, string argTail)
    {
        var segments = canonicalPath
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
        if (!string.IsNullOrWhiteSpace(argTail))
        {
            segments.Add(argTail.Trim());
        }

        return "/" + string.Join(" › ", segments);
    }
}
