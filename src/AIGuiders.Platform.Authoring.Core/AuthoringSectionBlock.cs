namespace AIGuiders.Platform.Authoring.Core;

public sealed record AuthoringSectionBlock(
    string Keyword,
    AuthoringSurfaceKind SurfaceKind,
    IReadOnlyList<AuthoringLine> Body);
