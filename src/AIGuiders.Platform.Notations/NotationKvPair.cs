#nullable enable

using ModelingNotations = AIGuiders.Platform.Modeling.Notations;

namespace AIGuiders.Platform.Notations;

/// <summary>
/// GUIDERS-FSHARP-ADR-0003 §4.4 cutover: KV wire atom SSOT via <see cref="ToModel"/> (Modeling.Notations.Core).
/// </summary>
public sealed record NotationKvPair(string Key, char Sign, string Value)
{
    /// <summary>Split on first <paramref name="sign"/> only (value may contain more signs).</summary>
    public static bool TrySplitFirst(string segment, char sign, out NotationKvPair pair, out string error)
    {
        pair = null!;
        error = "";
        var result = ModelingNotations.NotationKvPairModule.trySplitFirst(segment, sign);
        if (result.IsOk)
        {
            pair = FromModel(result.ResultValue);
            return true;
        }

        error = result.ErrorValue;
        return false;
    }

    public ModelingNotations.NotationKvPair ToModel() => new() { Key = Key, Sign = Sign, Value = Value };

    public static NotationKvPair FromModel(ModelingNotations.NotationKvPair model) =>
        new(model.Key, model.Sign, model.Value);
}
