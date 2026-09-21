namespace AIGuiders.Platform.Execution.Language;

/// <summary>Fluent registration for <see cref="LanguageResolverCenter"/>.</summary>
public sealed class LanguageResolverBuilder
{
    private readonly List<ILanguageBackend> _backends = [];
    private ILanguageActivationCatalog? _activation;

    public LanguageResolverBuilder Register(ILanguageBackend backend)
    {
        ArgumentNullException.ThrowIfNull(backend);
        _backends.Add(backend);
        return this;
    }

    public LanguageResolverBuilder RegisterRange(IEnumerable<ILanguageBackend> backends)
    {
        ArgumentNullException.ThrowIfNull(backends);
        _backends.AddRange(backends);
        return this;
    }

    public LanguageResolverBuilder WithActivation(ILanguageActivationCatalog activation)
    {
        ArgumentNullException.ThrowIfNull(activation);
        _activation = activation;
        return this;
    }

    public LanguageResolverCenter Build() => new(_backends, _activation);
}
