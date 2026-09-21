using AIGuiders.Platform.Execution.Language;
using AIGuiders.Platform.Modeling.Language;

namespace AIGuiders.Platform.Language.CSharp;

/// <summary>Standard ship plugin <c>language.csharp</c>.</summary>
public sealed class CsharpLanguageFamily : ExtensionLanguageFamily
{
    public override string PluginId => "language.csharp";

    public override string LanguageId => Modeling.Language.LanguageIds.Csharp;

    public override IReadOnlyList<string> FileExtensions =>
        [".cs", ".csproj", ".sln", ".slnx"];

    public override ILanguageBackend CreateLanguageBackend() => new CsharpLanguageBackend();
}
