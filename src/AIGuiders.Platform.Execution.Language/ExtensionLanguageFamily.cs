using AIGuiders.Platform.Modeling.Language;

namespace AIGuiders.Platform.Execution.Language;

/// <summary>Base for extension-based language.* ship plugins.</summary>
public abstract class ExtensionLanguageFamily : ILanguageFamilyPlugin
{
    public abstract string PluginId { get; }

    public abstract string LanguageId { get; }

    public virtual string DocumentProfileId => "neutral.plain";

    public abstract IReadOnlyList<string> FileExtensions { get; }

    public abstract ILanguageBackend CreateLanguageBackend();

    public bool MatchesDocument(string documentPathOrId) =>
        LanguageFamilyPathRules.HasAnyExtension(documentPathOrId, FileExtensions);
}
