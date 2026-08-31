#nullable enable

namespace AIGuiders.Platform.CommandPlane;

public static class SlashPrefixArmProjection
{
    public static SlashCompletionResult? ToSlashCompletionResult(
        PrefixArmResult result,
        SlashConstructorSession? session,
        SlashValueConstructorNavigator? navigator,
        string partial,
        SlashLocaleInputProfile? localeProfile)
    {
        switch (result)
        {
            case PrefixArmReadyResult ready:
                var breadcrumb = "/" + ready.CanonicalPath + " › " + ready.DisplayTail;
                return new SlashCompletionResult(
                    [],
                    new SlashInputGuidance(
                        SlashInputMode.Ready,
                        breadcrumb,
                        "Press Enter to run",
                        ready.Help,
                        ready.CanonicalPath,
                        ready.ArgTailKind,
                        ready.Wire,
                        ready.DisplayTail));
            case PrefixArmContinuedResult when session is not null && navigator is not null:
                return session.GetSlashCompletionResult(navigator, partial, localeProfile);
            default:
                return null;
        }
    }
}
