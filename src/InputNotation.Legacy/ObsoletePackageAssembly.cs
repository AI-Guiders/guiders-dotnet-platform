// Package-level obsoletion marker (GUIDERS-ADR-0021 §7).
// Type-level [Obsolete] aliases live in InputNotationLegacyAliases.cs.
namespace AIGuiders.Platform.InputNotation;

/// <summary>
/// Marker type for package obsoletion. Reference any legacy <c>InputNotation.*</c> package to receive CS0618.
/// </summary>
[Obsolete(
    "AIGuiders.Platform.InputNotation.* packages are obsolete. Migrate to AIGuiders.Platform.Notations.Keyboard.* — see docs/migration/inputnotation-to-notations-v1.md. Sunset date TBD.",
    error: false)]
public static class InputNotationPackageObsolete
{
}
