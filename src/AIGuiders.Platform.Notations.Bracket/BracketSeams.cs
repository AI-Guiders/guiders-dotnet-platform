#nullable enable
using AIGuiders.Platform.Modeling.Notations.Bracket;

namespace AIGuiders.Platform.Notations.Bracket;

public interface IBracketNotationReader
{
    bool TryRead(
        string wire,
        BracketNotationProfile profile,
        out NormalizedBracketWire? normalized,
        out string error);
}
