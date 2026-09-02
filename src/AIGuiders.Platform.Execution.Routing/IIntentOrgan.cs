namespace AIGuiders.Platform.Execution.Routing;

/// <summary>
/// Intent organ peel seam: parse wire → route; route + optional override → outcome.
/// CDP reference: <c>CitizenBufferEdit.Route</c> + <c>Execute</c>.
/// </summary>
public interface IIntentOrgan<TRoute, TOutcome>
{
    TRoute Route(string raw);

    TOutcome Execute(TRoute route, DispatchCallOverride? callOverride = null);
}
