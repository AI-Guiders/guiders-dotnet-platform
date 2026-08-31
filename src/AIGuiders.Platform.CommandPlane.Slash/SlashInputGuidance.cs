using AIGuiders.Platform.IntermediateRepresentation.Command;
#nullable enable

using AIGuiders.Platform.CommandPlane.ArgSuggestions;

namespace AIGuiders.Platform.CommandPlane;

/// <summary>Slash-surface guidance: line phase + optional arg mechanic (GUIDERS-ADR-0043).</summary>
public sealed record SlashInputGuidance(
    string Breadcrumb,
    string Placeholder,
    string Hint,
    InvocationLinePhase Phase,
    ArgMechanic? ArgMechanic = null,
    string? CanonicalPath = null,
    string ArgTailKind = "",
    string? ReadyWire = null,
    string? DisplayTail = null)
{
    /// <summary>Wire/conformance label — Path, Ready, or mechanic name during Arg phase.</summary>
    public string Mode => Phase switch
    {
        InvocationLinePhase.Path => nameof(InvocationLinePhase.Path),
        InvocationLinePhase.Ready => nameof(InvocationLinePhase.Ready),
        InvocationLinePhase.Arg => ArgMechanic!.ToString(),
        _ => Phase.ToString(),
    };
}

/// <summary>Items + input guidance for slash surfaces.</summary>
public sealed record SlashCompletionResult(
    IReadOnlyList<ArgCompletionItem> Items,
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
        ArgConstructorSession? constructorSession) =>
        GetResult(catalog, typedBody, suggestionBroker, constructorSession, options: null);

    public static SlashCompletionResult GetResult(
        CommandCatalogIndex catalog,
        string typedBody,
        ICommandArgSuggestionBroker? suggestionBroker,
        ArgConstructorSession? constructorSession,
        SlashCompletionOptions? options)
    {
        var culture = options?.Culture ?? CultureAmbient.Current;
        var profile = LocaleInputProfile.FromCulture(culture);

        if (constructorSession?.IsActive == true)
        {
            var partial = constructorSession.TypedArgTail;
            return constructorSession.ToSlashCompletionResult(
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
                : LocaleInputProfile.FromCulture(options.Culture);
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
            return constructorSession.ToSlashCompletionResult(
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
        IReadOnlyList<ArgCompletionItem> items,
        IReadOnlyList<IPrefixArmProfile>? prefixArmProfiles = null,
        LocaleInputProfile? localeProfile = null)
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
                    breadcrumb,
                    "Press Enter to run",
                    route.Help,
                    InvocationLinePhase.Ready,
                    CanonicalPath: line.CanonicalPath,
                    ArgTailKind: argTailKind,
                    ReadyWire: line.ArgTail.Trim(),
                    DisplayTail: line.ArgTail.Trim());
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
        IReadOnlyList<ArgCompletionItem> items,
        string breadcrumb,
        string argTailKind,
        IReadOnlyList<IPrefixArmProfile>? prefixArmProfiles,
        LocaleInputProfile? localeProfile)
    {
        var partial = line.ArgTail.Trim();
        if (partial.Length > 0
            && prefixArmProfiles is { Count: > 0 }
            && PrefixArmCoordinator.AnyProfileMatches(prefixArmProfiles, partial, route.ToPrefixArmSite()))
        {
            return new SlashInputGuidance(
                breadcrumb,
                localeProfile?.InputPlaceholder ?? "Type value",
                route.ArgHint ?? "Continue typing — prefix arms constructor or completes wire",
                InvocationLinePhase.Arg,
                ArgMechanic.TypedInput,
                line.CanonicalPath,
                argTailKind,
                DisplayTail: partial);
        }

        if (localeProfile is not null
            && partial.Length > 0
            && route.ResolvedConstructors.Count > 0
            && LocaleDateParser.TryParse(partial, localeProfile, out _, out var completeness)
            && completeness is LocaleDateCompleteness.Partial or LocaleDateCompleteness.MonthYear)
        {
            return new SlashInputGuidance(
                breadcrumb,
                localeProfile.InputPlaceholder,
                route.ArgHint ?? "Type date in locale format",
                InvocationLinePhase.Arg,
                ArgMechanic.TypedInput,
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
            var mechanic = partial.Length > 0 && hasConstructors
                ? ArgMechanic.TypedInput
                : ArgMechanic.Picker;
            return new SlashInputGuidance(
                breadcrumb,
                placeholder,
                hint,
                InvocationLinePhase.Arg,
                mechanic,
                line.CanonicalPath,
                argTailKind,
                DisplayTail: partial.Length > 0 ? partial : null);
        }

        return route.ArgTailKind switch
        {
            CommandArgTailKind.Required => new SlashInputGuidance(
                breadcrumb,
                FormatFreeTextPlaceholder(route.ArgHint),
                route.ArgHint ?? "Type the required argument and press Enter",
                InvocationLinePhase.Arg,
                ArgMechanic.FreeText,
                line.CanonicalPath,
                argTailKind),
            CommandArgTailKind.Optional => new SlashInputGuidance(
                breadcrumb,
                route.ArgHint ?? "Optional argument — Enter to run",
                route.ArgHint ?? "Add an argument or press Enter to run without it",
                InvocationLinePhase.Arg,
                ArgMechanic.Optional,
                line.CanonicalPath,
                argTailKind),
            _ => new SlashInputGuidance(
                breadcrumb,
                "Continue typing the command path",
                route.Help,
                InvocationLinePhase.Path,
                CanonicalPath: line.CanonicalPath,
                ArgTailKind: argTailKind),
        };
    }

    static SlashInputGuidance ResolvePathGuidance(string body, IReadOnlyList<ArgCompletionItem> items)
    {
        var breadcrumb = body.Length == 0 ? "/" : "/" + body.TrimEnd();
        if (items.Count > 0)
        {
            var next = items[0].StepSegment ?? items[0].CommandPath.TrimStart('/');
            return new SlashInputGuidance(
                breadcrumb,
                $"Next: {next}",
                items[0].Help,
                InvocationLinePhase.Path,
                CanonicalPath: items[0].CommandPath.TrimStart('/'),
                ArgTailKind: nameof(CommandArgTailKind.None));
        }

        return new SlashInputGuidance(
            breadcrumb,
            "Type a command path",
            "Start typing — Tab completes the next segment",
            InvocationLinePhase.Path,
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
