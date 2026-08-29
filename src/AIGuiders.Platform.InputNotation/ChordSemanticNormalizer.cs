#nullable enable

namespace AIGuiders.Platform.InputNotation;

/// <summary>Shared key-symbol normalization for all notation surfaces.</summary>
public static class ChordSemanticNormalizer
{
    public static string NormalizeKeySymbol(string key)
    {
        if (key.Length == 1 && char.IsLetter(key[0]))
            return key.ToUpperInvariant();

        return key;
    }
}
