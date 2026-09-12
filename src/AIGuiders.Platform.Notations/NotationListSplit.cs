#nullable enable

using ModelingNotations = AIGuiders.Platform.Modeling.Notations;

namespace AIGuiders.Platform.Notations;

/// <summary>
/// GUIDERS-FSHARP-ADR-0003 §4.4 cutover: list split SSOT via Modeling.Notations.Core.
/// </summary>
public static class NotationListSplit
{
    public static List<string> SplitTopLevel(
        string text,
        char separator,
        char openBracket = '[',
        char closeBracket = ']') =>
        ModelingNotations.NotationListSplit
            .splitTopLevel(text, separator, openBracket, closeBracket)
            .ToList();
}
