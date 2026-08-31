using AIGuiders.Platform.IntermediateRepresentation.Command;
#nullable enable

using AIGuiders.Platform.CommandPlane.ArgSuggestions;

namespace AIGuiders.Platform.CommandPlane;

/// <summary>Human-facing mode + hints for slash input (breadcrumb, placeholder).</summary>
public sealed record SlashInputGuidance(
    SlashInputMode Mode,
    string Breadcrumb,
    string Placeholder,
    string Hint,
    string? CanonicalPath = null,
    string ArgTailKind = "",
    string? ReadyWire = null,
    string? DisplayTail = null);

/// <summary>Items + input guidance for slash surfaces.</summary>
public sealed record SlashCompletionResult(
    IReadOnlyList<SlashCompletionItem> Items,
    SlashInputGuidance Guidance);

public static class SlashCompletion
{
    public static SlashCompletionResult GetResult(
        CommandCatalogIndex catalog,
        string typedBody,
        ICommandArgSuggestionBroker? suggestionBroker = null) =>
        GetResult(catalog, typedBody, suggestionBroker, constructorSession: null);

    public static SlashCompletionResult GetResult(
        CommandCatalogIndex catalog,
        string typedBody,
        ICommandArgSuggestionBroker? suggestionBroker,
        SlashConstructorSession? constructorSession) =>
        GetResult(catalog, typedBody, suggestionBroker, constructorSession, options: null);

    public static SlashCompletionResult GetResult(
        CommandCatalogIndex catalog,
        string typedBody,
        ICommandArgSuggestionBroker? suggestionBroker,
        SlashConstructorSession? constructorSession,
        SlashCompletionOptions? options)
    {
        var culture = options?.Culture ?? SlashCultureAmbient.Current;
        var profile = SlashLocaleInputProfile.FromCulture(culture);

        if (constructorSession?.IsActive == true)
        {
            var partial = constructorSession.TypedArgTail;
            return constructorSession.GetSlashCompletionResult(
                constructorSession.Navigator,
                partial,
                profile);
        }

        if (options?.ConstructorRegistry is not null
            && options.SegmentProvider is not null
            && constructorSession is not null
            && options.PrefixArmProfiles.Count > 0
            && SlashLineResolver.TryResolveBody(typedBody.TrimStart(), catalog, out var line)
            && catalog.TryGet(line.CanonicalPath, out var route)
            && line.HasArgTailContent)
        {
            var navigator = constructorSession.Navigator;
            var coordinator = new PrefixArmCoordinator(navigator, options.ConstructorRegistry);
            var localeProfile = options.Culture is null
                ? null
                : SlashLocaleInputProfile.FromCulture(options.Culture);
            var site = route.ToPrefixArmSite();
            if (coordinator.TryHandle(
                    line.CanonicalPath,
                    line.ArgTail,
                    site,
                    constructorSession,
                    options.PrefixArmProfiles,
                    localeProfile,
                    out var pacResult)
                && pacResult is not null
                && SlashPrefixArmProjection.ToSlashCompletionResult(
                    pacResult,
                    constructorSession,
                    navigator,
                    line.ArgTail,
                    localeProfile) is { } slashResult)
            {
                return slashResult;
            }
        }

        if (constructorSession?.IsActive == true)
        {
            return constructorSession.GetSlashCompletionResult(
                constructorSession.Navigator,
                "",
                profile);
        }

        var items = SlashStepCompletion.GetSuggestions(catalog, typedBody, suggestionBroker);
        var guidance = SlashInputGuidanceResolver.Resolve(
            catalog,
            typedBody,
            suggestionBroker,
            items,
            options?.PrefixArmProfiles,
            options?.Culture is null ? null : profile);
        return new SlashCompletionResult(items, guidance);
    }
}

static class SlashInputGuidanceResolver
{
    public static SlashInputGuidance Resolve(
        CommandCatalogIndex catalog,
        string typedBody,
        ICommandArgSuggestionBroker? suggestionBroker,
        IReadOnlyList<SlashCompletionItem> items,
        IReadOnlyList<IPrefixArmProfile>? prefixArmProfiles = null,
        SlashLocaleInputProfile? localeProfile = null)
    {
        var body = typedBody.TrimStart();
        if (SlashLineResolver.TryResolveBody(body, catalog, out var line)
            && catalog.TryGet(line.CanonicalPath, out var route))
        {
            var breadcrumb = BuildBreadcrumb(line.CanonicalPath, line.ArgTail);
            var argTailKind = route.ArgTailKind.ToString();

            if (SlashArgCompletion.ShouldComplete(line, route) && AwaitingArgInput(line, route))
            {
                return ResolveArgGuidance(line, route, suggestionBroker, items, breadcrumb, argTailKind, prefixArmProfiles, localeProfile);
            }

            if (line.IsRunnable)
            {
                return new SlashInputGuidance(
                    SlashInputMode.Ready,
                    breadcrumb,
                    "Press Enter to run",
                    route.Help,
                    line.CanonicalPath,
                    argTailKind,
                    line.ArgTail.Trim(),
                    line.ArgTail.Trim());
            }

            if (SlashArgCompletion.ShouldComplete(line, route))
            {
                return ResolveArgGuidance(line, route, suggestionBroker, items, breadcrumb, argTailKind, prefixArmProfiles, localeProfile);
            }
        }

        return ResolvePathGuidance(body, items);
    }

    static bool AwaitingArgInput(SlashLineResolver.SlashLineResolution line, CatalogRouteEntry route) =>
        route.ArgTailKind switch
        {
            CommandArgTailKind.Required => !line.HasArgTailContent,
            CommandArgTailKind.Picker => !line.HasArgTailContent,
            CommandArgTailKind.Optional => line.EndsWithSpaceAfterPath && !line.HasArgTailContent,
            _ => false,
        };

    static SlashInputGuidance ResolveArgGuidance(
        SlashLineResolver.SlashLineResolution line,
        CatalogRouteEntry route,
        ICommandArgSuggestionBroker? suggestionBroker,
        IReadOnlyList<SlashCompletionItem> items,
        string breadcrumb,
        string argTailKind,
        IReadOnlyList<IPrefixArmProfile>? prefixArmProfiles,
        SlashLocaleInputProfile? localeProfile)
    {
        var partial = line.ArgTail.Trim();
        if (partial.Length > 0
            && prefixArmProfiles is { Count: > 0 }
            && PrefixArmCoordinator.AnyProfileMatches(prefixArmProfiles, partial, route.ToPrefixArmSite()))
        {
            return new SlashInputGuidance(
                SlashInputMode.TypedInput,
                breadcrumb,
                localeProfile?.InputPlaceholder ?? "Type value",
                route.ArgHint ?? "Continue typing — prefix arms constructor or completes wire",
                line.CanonicalPath,
                argTailKind,
                DisplayTail: partial);
        }

        if (localeProfile is not null
            && partial.Length > 0
            && route.ResolvedConstructors.Count > 0
            && SlashLocaleDateParser.TryParse(partial, localeProfile, out _, out var completeness)
            && completeness is SlashLocaleDateCompleteness.Partial or SlashLocaleDateCompleteness.MonthYear)
        {
            return new SlashInputGuidance(
                SlashInputMode.TypedInput,
                breadcrumb,
                localeProfile.InputPlaceholder,
                route.ArgHint ?? "Type date in locale format",
                line.CanonicalPath,
                argTailKind,
                DisplayTail: partial);
        }

        var hasPickerSurface = route.ArgTailKind == CommandArgTailKind.Picker
                               || route.ResolvedPickerChoices.Count > 0
                               || CommandArgTailPolicy.ExtractSuggestionId(route.ArgTail) is not null;

        if (hasPickerSurface)
        {
            var hasChoices = items.Count > 0
                             || SlashArgCompletion.HasChoices(route, partial, suggestionBroker);
            var hasConstructors = route.ResolvedConstructors.Count > 0;
            var hint = route.ArgHint
                       ?? (hasChoices || hasConstructors
                           ? "Choose a value — Tab to insert, or type locale date"
                           : "Type locale date or search choices");
            var placeholder = localeProfile?.InputPlaceholder
                              ?? route.ArgHint
                              ?? (hasChoices || hasConstructors
                                  ? "Pick a value or type locale date"
                                  : "Type to filter choices");
            var mode = partial.Length > 0 && hasConstructors
                ? SlashInputMode.TypedInput
                : SlashInputMode.Picker;
            return new SlashInputGuidance(
                mode,
                breadcrumb,
                placeholder,
                hint,
                line.CanonicalPath,
                argTailKind,
                DisplayTail: partial.Length > 0 ? partial : null);
        }

        return route.ArgTailKind switch
        {
            CommandArgTailKind.Required => new SlashInputGuidance(
                SlashInputMode.FreeText,
                breadcrumb,
                FormatFreeTextPlaceholder(route.ArgHint),
                route.ArgHint ?? "Type the required argument and press Enter",
                line.CanonicalPath,
                argTailKind),
            CommandArgTailKind.Optional => new SlashInputGuidance(
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
                nameof(CommandArgTailKind.None));
        }

        return new SlashInputGuidance(
            SlashInputMode.Path,
            breadcrumb,
            "Type a command path",
            "Start typing — Tab completes the next segment",
            CanonicalPath: null,
            ArgTailKind: nameof(CommandArgTailKind.None));
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
