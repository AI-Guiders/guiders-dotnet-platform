#nullable enable

namespace AIGuiders.Platform.Execution.CommandPlane;

public sealed class ArgConstructorSession(ValueConstructorNavigator navigator)
{
    public ValueConstructorNavigator Navigator { get; } = navigator;

    ArgConstructorDraft? _draft;
    string _typedArgTail = "";

    public bool IsActive => _draft is not null;

    public ArgConstructorDraft? Draft => _draft;

    public string TypedArgTail => _typedArgTail;

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
        _draft = new ArgConstructorDraft
        {
            RootConstructorId = rootConstructorId,
            CanonicalPath = canonicalPath,
        };
        _typedArgTail = "";
    }

    public void SetTypedArgTail(string typedArgTail) => _typedArgTail = typedArgTail.Trim();

    public bool TryAdvance(string pickedValue) =>
        _draft is not null && navigator.TryAdvance(_draft, pickedValue);

    public bool TryApplyLocaleParts(
        LocaleDateParts parts,
        ValueConstructorRegistry registry,
        ValueConstructorNavigator navigatorInstance,
        LocaleInputProfile profile)
    {
        if (_draft is null)
        {
            return false;
        }

        return navigatorInstance.TryApplyLocaleParts(_draft, parts, registry, profile);
    }

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

    public void Cancel()
    {
        _draft = null;
        _typedArgTail = "";
    }
}
