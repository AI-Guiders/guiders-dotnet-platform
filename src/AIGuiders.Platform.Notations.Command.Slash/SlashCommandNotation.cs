namespace AIGuiders.Platform.Notations.Command.Slash;

public static class SlashCommandNotation
{
    public static SlashWireBody ParseBody(string body)
    {
        var endsWithSpace = body.EndsWith(' ');
        var tokens = body.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
        return new SlashWireBody(tokens, endsWithSpace);
    }

    public static bool TryParseLine(string slashLine, out SlashWireBody body)
    {
        body = new SlashWireBody([], false);
        if (string.IsNullOrWhiteSpace(slashLine) || slashLine[0] != '/')
            return false;

        body = ParseBody(slashLine[1..].TrimEnd());
        return body.Tokens.Count > 0;
    }
}
