#nullable enable

namespace AIGuiders.Platform.Notations.Keyboard.Quarry;

public static class QuarrySequenceParser
{
    public delegate bool TokenParser(string token, out QuarryWireStep? step, out string error);

    public static QuarryParseResult Parse(string? input, TokenParser parseToken)
    {
        if (string.IsNullOrEmpty(input))
            return QuarryParseResult.Ok(Array.Empty<QuarryWireStep>());

        var trimmed = input.Trim();
        var tokens = trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        var steps = new List<QuarryWireStep>(tokens.Length);

        foreach (var token in tokens)
        {
            if (!parseToken(token, out var step, out var error))
                return QuarryParseResult.Fail(error);

            steps.Add(step!);
        }

        return QuarryParseResult.Ok(steps);
    }
}
