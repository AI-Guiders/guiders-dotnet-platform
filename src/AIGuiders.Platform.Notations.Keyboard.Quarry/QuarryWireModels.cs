#nullable enable

namespace AIGuiders.Platform.Notations.Keyboard.Quarry;

public abstract record QuarryWireStep;

public sealed record QuarryWireChordStep(IReadOnlyList<string> ModifierPrefixes, string Key) : QuarryWireStep;

public sealed record QuarryWirePlainStep(string Token) : QuarryWireStep;

public readonly record struct QuarryParseResult(bool IsSuccess, IReadOnlyList<QuarryWireStep> Steps, string Error)
{
    public static QuarryParseResult Ok(IReadOnlyList<QuarryWireStep> steps) =>
        new(true, steps, "");

    public static QuarryParseResult Fail(string message) =>
        new(false, Array.Empty<QuarryWireStep>(), message);
}
