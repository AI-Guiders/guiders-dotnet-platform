using AIGuiders.Platform.Execution.Language;
using AIGuiders.Platform.Modeling.Language;
using AIGuiders.Platform.Modeling.Language.Adapters.Gdl;

namespace AIGuiders.Platform.Language.Gdl;

/// <summary>Standard ship plugin <c>language.gdl</c>.</summary>
public sealed class GdlLanguageFamily : ExtensionLanguageFamily
{
    public override string PluginId => "language.gdl";

    public override string LanguageId => Modeling.Language.LanguageIds.Gdl;

    public override IReadOnlyList<string> FileExtensions => [".gdl", ".gdlproj"];

    public override ILanguageBackend CreateLanguageBackend() => new GdlLanguageBackend();
}
