#nullable enable

using AIGuiders.Platform.LanguageIntelligence.Line;

namespace AIGuiders.Platform.LanguageIntelligence.Markup;

public static class EditorTextTransform
{
    public static EditorTextEditResult ApplyFormat(
        string text,
        EditorSelectionSpan selection,
        TextInsertFormatDefinition format,
        string textMode = "markdown")
    {
        var resolved = ResolveFormat(format, textMode);
        var start = Math.Clamp(selection.Start, 0, text.Length);
        var end = Math.Clamp(selection.End, 0, text.Length);
        if (end < start)
            (start, end) = (end, start);

        if (resolved.WrapOpen is not null && resolved.WrapClose is not null)
        {
            var selected = end > start ? text[start..end] : "text";
            var wrapped = resolved.WrapOpen + selected + resolved.WrapClose;
            var newText = text[..start] + wrapped + text[end..];
            var newStart = start + resolved.WrapOpen.Length;
            var newEnd = newStart + selected.Length;
            return new EditorTextEditResult(newText, newStart, newEnd);
        }

        if (resolved.Insert is not null)
        {
            var newText = text[..start] + resolved.Insert + text[end..];
            var pos = start + resolved.Insert.Length;
            return new EditorTextEditResult(newText, pos, pos);
        }

        return new EditorTextEditResult(text, start, end);
    }

    public static ResolvedInsertFormat ResolveFormat(TextInsertFormatDefinition format, string textMode)
    {
        var mode = (textMode ?? "markdown").Trim().ToLowerInvariant();
        if (mode == "html")
        {
            if (format.HtmlWrapOpen is not null && format.HtmlWrapClose is not null)
                return new ResolvedInsertFormat(null, format.HtmlWrapOpen, format.HtmlWrapClose);
            if (format.HtmlInsert is not null)
                return new ResolvedInsertFormat(format.HtmlInsert, null, null);
        }

        if (format.WrapOpen is not null && format.WrapClose is not null)
            return new ResolvedInsertFormat(null, format.WrapOpen, format.WrapClose);
        return new ResolvedInsertFormat(format.Insert, null, null);
    }

    public readonly record struct ResolvedInsertFormat(string? Insert, string? WrapOpen, string? WrapClose);
}
