#nullable enable

namespace AIGuiders.Platform.CommandPlane;

public static class SlashConstructorSessionExtensions
{
    public static SlashCompletionResult GetSlashCompletionResult(
        this SlashConstructorSession session,
        SlashValueConstructorNavigator navigator,
        string partial,
        SlashLocaleInputProfile? profile = null)
    {
        if (session.Draft is null)
        {
            return new SlashCompletionResult([], new SlashInputGuidance(
                SlashInputMode.Path,
                "/",
                "",
                "",
                null,
                ""));
        }

        var items = navigator.GetSuggestions(session.Draft, partial);
        var guidance = navigator.BuildSlashGuidance(session.Draft, profile);
        return new SlashCompletionResult(items, guidance);
    }
}
