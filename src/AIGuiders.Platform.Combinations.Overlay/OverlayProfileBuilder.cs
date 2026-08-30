#nullable enable

namespace AIGuiders.Platform.Combinations.Overlay;

/// <summary>Fluent overlay recipe (FV-style DX): rules read top-to-bottom.</summary>
public sealed class OverlayProfileBuilder<T> where T : class
{
    readonly string _name;
    readonly CombinationSemantics _semantics;
    readonly List<IOverlayStep<T>> _steps = [];

    internal OverlayProfileBuilder(string name, CombinationSemantics semantics)
    {
        _name = name;
        _semantics = semantics;
    }

    /// <summary>Skip all following rules in this builder when overlay does not match.</summary>
    public OverlayProfileBuilder<T> When(Func<T, bool> overlayMatches, Action<OverlayProfileBuilder<T>> configure)
    {
        ArgumentNullException.ThrowIfNull(overlayMatches);
        ArgumentNullException.ThrowIfNull(configure);

        var nested = new OverlayProfileBuilder<T>(_name, _semantics);
        configure(nested);
        _steps.Add(new ConditionalOverlayStep<T>(overlayMatches, nested._steps));
        return this;
    }

    /// <summary>Merge a nested section when overlay supplies it.</summary>
    public OverlayProfileBuilder<T> MergeSection<TSection>(
        Func<T, TSection?> getSection,
        Func<T, TSection?, T> setSection,
        Action<SectionOverlayBuilder<TSection>> configure)
        where TSection : class
    {
        ArgumentNullException.ThrowIfNull(getSection);
        ArgumentNullException.ThrowIfNull(setSection);
        ArgumentNullException.ThrowIfNull(configure);

        var sectionBuilder = new SectionOverlayBuilder<TSection>();
        configure(sectionBuilder);
        _steps.Add(new SectionOverlayStep<T, TSection>(getSection, setSection, sectionBuilder.Steps));
        return this;
    }

    /// <summary>Escape hatch: imperative merge rule.</summary>
    public OverlayProfileBuilder<T> Rule(Func<T, T, T> merge) =>
        Rule(new DelegateOverlayStep<T>(merge));

    public OverlayProfileBuilder<T> Rule(IOverlayStep<T> step)
    {
        ArgumentNullException.ThrowIfNull(step);
        _steps.Add(step);
        return this;
    }

    public OverlayPolicy<T> Build()
    {
        if (_steps.Count == 0)
            throw new InvalidOperationException($"Overlay profile '{_name}' has no rules.");

        Combinator<T> combinator = (baseline, overlay) =>
        {
            var current = baseline;
            foreach (var step in _steps)
                current = step.Apply(baseline, overlay, current);
            return current;
        };

        return new OverlayPolicy<T>(_name, _semantics, combinator);
    }
}
