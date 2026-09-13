namespace AIGuiders.Platform.Authoring.Sat.IdeSession;

/// <summary>
/// Known gate implementation symbols from ide-session federation (Modeling.Ide.Session).
/// </summary>
public static class IdeSessionGateCodeRegistry
{
    static readonly HashSet<string> KnownCodes = new(StringComparer.Ordinal)
    {
        "GraphValidation.validate",
        "HoareChecker.checkTypes",
        "ObsChecker.checkRename",
        "GraphPatch.scope",
        "FcsLanguageBackend blocker",
    };

    public static bool IsKnown(string code) =>
        !string.IsNullOrWhiteSpace(code) && KnownCodes.Contains(code.Trim());

    public static bool IsFuturePort(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return true;
        }

        var trimmed = code.Trim();
        return trimmed.StartsWith('(')
            || trimmed.Contains("future", StringComparison.OrdinalIgnoreCase);
    }
}
