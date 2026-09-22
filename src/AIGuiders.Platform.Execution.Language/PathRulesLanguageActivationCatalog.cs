namespace AIGuiders.Platform.Execution.Language;

/// <summary>LRC activation via federation <see cref="LanguagePathRules"/>.</summary>
public sealed class PathRulesLanguageActivationCatalog : ILanguageActivationCatalog
{
    public string ResolveLanguageId(string path) =>
        LanguagePathRules.ResolveLanguageId(path) ?? string.Empty;
}
