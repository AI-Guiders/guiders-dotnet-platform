namespace AIGuiders.Platform.Authoring.Core;

public sealed class SectionHandlerRegistry<TContext>
{
    readonly IReadOnlyDictionary<string, IAuthoringSectionHandler<TContext>> _handlers;

    public SectionHandlerRegistry(IEnumerable<IAuthoringSectionHandler<TContext>> handlers) =>
        _handlers = handlers.ToDictionary(static h => h.Keyword, StringComparer.OrdinalIgnoreCase);

    public bool TryGet(string keyword, out IAuthoringSectionHandler<TContext> handler) =>
        _handlers.TryGetValue(keyword, out handler!);

    public bool Apply(TContext context, AuthoringSectionBlock block)
    {
        if (!TryGet(block.Keyword, out var handler))
        {
            return false;
        }

        handler.Apply(context, block);
        return true;
    }
}
