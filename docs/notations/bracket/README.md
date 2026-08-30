# Notations.Bracket

Canonical plan: [GUIDERS-ADR-0026](../adr/GUIDERS-ADR-0026-notations-bracket-branch.md).

## Shipped (v0.20.0)

- `NotationKvPair`, `NotationListSplit` in **`AIGuiders.Platform.Notations`**
- **`BracketReader`** in `AIGuiders.Platform.Notations.Bracket`
- Conformance **`notation/bracket-cdp-square-kv`**
- `Argument.Kv` uses shared `NotationKvPair`

## Remaining

- guiders-core `BracketLocate` → pin/consume `Notations.Bracket` (planet migration)
- CIDE H1 profile, Forge FRG compound tail
- Pass 2 structured parse (path segments, inner KV objects)
