using AIGuiders.Platform.Modeling.Language;
using AIGuiders.Platform.Modeling.Language.Adapters.Gdl;

namespace AIGuiders.Platform.Execution.Language.Builtins;

public sealed class GdlLanguageFamily : ILanguageFamilyPlugin
{
    public string PluginId => "language.gdl";

    public string LanguageId => LanguageIds.Gdl;

    public string DocumentProfileId => "neutral.plain";

    public IReadOnlyList<string> FileExtensions => [".gdl", ".gdlproj"];

    public bool MatchesDocument(string documentPathOrId) =>
        ExtensionLanguagePathRules.HasAnyExtension(documentPathOrId, FileExtensions);

    public ILanguageBackend CreateLanguageBackend() => new GdlLanguageBackend();
}
