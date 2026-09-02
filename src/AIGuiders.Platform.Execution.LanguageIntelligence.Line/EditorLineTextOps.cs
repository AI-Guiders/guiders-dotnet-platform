#nullable enable

namespace AIGuiders.Platform.Execution.LanguageIntelligence.Line;

public static class EditorLineTextOps
{
    public static int LineCount(string text) =>
        string.IsNullOrEmpty(text) ? 1 : text.Split('\n').Length;

    public static EditorSelectionSpan SelectionSpanForLineRange(string text, EditorLineRange range)
    {
        var lines = SplitLines(text);
        if (!range.IsValid(lines.Count))
            return new EditorSelectionSpan(0, 0);

        var start = CharOffsetForLine(lines, range.StartLine, 0);
        var endLineText = lines[range.EndLine - 1];
        var end = CharOffsetForLine(lines, range.EndLine, endLineText.Length);
        return new EditorSelectionSpan(start, end);
    }

    public static EditorLineRange LineRangeForSelection(string text, EditorSelectionSpan selection)
    {
        var lines = SplitLines(text);
        var startLine = LineNumberAtOffset(text, selection.Start);
        var endLine = LineNumberAtOffset(text, Math.Max(selection.Start, selection.End - 1));
        return new EditorLineRange(startLine, endLine);
    }

    public static EditorTextEditResult DeleteLineRange(string text, EditorLineRange range)
    {
        var lines = SplitLines(text);
        if (!range.IsValid(lines.Count))
            return new EditorTextEditResult(text, 0, 0);

        var kept = new List<string>();
        for (var i = 0; i < lines.Count; i++)
        {
            var lineNo = i + 1;
            if (lineNo < range.StartLine || lineNo > range.EndLine)
                kept.Add(lines[i]);
        }

        var newText = string.Join("\n", kept);
        var sel = range.StartLine <= kept.Count
            ? CharOffsetForLine(kept, range.StartLine, 0)
            : newText.Length;
        return new EditorTextEditResult(newText, sel, sel);
    }

    public static int LineNumberAtOffset(string text, int offset)
    {
        offset = Math.Clamp(offset, 0, text.Length);
        var line = 1;
        for (var i = 0; i < offset; i++)
        {
            if (text[i] == '\n')
                line++;
        }

        return line;
    }

    static List<string> SplitLines(string text) =>
        string.IsNullOrEmpty(text) ? new List<string> { "" } : text.Split('\n').ToList();

    static int CharOffsetForLine(IReadOnlyList<string> lines, int lineNumber, int column)
    {
        var offset = 0;
        for (var i = 0; i < lineNumber - 1; i++)
            offset += lines[i].Length + 1;
        offset += Math.Clamp(column, 0, lines[lineNumber - 1].Length);
        return offset;
    }
}
