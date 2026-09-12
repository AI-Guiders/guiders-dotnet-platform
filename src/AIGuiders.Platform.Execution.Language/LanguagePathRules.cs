using Microsoft.FSharp.Core;

namespace AIGuiders.Platform.Execution.Language;

/// <summary>Transitional facade — GUIDERS-FSHARP-ADR-0003 §4.6 cutover.</summary>
public static class LanguagePathRules
{
    public static string? ResolveLanguageId(string path)
    {
        var option = Modeling.Language.LanguagePathRules.resolveLanguageId(path);
        return FSharpOption<string>.get_IsSome(option) ? option.Value : null;
    }
}
