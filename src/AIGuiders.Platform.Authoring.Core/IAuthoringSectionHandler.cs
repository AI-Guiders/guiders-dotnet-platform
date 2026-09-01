namespace AIGuiders.Platform.Authoring.Core;

public interface IAuthoringSectionHandler<TContext>
{
    string Keyword { get; }

    void Apply(TContext context, AuthoringSectionBlock block);
}
