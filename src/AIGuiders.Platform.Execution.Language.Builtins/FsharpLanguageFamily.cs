using AIGuiders.Platform.Modeling.Language;
using AIGuiders.Platform.Modeling.Language.Adapters.Fcs;

namespace AIGuiders.Platform.Execution.Language.Builtins;

public sealed class FsharpLanguageFamily : ILanguageFamilyPlugin
{
    public string PluginId => "language.fsharp";

    public string LanguageId => LanguageIds.Fsharp;

    public string DocumentProfileId => "neutral.plain";

    public IReadOnlyList<string> FileExtensions => [".fs", ".fsx", ".fsproj"];

    public bool MatchesDocument(string documentPathOrId) =>
        ExtensionLanguagePathRules.HasAnyExtension(documentPathOrId, FileExtensions);

    public ILanguageBackend CreateLanguageBackend() =>
        new FcsLanguageBackend(projectOptionsSource: null);
}
