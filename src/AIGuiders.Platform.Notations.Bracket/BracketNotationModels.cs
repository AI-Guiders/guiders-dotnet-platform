#nullable enable
using AIGuiders.Platform.IntermediateRepresentation.Bracket;
using AIGuiders.Platform.Notations;

namespace AIGuiders.Platform.Notations.Bracket;

public interface IBracketNotationReader
{
    bool TryRead(
        string wire,
        BracketNotationProfile profile,
        out NormalizedBracketWire? normalized,
        out string error);
}

public static class BracketAxisExtensions
{
    public static NotationKvPair ToKvPair(this BracketAxis axis) => new(axis.Key, axis.Sign, axis.Value);
}