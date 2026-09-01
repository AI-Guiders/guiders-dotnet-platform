namespace AIGuiders.Platform.Authoring.Core;

public static class AuthoringImportLine
{
    public const string NormativeKeyword = "import";

    public static bool TryParse(string line, out AuthoringImportDirective? directive)
    {
        directive = null;
        var text = AuthoringSource.StripComment(line).Trim();
        if (text.Length == 0)
        {
            return false;
        }

        var legacy = false;
        if (text.StartsWith("!include ", StringComparison.Ordinal))
        {
            legacy = true;
            text = text["!include ".Length..].Trim();
        }
        else if (text.StartsWith("import ", StringComparison.Ordinal))
        {
            text = text["import ".Length..].Trim();
        }
        else
        {
            return false;
        }

        if (text.Length == 0)
        {
            return false;
        }

        string path;
        string? alias = null;
        AuthoringImportTargetKind kind;

        if (text[0] is '"' or '\'')
        {
            if (!TryReadQuoted(text, out path, out var rest))
            {
                return false;
            }

            kind = AuthoringImportTargetKind.LogicalPath;
            if (!TryReadAlias(rest, ref alias))
            {
                return false;
            }
        }
        else if (text[0] is '<')
        {
            var close = text.IndexOf('>');
            if (close <= 1)
            {
                return false;
            }

            path = text[1..close].Trim();
            if (path.Length == 0)
            {
                return false;
            }

            kind = AuthoringImportTargetKind.WireLibrary;
            if (!TryReadAlias(text[(close + 1)..].Trim(), ref alias))
            {
                return false;
            }
        }
        else
        {
            return false;
        }

        directive = new(kind, path, alias, legacy);
        return true;
    }

    static bool TryReadQuoted(string text, out string path, out string rest)
    {
        path = "";
        rest = "";
        var quote = text[0];
        var end = text.IndexOf(quote, 1);
        if (end < 0)
        {
            return false;
        }

        path = text[1..end];
        rest = text[(end + 1)..].Trim();
        return true;
    }

    static bool TryReadAlias(string remainder, ref string? alias)
    {
        if (remainder.Length == 0)
        {
            return true;
        }

        if (!remainder.StartsWith("as ", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        alias = remainder[3..].Trim();
        return alias.Length > 0;
    }
}
