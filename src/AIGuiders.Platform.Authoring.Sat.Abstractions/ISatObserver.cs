namespace AIGuiders.Platform.Authoring.Sat;

public interface ISatObserver
{
    string ObserverId { get; }

    int Priority { get; }

    bool CanObserve(SatContext context);

    SatRunResult Observe(SatContext context);
}
