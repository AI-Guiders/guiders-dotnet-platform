namespace AIGuiders.Platform.Authoring.Core;

/// <summary>
/// Guild-level document walk: preamble lines + <see cref="BlockReader"/> blocks + section handler dispatch.
/// </summary>
public sealed class AuthoringDocumentWalker<TContext>
{
    readonly Func<AuthoringLine, TContext, bool> _tryPreambleLine;
    readonly SectionHandlerRegistry<TContext> _registry;
    readonly Func<TContext, IList<AuthoringDiagnostic>> _diagnostics;
    readonly Action<TContext, AuthoringSectionOpener, IReadOnlyList<AuthoringLine>>? _onUnknownSection;

    public AuthoringDocumentWalker(
        SectionHandlerRegistry<TContext> registry,
        Func<TContext, IList<AuthoringDiagnostic>> diagnostics,
        Func<AuthoringLine, TContext, bool> tryPreambleLine,
        Action<TContext, AuthoringSectionOpener, IReadOnlyList<AuthoringLine>>? onUnknownSection = null)
    {
        _registry = registry;
        _diagnostics = diagnostics;
        _tryPreambleLine = tryPreambleLine;
        _onUnknownSection = onUnknownSection;
    }

    public void Walk(IReadOnlyList<AuthoringLine> lines, TContext context)
    {
        var diagnostics = _diagnostics(context);
        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line.Text))
            {
                continue;
            }

            if (_tryPreambleLine(line, context))
            {
                continue;
            }

            if (!BlockReader.TryParseOpener(line.Text, out var opener))
            {
                continue;
            }

            var block = BlockReader.Read(lines, i + 1, opener.Keyword, diagnostics);
            i = block.EndLineIndex;
            if (!block.IsClosed)
            {
                continue;
            }

            var sectionBlock = new AuthoringSectionBlock(
                opener.Keyword,
                BlockReader.ResolveSurfaceKind(opener),
                block.Body);

            if (_registry.Apply(context, sectionBlock))
            {
                continue;
            }

            _onUnknownSection?.Invoke(context, opener, block.Body);
        }
    }
}
