#nullable enable

namespace AIGuiders.Platform.Notations;

/// <summary>
/// Universal KV wire atom: <c>Key</c> + <c>Sign</c> + <c>Value</c> (GUIDERS-ADR-0021/0026).
/// </summary>
public sealed record NotationKvPair(string Key, char Sign, string Value)
{
    /// <summary>Split on first <paramref name="sign"/> only (value may contain more signs).</summary>
    public static bool TrySplitFirst(string segment, char sign, out NotationKvPair pair, out string error)
    {
        pair = null!;
        error = "";
        if (string.IsNullOrWhiteSpace(segment))
        {
            error = "Empty segment.";
            return false;
        }

        segment = segment.Trim();
        var index = segment.IndexOf(sign);
        if (index <= 0)
        {
            error = $"Missing KV sign '{sign}'.";
            return false;
        }

        pair = new NotationKvPair(segment[..index].Trim(), sign, segment[(index + 1)..].Trim());
        return true;
    }
}
