#nullable enable

namespace AIGuiders.Platform.Combinations.Overlay;

public sealed class SectionOverlayBuilder<TSection> where TSection : class
{
    internal List<ISectionOverlayStep<TSection>> Steps { get; } = [];

    /// <summary>Field-level overlay: overlay non-null values win per declared field.</summary>
    public SectionOverlayBuilder<TSection> FieldOverlay<TNested>(
        Func<TSection, TNested?> getNested,
        Func<TSection, TNested?, TSection> setNested,
        Action<FieldOverlayBuilder<TNested>> configure)
        where TNested : class, new()
    {
        ArgumentNullException.ThrowIfNull(getNested);
        ArgumentNullException.ThrowIfNull(setNested);
        ArgumentNullException.ThrowIfNull(configure);

        Steps.Add(new FieldOverlaySectionStep<TSection, TNested>(getNested, setNested, configure));
        return this;
    }

    /// <summary>Replace whole nested value when overlay provides non-null.</summary>
    public SectionOverlayBuilder<TSection> ReplaceWhenPresent<TNested>(
        Func<TSection, TNested?> getNested,
        Func<TSection, TNested?, TSection> setNested)
    {
        ArgumentNullException.ThrowIfNull(getNested);
        ArgumentNullException.ThrowIfNull(setNested);
        Steps.Add(new ReplaceWhenPresentStep<TSection, TNested>(getNested, setNested));
        return this;
    }
}

/// <summary>Declarative nullable-field overlay (spirit of RuleFor(...)).</summary>
public sealed class FieldOverlayBuilder<T> where T : class, new()
{
    readonly T _baseline;
    readonly T _overlay;
    readonly T _result = new();

    internal FieldOverlayBuilder(T baseline, T overlay)
    {
        _baseline = baseline;
        _overlay = overlay;
    }

    public FieldOverlayBuilder<T> Field<TValue>(
        Func<T, TValue?> get,
        Action<T, TValue?> set)
    {
        ArgumentNullException.ThrowIfNull(get);
        ArgumentNullException.ThrowIfNull(set);
        var overlayValue = get(_overlay);
        var baselineValue = get(_baseline);
        set(_result, overlayValue ?? baselineValue);
        return this;
    }

    internal T Complete() => _result;
}
