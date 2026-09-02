#nullable enable

namespace AIGuiders.Platform.Execution.CommandPlane;

public static class ArgConstructorSessionExtensions
{
    public static (IReadOnlyList<ArgCompletionItem> Items, ArgInputGuidance Guidance) GetArgCompletion(
        this ArgConstructorSession session,
        ValueConstructorNavigator navigator,
        string partial,
        LocaleInputProfile? profile = null)
    {
        if (session.Draft is null)
        {
            return ([], new ArgInputGuidance(
                InvocationLinePhase.Path,
                null,
                "",
                "",
                null,
                ""));
        }

        var items = navigator.GetSuggestions(session.Draft, partial);
        var guidance = navigator.BuildArgGuidance(session.Draft, profile);
        return (items, guidance);
    }
}
