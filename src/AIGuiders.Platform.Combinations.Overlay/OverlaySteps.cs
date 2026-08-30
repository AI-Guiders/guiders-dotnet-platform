#nullable enable



namespace AIGuiders.Platform.Combinations.Overlay;



public interface IOverlayStep<T> where T : class

{

    T Apply(T baseline, T overlay, T current);

}



public interface ISectionOverlayStep<TSection> where TSection : class

{

    TSection? Apply(TSection? baseline, TSection? overlay, TSection? current);

}



sealed class ConditionalOverlayStep<T>(

    Func<T, bool> overlayMatches,

    IReadOnlyList<IOverlayStep<T>> steps) : IOverlayStep<T>

    where T : class

{

    public T Apply(T baseline, T overlay, T current)

    {

        if (!overlayMatches(overlay))

            return current;



        var acc = current;

        foreach (var step in steps)

            acc = step.Apply(baseline, overlay, acc);

        return acc;

    }

}



sealed class DelegateOverlayStep<T>(Func<T, T, T> merge) : IOverlayStep<T> where T : class

{

    public T Apply(T baseline, T overlay, T current) => merge(current, overlay);

}



sealed class SectionOverlayStep<TDocument, TSection>(

    Func<TDocument, TSection?> getSection,

    Func<TDocument, TSection?, TDocument> setSection,

    IReadOnlyList<ISectionOverlayStep<TSection>> steps) : IOverlayStep<TDocument>

    where TDocument : class

    where TSection : class

{

    public TDocument Apply(TDocument baseline, TDocument overlay, TDocument current)

    {

        var overlaySection = getSection(overlay);

        if (overlaySection is null)

            return current;



        var baselineSection = getSection(baseline);

        var mergedSection = baselineSection;

        foreach (var step in steps)

            mergedSection = step.Apply(baselineSection, overlaySection, mergedSection);



        return setSection(current, mergedSection);

    }

}



sealed class FieldOverlaySectionStep<TSection, TNested>(

    Func<TSection, TNested?> getNested,

    Func<TSection, TNested?, TSection> setNested,

    Action<FieldOverlayBuilder<TNested>> configure) : ISectionOverlayStep<TSection>

    where TSection : class

    where TNested : class, new()

{

    public TSection? Apply(TSection? baseline, TSection? overlay, TSection? current)

    {

        if (overlay is null)

            return current;



        var overlayNested = getNested(overlay);

        if (overlayNested is null)

            return current;



        var baselineNested = baseline is null ? null : getNested(baseline);

        if (baselineNested is null)

            return setNested(current ?? overlay, overlayNested);



        var builder = new FieldOverlayBuilder<TNested>(baselineNested, overlayNested);

        configure(builder);

        return setNested(current ?? baseline ?? overlay, builder.Complete());

    }

}



sealed class ReplaceWhenPresentStep<TSection, TNested>(

    Func<TSection, TNested?> getNested,

    Func<TSection, TNested?, TSection> setNested) : ISectionOverlayStep<TSection>

    where TSection : class

{

    public TSection? Apply(TSection? baseline, TSection? overlay, TSection? current)

    {

        if (overlay is null)

            return current;



        var overlayNested = getNested(overlay);

        if (overlayNested is null)

            return current;



        return setNested(current ?? baseline ?? overlay, overlayNested);

    }

}


