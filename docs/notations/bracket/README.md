# Notations.Bracket

Canonical plan: [GUIDERS-ADR-0026](../adr/GUIDERS-ADR-0026-notations-bracket-branch.md).

## Shipped (v0.20.0+)

- `NotationKvPair`, `NotationListSplit` in **`AIGuiders.Platform.Notations`**
- **`BracketReader`** in `AIGuiders.Platform.Notations.Bracket`
- Conformance **`notation/bracket-cdp-square-kv`**, **`notation/bracket-angle-opaque`**, **`notation/bracket-forge-frg`**
- **`BracketAnchorWire`** + conformance **`language-intelligence/anchor-resolve`**
- `BracketProfiles.ForgeFrg` compound profile (FRG path + CDP tail)
- `Argument.Kv` uses shared `NotationKvPair`
- **`guiders-core` `BracketLocate.Parse`** → delegates to `BracketReader` (project ref / NuGet 0.20.x)

## Remaining
- CIDE H1 profile (`bracket.cide-h1`)
- Pass 2 structured parse (path segments, inner KV objects)
