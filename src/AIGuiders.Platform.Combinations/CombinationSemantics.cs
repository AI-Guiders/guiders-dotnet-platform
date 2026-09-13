#nullable enable

namespace AIGuiders.Platform.Combinations;

/// <summary>
/// GUIDERS-FSHARP-ADR-0003 §4.5 cutover: legacy C# name over <see cref="ModelingSemantics"/> DU (F# SSOT).
/// </summary>
public readonly struct CombinationSemantics : IEquatable<CombinationSemantics>
{
    readonly ModelingSemantics _model;

    CombinationSemantics(ModelingSemantics model) => _model = model;

    public static CombinationSemantics FieldOverlay => new(ModelingSemantics.FieldOverlay);

    public static CombinationSemantics SectionReplace => new(ModelingSemantics.SectionReplace);

    public static CombinationSemantics ShipFirst => new(ModelingSemantics.ShipFirst);

    public static CombinationSemantics OverlayWins => new(ModelingSemantics.OverlayWins);

    public ModelingSemantics ToModel() => _model;

    public static CombinationSemantics FromModel(ModelingSemantics model) => new(model);

    public static implicit operator ModelingSemantics(CombinationSemantics semantics) => semantics._model;

    public static implicit operator CombinationSemantics(ModelingSemantics model) => new(model);

    public bool Equals(CombinationSemantics other) => _model.Equals(other._model);

    public override bool Equals(object? obj) => obj is CombinationSemantics other && Equals(other);

    public override int GetHashCode() => _model.GetHashCode();

    public static bool operator ==(CombinationSemantics left, CombinationSemantics right) => left.Equals(right);

    public static bool operator !=(CombinationSemantics left, CombinationSemantics right) => !left.Equals(right);
}
