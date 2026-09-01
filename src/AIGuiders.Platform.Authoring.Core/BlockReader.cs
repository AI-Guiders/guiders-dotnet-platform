namespace AIGuiders.Platform.Authoring.Core;

public enum AuthoringSurfaceKind
{
    KeyValue,
    Table,
    IndentedTree,
}

public sealed record AuthoringSectionOpener(string Keyword, AuthoringSurfaceKind Kind);

public sealed class AuthoringBlock
{
    public required IReadOnlyList<AuthoringLine> Body { get; init; }
    public int EndLineIndex { get; init; }
    public bool IsClosed { get; init; }
}

/// <summary>DashSpec-parity <c>keyword … end keyword</c> blocks (GUIDERS-ADR-0048 §3).</summary>
public static class BlockReader
{
    static readonly HashSet<string> TreeSections = new(StringComparer.OrdinalIgnoreCase)
    {
        "channels",
    };

    static readonly HashSet<string> KeyValueSections = new(StringComparer.OrdinalIgnoreCase)
    {
        "defaults",
        "executors",
    };

    static readonly HashSet<string> TableCapableSections = new(StringComparer.OrdinalIgnoreCase)
    {
        "variables",
        "helps",
        "phrases",
        "profiles",
        "commands",
        "bindings",
        "melodies",
        "mcp",
    };

    public static bool TryParseOpener(string line, out AuthoringSectionOpener opener)
    {
        opener = default!;
        var trimmed = line.Trim();
        if (trimmed.EndsWith(" table", StringComparison.Ordinal))
        {
            opener = new(trimmed[..^" table".Length].Trim(), AuthoringSurfaceKind.Table);
            return true;
        }

        if (TreeSections.Contains(trimmed))
        {
            opener = new(trimmed, AuthoringSurfaceKind.IndentedTree);
            return true;
        }

        if (KeyValueSections.Contains(trimmed))
        {
            opener = new(trimmed, AuthoringSurfaceKind.KeyValue);
            return true;
        }

        if (TableCapableSections.Contains(trimmed))
        {
            opener = new(trimmed, AuthoringSurfaceKind.KeyValue);
            return true;
        }

        return false;
    }

    public static AuthoringBlock Read(
        IReadOnlyList<AuthoringLine> lines,
        int bodyStartIndex,
        string keyword,
        IList<AuthoringDiagnostic>? diagnostics = null)
    {
        var body = new List<AuthoringLine>();
        var i = bodyStartIndex;
        for (; i < lines.Count; i++)
        {
            var line = lines[i];
            if (line.Text.StartsWith($"end {keyword}", StringComparison.Ordinal))
            {
                return new AuthoringBlock
                {
                    Body = body,
                    EndLineIndex = i,
                    IsClosed = true,
                };
            }

            if (!string.IsNullOrWhiteSpace(line.Text))
            {
                body.Add(line);
            }
        }

        diagnostics?.Add(new(
            AuthoringDiagnosticCode.InvalidSyntax,
            $"Unclosed block `{keyword}`.",
            bodyStartIndex > 0 ? lines[bodyStartIndex - 1].LineNumber : 1));

        return new AuthoringBlock
        {
            Body = body,
            EndLineIndex = lines.Count - 1,
            IsClosed = false,
        };
    }

    public static AuthoringSurfaceKind ResolveSurfaceKind(AuthoringSectionOpener opener) =>
        opener.Kind switch
        {
            AuthoringSurfaceKind.Table => AuthoringSurfaceKind.Table,
            AuthoringSurfaceKind.IndentedTree => AuthoringSurfaceKind.IndentedTree,
            AuthoringSurfaceKind.KeyValue when TableCapableSections.Contains(opener.Keyword) => AuthoringSurfaceKind.Table,
            _ => AuthoringSurfaceKind.KeyValue,
        };
}
