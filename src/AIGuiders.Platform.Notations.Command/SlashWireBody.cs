namespace AIGuiders.Platform.Notations.Command;

/// <summary>Pre-catalog slash/console wire: tokenized path + tail before longest-prefix resolve.</summary>
public sealed record SlashWireBody(
    IReadOnlyList<string> Tokens,
    bool EndsWithSpaceAfterTokens)
{
    public string JoinedTokens => string.Join(' ', Tokens);
}
