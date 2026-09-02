#nullable enable

namespace AIGuiders.Platform.Execution.CommandPlane;

public static class SlashPrefixArmProjection
{
    public static SlashCompletionResult? ToSlashCompletionResult(
        PrefixArmResult result,
        ArgConstructorSession? session,
        ValueConstructorNavigator? navigator,
        string partial,
        LocaleInputProfile? localeProfile)
    {
        switch (result)
        {
            case PrefixArmReadyResult ready:
                var breadcrumb = "/" + ready.CanonicalPath + " › " + ready.DisplayTail;
                return new SlashCompletionResult(
                    [],
                    new SlashInputGuidance(
                        breadcrumb,
                        "Press Enter to run",
                        ready.Help,
                        InvocationLinePhase.Ready,
                        CanonicalPath: ready.CanonicalPath,
                        ArgTailKind: ready.ArgTailKind,
                        ReadyWire: ready.Wire,
                        DisplayTail: ready.DisplayTail));
            case PrefixArmContinuedResult when session is not null && navigator is not null:
                return session.ToSlashCompletionResult(navigator, partial, localeProfile);
            default:
                return null;
        }
    }
}
