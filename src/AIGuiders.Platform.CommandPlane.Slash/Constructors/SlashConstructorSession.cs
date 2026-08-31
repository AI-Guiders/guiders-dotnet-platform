#nullable enable

namespace AIGuiders.Platform.CommandPlane;

public sealed class SlashConstructorSession(SlashValueConstructorNavigator navigator)
{
    SlashConstructorDraft? _draft;

    public bool IsActive => _draft is not null;

    public SlashConstructorDraft? Draft => _draft;

    public bool IsComplete
    {
        get
        {
            if (_draft is null)
            {
                return false;
            }

            return navigator.TryEmitWire(_draft, out _, out _);
        }
    }

    public void Start(string rootConstructorId, string canonicalPath)
    {
        _draft = new SlashConstructorDraft
        {
            RootConstructorId = rootConstructorId,
            CanonicalPath = canonicalPath,
        };
    }

    public bool TryAdvance(string pickedValue) =>
        _draft is not null && navigator.TryAdvance(_draft, pickedValue);

    public bool TryComplete(out string wireValue)
    {
        wireValue = "";
        if (_draft is null)
        {
            return false;
        }

        if (!navigator.TryEmitWire(_draft, out wireValue, out _))
        {
            return false;
        }

        Cancel();
        return true;
    }

    public SlashCompletionResult GetCompletionResult()
    {
        if (_draft is null)
        {
            return new SlashCompletionResult([], new SlashInputGuidance(
                SlashInputMode.Path,
                "/",
                "",
                "",
                null,
                ""));
        }

        var items = navigator.GetSuggestions(_draft, partial: "");
        var guidance = navigator.BuildGuidance(_draft);
        return new SlashCompletionResult(items, guidance);
    }

    public void Cancel() => _draft = null;
}
