namespace AIGuiders.Platform.Execution.Language;

/// <summary>Compose LRC from language family plugins.</summary>
public static class LanguageFamilyResolverHost
{
    public static LanguageResolverCenter Create(
        IEnumerable<ILanguageFamilyPlugin> families,
        Action<LanguageResolverBuilder>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(families);

        var builder = new LanguageResolverBuilder();
        foreach (var family in families)
        {
            ArgumentNullException.ThrowIfNull(family);
            builder.Register(family.CreateLanguageBackend());
        }

        configure?.Invoke(builder);
        return builder.Build();
    }

    public static LanguageResolverCenter Create(
        IEnumerable<ILanguageFamilyPlugin> families,
        ILanguageActivationCatalog activation,
        Action<LanguageResolverBuilder>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(families);
        ArgumentNullException.ThrowIfNull(activation);

        var builder = new LanguageResolverBuilder().WithActivation(activation);
        foreach (var family in families)
        {
            builder.Register(family.CreateLanguageBackend());
        }

        configure?.Invoke(builder);
        return builder.Build();
    }
}
