#nullable enable

namespace AIGuiders.Platform.CommandPlane;

public static class SlashConstructorSessionExtensions
{
    public static SlashCompletionResult GetSlashCompletionResult(
        this ArgConstructorSession session,
        ValueConstructorNavigator navigator,
        string partial,
        LocaleInputProfile? profile = null)
    {
        if (session.Draft is null)
        {
            return new SlashCompletionResult([], new SlashInputGuidance(
                "/",
                "",
                "",
                InvocationLinePhase.Path));
        }

        var (items, argGuidance) = session.GetArgCompletion(navigator, partial, profile);
        var breadcrumb = BuildConstructorBreadcrumb(session.Draft);
        return new SlashCompletionResult(items, argGuidance.ToSlashGuidance(breadcrumb));
    }

    static string BuildConstructorBreadcrumb(ArgConstructorDraft draft)
    {
        var parts = new List<string> { "/" + draft.CanonicalPath };
        if (!string.IsNullOrWhiteSpace(draft.DisplayBuffer))
        {
            parts.Add(draft.DisplayBuffer);
        }

        return string.Join(" › ", parts);
    }
}

static class ArgInputGuidanceSlashExtensions
{
    public static SlashInputGuidance ToSlashGuidance(this ArgInputGuidance guidance, string breadcrumb) =>
        new(
            breadcrumb,
            guidance.Placeholder,
            guidance.Hint,
            guidance.Phase,
            guidance.Mechanic,
            guidance.CanonicalPath,
            guidance.ArgTailKind,
            guidance.ReadyWire,
            guidance.DisplayTail);
}
