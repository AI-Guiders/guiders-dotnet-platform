namespace AIGuiders.Platform.Execution.Language.Builtins;

/// <summary>Built-in language.* families shipped with platform Execution.Language.</summary>
public static class LanguageFamilyBundle
{
    public static IReadOnlyList<ILanguageFamilyPlugin> CoreFamilies =>
    [
        new FsharpLanguageFamily(),
        new GdlLanguageFamily(),
    ];

    public static LanguageResolverCenter CreateLanguageResolver(
        Action<LanguageResolverBuilder>? configure = null,
        params ILanguageFamilyPlugin[] planetFamilies)
    {
        var families = CoreFamilies.Concat(planetFamilies).ToArray();
        return LanguageFamilyResolverHost.Create(
            families,
            new LanguageFamilyActivationCatalog(families),
            configure);
    }
}
