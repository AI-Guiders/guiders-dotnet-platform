using AIGuiders.Platform.Execution.Language;
using AIGuiders.Platform.Modeling.Language;
using AIGuiders.Platform.Modeling.Language.Adapters.Fcs;

namespace AIGuiders.Platform.Language.Fsharp;

/// <summary>Standard ship plugin <c>language.fsharp</c>.</summary>
public sealed class FsharpLanguageFamily : ExtensionLanguageFamily
{
    public override string PluginId => "language.fsharp";

    public override string LanguageId => Modeling.Language.LanguageIds.Fsharp;

    public override IReadOnlyList<string> FileExtensions => [".fs", ".fsx", ".fsproj"];

    public override ILanguageBackend CreateLanguageBackend() =>
        new FcsLanguageBackend(projectOptionsSource: null);
}
