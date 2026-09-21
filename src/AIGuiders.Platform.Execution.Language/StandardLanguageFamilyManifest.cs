namespace AIGuiders.Platform.Execution.Language;

/// <summary>Default language.* plugins in standard federation ship set.</summary>
public static class StandardLanguageFamilyManifest
{
    public static readonly (string Assembly, string Type)[] Federation =
    [
        ("AIGuiders.Platform.Language.Fsharp", "AIGuiders.Platform.Language.Fsharp.FsharpLanguageFamily"),
        ("AIGuiders.Platform.Language.Gdl", "AIGuiders.Platform.Language.Gdl.GdlLanguageFamily"),
        ("AIGuiders.Platform.Language.CSharp", "AIGuiders.Platform.Language.CSharp.CsharpLanguageFamily"),
    ];

    public static IReadOnlyList<ILanguageFamilyPlugin> LoadFederation(
        string appBaseDirectory,
        string? pluginsPath = null) =>
        LanguageFamilyAssemblyLoader.LoadMany(Federation, appBaseDirectory, pluginsPath);
}
